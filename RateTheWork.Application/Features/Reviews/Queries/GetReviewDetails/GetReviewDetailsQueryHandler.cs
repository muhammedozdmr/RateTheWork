using AutoMapper;
using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings.DTOs;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Queries.GetReviewDetails;

/// <summary>
/// Yorum detaylarını getiren query
/// </summary>
public record GetReviewDetailsQuery : IRequest<ReviewDetailDto>
{
    /// <summary>
    /// Detayları getirilecek yorumun ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
}

/// <summary>
/// GetReviewDetailsQuery handler
/// </summary>
public class GetReviewDetailsQueryHandler : IRequestHandler<GetReviewDetailsQuery, ReviewDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetReviewDetailsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<ReviewDetailDto> Handle(GetReviewDetailsQuery request, CancellationToken cancellationToken)
    {
        // 1. Yorumu getir
        var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId);
        
        if (review == null)
        {
            throw new NotFoundException("Yorum bulunamadı.");
        }

        // 2. Sadece aktif yorumlar görüntülenebilir (admin hariç)
        var isAdmin = _currentUserService.Roles.Contains("SuperAdmin") || 
                     _currentUserService.Roles.Contains("Moderator");
                     
        if (!review.IsActive && !isAdmin)
        {
            throw new NotFoundException("Bu yorum silinmiş veya gizlenmiştir.");
        }

        // 3. Entity'den DTO'ya map'le
        var reviewDetail = _mapper.Map<ReviewDetailDto>(review);

        // 4. Yorumu yapan kullanıcının bilgilerini getir
        var author = await _unitOfWork.Users.GetByIdAsync(review.UserId);
        if (author != null)
        {
            // Önce kullanıcı rozetlerini al
            var badgeList = new List<BadgeInfo>();
            var userBadges = await _unitOfWork.UserBadges.GetAsync(ub => ub.UserId == author.Id);
            if (userBadges.Any())
            {
                List<string?> badgeIds = userBadges.Select(ub => ub.BadgeId).ToList();
                var badges = await _unitOfWork.Badges.GetAsync(b => badgeIds.Contains(b.Id));
                badgeList = badges.Select(b => new BadgeInfo
                {
                    Name = b.Name,
                    IconUrl = b.IconUrl
                }).ToList();
            }

            // Tek seferde tüm author bilgilerini ata
            reviewDetail = reviewDetail with 
            { 
                AuthorUsername = author.AnonymousUsername,
                AuthorInfo = new ReviewAuthorInfo
                {
                    Username = author.AnonymousUsername,
                    Profession = author.Profession,
                    ReviewCount = await _unitOfWork.Reviews.GetCountAsync(r => r.UserId == author.Id && r.IsActive),
                    JoinDate = author.CreatedAt,
                    IsVerified = author.IsEmailVerified && author.IsPhoneVerified && author.IsTcIdentityVerified,
                    Badges = badgeList
                }
            };
        }

        // 5. Şirket bilgilerini getir
        var company = await _unitOfWork.Companies.GetByIdAsync(review.CompanyId);
        if (company != null)
        {
            reviewDetail = reviewDetail with 
            { 
                CompanyName = company.Name,
                CompanyInfo = new ReviewCompanyInfo
                {
                    CompanyId = company.Id,
                    Name = company.Name,
                    Sector = company.Sector,
                    LogoUrl = company.LogoUrl,
                    AverageRating = company.AverageRating,
                    TotalReviews = company.TotalReviews
                }
            };
        }

        // 6. Tam yorum metnini set et
        reviewDetail = reviewDetail with
        {
            FullComment = review.CommentText,
            CommentPreview = review.CommentText.Length > 200 
                ? review.CommentText.Substring(0, 200) + "..." 
                : review.CommentText
        };

        // 7. Mevcut kullanıcının bu yoruma verdiği oyu kontrol et
        if (_currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var userVote = await _unitOfWork.ReviewVotes
                .GetUserVoteForReviewAsync(_currentUserService.UserId, request.ReviewId);
            
            if (userVote != null)
            {
                reviewDetail = reviewDetail with 
                { 
                    CurrentUserVote = userVote.IsUpvote 
                };
            }

            // Kullanıcı bu yorumu şikayet etmiş mi?
            var userReport = await _unitOfWork.Reports.GetFirstOrDefaultAsync(r => 
                r.ReviewId == request.ReviewId && 
                r.ReporterUserId == _currentUserService.UserId);
                
            reviewDetail = reviewDetail with 
            { 
                HasUserReported = userReport != null 
            };

            // Kullanıcı yorum sahibi mi?
            reviewDetail = reviewDetail with 
            { 
                IsOwnReview = review.UserId == _currentUserService.UserId 
            };
        }

        // 8. Admin ise ekstra bilgileri ekle
        if (isAdmin)
        {
            // 1. Önce boş liste oluştur
            var reportDetailList = new List<ReportDetail>();
    
            // 2. Eğer şikayet varsa, detayları doldur
            if (review.ReportCount > 0)
            {
                var reports = await _unitOfWork.Reviews.GetReviewReportsAsync(review.Id);
                reportDetailList = reports.Select(r => new ReportDetail
                {
                    ReportId = r.Id, 
                    Reason = r.ReportReason, 
                    Details = r.ReportDetails, 
                    ReportedAt = r.ReportedAt,
                    Status = r.Status
                }).ToList();
            }
    
            // 3. Tek seferde tüm AdminInfo'yu ata (with expression ile)
            reviewDetail = reviewDetail with
            {
                AdminInfo = new ReviewAdminInfo
                {
                    ReportCount = review.ReportCount,
                    IsActive = review.IsActive,
                    CreatedBy = review.CreatedBy,
                    ModifiedBy = review.ModifiedBy,
                    ModifiedAt = review.ModifiedAt,
                    UserEmail = author?.Email ?? "Unknown",
                    ReportDetails = reportDetailList  // Önceden hazırladığım liste
                }
            };
        }

        return reviewDetail;
    }
}

