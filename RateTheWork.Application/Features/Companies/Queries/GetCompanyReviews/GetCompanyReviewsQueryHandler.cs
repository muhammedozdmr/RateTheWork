using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Queries.GetCompanyReviews;

/// <summary>
/// Şirket yorumlarını getiren query
/// </summary>
public record GetCompanyReviewsQuery : PaginatedRequest, IRequest<PagedList<CompanyReviewDto>>
{
    /// <summary>
    /// Yorumları getirilecek şirketin ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum türü filtresi (opsiyonel)
    /// </summary>
    public string? CommentType { get; init; }
    
    /// <summary>
    /// Minimum puan filtresi (opsiyonel)
    /// </summary>
    public decimal? MinRating { get; init; }
    
    /// <summary>
    /// Sadece doğrulanmış yorumlar mı?
    /// </summary>
    public bool? VerifiedOnly { get; init; }
    
    /// <summary>
    /// Sıralama kriteri
    /// </summary>
    public string SortBy { get; init; } = "Date"; // Date, Rating, Votes
    
    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public bool IsDescending { get; init; } = true;
}

/// <summary>
/// Şirket yorumu DTO'su
/// </summary>
public record CompanyReviewDto
{
    /// <summary>
    /// Yorum ID'si
    /// </summary>
    public string ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorumu yapan kullanıcının anonim adı
    /// </summary>
    public string AuthorUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum türü
    /// </summary>
    public string CommentType { get; init; } = string.Empty;
    
    /// <summary>
    /// Verilen puan
    /// </summary>
    public decimal OverallRating { get; init; }
    
    /// <summary>
    /// Yorum metni
    /// </summary>
    public string CommentText { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum tarihi
    /// </summary>
    public DateTime PostedDate { get; init; }
    
    /// <summary>
    /// Upvote sayısı
    /// </summary>
    public int Upvotes { get; init; }
    
    /// <summary>
    /// Downvote sayısı
    /// </summary>
    public int Downvotes { get; init; }
    
    /// <summary>
    /// Net oy (upvotes - downvotes)
    /// </summary>
    public int NetVotes { get; init; }
    
    /// <summary>
    /// Belge ile doğrulanmış mı?
    /// </summary>
    public bool IsVerified { get; init; }
    
    /// <summary>
    /// Kullanıcının bu yoruma verdiği oy (null: oy yok, true: upvote, false: downvote)
    /// </summary>
    public bool? CurrentUserVote { get; init; }
    
    /// <summary>
    /// Yorum güncellenmiş mi?
    /// </summary>
    public bool IsEdited { get; init; }
}

/// <summary>
/// GetCompanyReviews query handler
/// </summary>
public class GetCompanyReviewsQueryHandler : IRequestHandler<GetCompanyReviewsQuery, PagedList<CompanyReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<CompanyReviewDto>> Handle(GetCompanyReviewsQuery request, CancellationToken cancellationToken)
    {
        // 1. Şirket var mı ve onaylı mı?
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        if (company == null || !company.IsApproved)
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        // 2. Temel query - sadece aktif yorumlar
        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.CompanyId == request.CompanyId && r.IsActive);

        // 3. Yorum türü filtresi
        if (!string.IsNullOrWhiteSpace(request.CommentType))
        {
            query = query.Where(r => r.CommentType == request.CommentType);
        }

        // 4. Minimum puan filtresi
        if (request.MinRating.HasValue && request.MinRating > 0)
        {
            query = query.Where(r => r.OverallRating >= request.MinRating.Value);
        }

        // 5. Doğrulanmış yorumlar filtresi
        if (request.VerifiedOnly.HasValue && request.VerifiedOnly.Value)
        {
            query = query.Where(r => r.IsDocumentVerified);
        }

        // 6. Sıralama
        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.IsDescending 
                ? query.OrderByDescending(r => r.OverallRating).ThenByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.OverallRating).ThenByDescending(r => r.CreatedAt),
            
            "votes" => request.IsDescending 
                ? query.OrderByDescending(r => r.Upvotes - r.Downvotes).ThenByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.Upvotes - r.Downvotes).ThenByDescending(r => r.CreatedAt),
            
            _ => request.IsDescending 
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt)
        };

        // 7. Sayfalama
        var (pageNumber, pageSize) = request.GetValidatedPaginationParams();
        var totalCount = await query.CountAsync(cancellationToken);

        // 8. Veriyi getir ve map'le
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // 9. Kullanıcı bilgilerini ve oyları ekle
        var reviewIds = reviews.Select(r => r.Id).ToList();
        var userIds = reviews.Select(r => r.UserId).Distinct().ToList();
        
        // Kullanıcıları toplu getir (N+1 problemi önleme)
        var users = await _unitOfWork.Users.GetAsync(u => userIds.Contains(u.Id));
        var userDict = users.ToDictionary(u => u.Id);

        // Mevcut kullanıcının oylarını getir
        Dictionary<string, bool>? userVotes = null;
        if (_currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
        {
            userVotes = await _unitOfWork.ReviewVotes
                .GetUserVotesForReviewsAsync(_currentUserService.UserId, reviewIds);
        }

        // 10. DTO'ya dönüştür
        var reviewDtos = reviews.Select(review => new CompanyReviewDto
        {
            ReviewId = review.Id,
            AuthorUsername = userDict.TryGetValue(review.UserId, out var user) 
                ? user.AnonymousUsername 
                : "Anonim Kullanıcı",
            CommentType = review.CommentType,
            OverallRating = review.OverallRating,
            CommentText = review.CommentText,
            PostedDate = review.CreatedAt,
            Upvotes = review.Upvotes,
            Downvotes = review.Downvotes,
            NetVotes = review.Upvotes - review.Downvotes,
            IsVerified = review.IsDocumentVerified,
            CurrentUserVote = userVotes?.TryGetValue(review.Id, out var vote) == true ? vote : null,
            IsEdited = review.ModifiedAt.HasValue
        }).ToList();

        return new PagedList<CompanyReviewDto>(reviewDtos, totalCount, pageNumber, pageSize);
    }
}

/// <summary>
/// GetCompanyReviews validation kuralları
/// </summary>
public class GetCompanyReviewsQueryValidator : AbstractValidator<GetCompanyReviewsQuery>
{
    public GetCompanyReviewsQueryValidator()
    {
        // Şirket ID
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Lütfen bir şirket seçiniz.")
            .Must(BeAValidGuid).WithMessage("Geçersiz şirket seçimi.");

        // Yorum türü
        When(x => !string.IsNullOrWhiteSpace(x.CommentType), () =>
        {
            RuleFor(x => x.CommentType)
                .Must(BeValidCommentType).WithMessage("Geçersiz yorum kategorisi.");
        });

        // Minimum puan
        When(x => x.MinRating.HasValue, () =>
        {
            RuleFor(x => x.MinRating!.Value)
                .InclusiveBetween(1, 5).WithMessage("Minimum puan 1-5 arasında olmalıdır.");
        });

        // Sıralama
        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Geçersiz sıralama kriteri.");

        // Sayfalama
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası geçersiz.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Sayfa başına en fazla 50 yorum gösterilebilir.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidCommentType(string? commentType)
    {
        return commentType != null && Domain.Enums.CommentTypes.IsValid(commentType);
    }

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "date", "rating", "votes" };
        return validFields.Contains(sortBy.ToLower());
    }
}
