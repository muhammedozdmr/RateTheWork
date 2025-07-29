using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;

namespace RateTheWork.Application.Features.Reviews.Commands.VoteReview;

/// <summary>
/// Yorum oylama komutu
/// </summary>
public record VoteReviewCommand : IRequest<VoteReviewResult>
{
    /// <summary>
    /// Oylanacak yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Oy türü (true: upvote, false: downvote)
    /// </summary>
    public bool IsUpvote { get; init; }
}

/// <summary>
/// Oylama sonucu
/// </summary>
public record VoteReviewResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// İşlem türü (Voted, Removed, Changed)
    /// </summary>
    public string Action { get; init; } = string.Empty;
    
    /// <summary>
    /// Güncel upvote sayısı
    /// </summary>
    public int CurrentUpvotes { get; init; }
    
    /// <summary>
    /// Güncel downvote sayısı
    /// </summary>
    public int CurrentDownvotes { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// VoteReview command handler
/// </summary>
public class VoteReviewCommandHandler : IRequestHandler<VoteReviewCommand, VoteReviewResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public VoteReviewCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<VoteReviewResult> Handle(VoteReviewCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Oy vermek için giriş yapmalısınız.");
        }

        // 2. Yorum var mı ve aktif mi?
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        if (review == null || !review.IsActive)
        {
            throw new NotFoundException("Review", request.ReviewId);
        }

        // 3. Kullanıcı kendi yorumuna oy veremez
        if (review.UserId == _currentUserService.UserId)
        {
            throw new BusinessRuleException("SELF_VOTE", "Kendi yorumunuza oy veremezsiniz.");
        }

        // 4. Kullanıcının mevcut oyunu kontrol et
        var existingVote = await _unitOfWork.ReviewVotes
            .GetUserVoteForReviewAsync(_currentUserService.UserId, request.ReviewId);

        string action;
        string message;

        if (existingVote == null)
        {
            // 5a. İlk kez oy veriyor
            var newVote = ReviewVote.Create(
                userId: _currentUserService.UserId!,
                reviewId: request.ReviewId,
                isUpvote: request.IsUpvote
            );

            await _unitOfWork.ReviewVotes.AddAsync(newVote);
            
            action = "Voted";
            message = request.IsUpvote 
                ? "Yorumu faydalı buldunuz." 
                : "Yorumu faydasız buldunuz.";
        }
        else if (existingVote.IsUpvote == request.IsUpvote)
        {
            // 5b. Aynı oyu tekrar veriyor - oyu kaldır
            _unitOfWork.ReviewVotes.Delete(existingVote);
            
            action = "Removed";
            message = "Oyunuz kaldırıldı.";
        }
        else
        {
            // 5c. Farklı oy veriyor - oyu değiştir
            existingVote.IsUpvote = request.IsUpvote;
            _unitOfWork.ReviewVotes.Update(existingVote);
            
            action = "Changed";
            message = request.IsUpvote 
                ? "Oyunuz 'faydalı' olarak değiştirildi." 
                : "Oyunuz 'faydasız' olarak değiştirildi.";
        }

        // 6. Review'daki oy sayılarını güncelle
        await _unitOfWork.Reviews.UpdateReviewVoteCountsAsync(request.ReviewId);
        
        // 7. Değişiklikleri kaydet
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Güncel oy sayılarını al
        var updatedReview = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        
        // 9. Rozet kontrolü - 10 upvote aldıysa
        if (updatedReview!.Upvotes >= 10 && updatedReview.Upvotes - updatedReview.Downvotes == 10)
        {
            // TODO: Award helpful reviewer badge
            // await _mediator.Send(new AwardBadgeCommand { UserId = review.UserId, BadgeType = "HelpfulReviewer" });
        }

        return new VoteReviewResult
        {
            Success = true,
            Action = action,
            CurrentUpvotes = updatedReview.Upvotes,
            CurrentDownvotes = updatedReview.Downvotes,
            Message = message
        };
    }
}

/// <summary>
/// VoteReview validation kuralları
/// </summary>
public class VoteReviewCommandValidator : AbstractValidator<VoteReviewCommand>
{
    public VoteReviewCommandValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen yorum listesine dönüp tekrar deneyin.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}