/// <summary>
/// Yorum yazarı bilgileri
/// </summary>
public record ReviewAuthorInfo
{
    public string Username { get; init; } = string.Empty;
    public string Profession { get; init; } = string.Empty;
    public int ReviewCount { get; init; }
    public DateTime JoinDate { get; init; }
    public bool IsVerified { get; init; }
    public List<BadgeInfo> Badges { get; init; } = new();
}

/// <summary>
/// Rozet bilgisi
/// </summary>
public record BadgeInfo
{
    public string? Name { get; init; } = string.Empty;
    public string? IconUrl { get; init; } = string.Empty;
}

/// <summary>
/// Yorum şirket bilgileri
/// </summary>
public record ReviewCompanyInfo
{
    public string? CompanyId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Sector { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
}

/// <summary>
/// Admin için ekstra bilgiler
/// </summary>
public record ReviewAdminInfo
{
    public int ReportCount { get; init; }
    public bool IsActive { get; init; }
    public string? CreatedBy { get; init; }
    public string? ModifiedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string UserEmail { get; init; } = string.Empty;
    public List<ReportDetail> ReportDetails { get; init; } = new();
}

/// <summary>
/// Şikayet detayı
/// </summary>
public record ReportDetail
{
    public string? ReportId { get; init; } = string.Empty;
    public string? Reason { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTime ReportedAt { get; init; }
    public string? Status { get; init; } = string.Empty;
}

/// <summary>
/// GetReviewDetailsQuery için FluentValidation validator
/// </summary>
public class GetReviewDetailsQueryValidator : AbstractValidator<GetReviewDetailsQuery>
{
    public GetReviewDetailsQueryValidator()
    {
        RuleFor(x => x.ReviewId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen yorum listesine dönüp tekrar deneyiniz.")
            .Must(BeAValidGuid).WithMessage("Geçersiz yorum seçimi. Lütfen yorum listesinden bir yorum seçiniz.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
