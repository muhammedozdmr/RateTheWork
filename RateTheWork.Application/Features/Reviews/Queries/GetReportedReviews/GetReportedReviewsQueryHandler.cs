using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Queries.GetReportedReviews;

/// <summary>
/// Şikayet edilen yorumları getiren query (Admin için)
/// </summary>
public record GetReportedReviewsQuery : PaginatedRequest, IRequest<PagedList<ReportedReviewDto>>
{
    /// <summary>
    /// Şikayet durumu filtresi
    /// </summary>
    public string? Status { get; init; } // Pending, Reviewed, ActionTaken, Dismissed
    
    /// <summary>
    /// Minimum şikayet sayısı filtresi
    /// </summary>
    public int? MinReportCount { get; init; }
    
    /// <summary>
    /// Belirli bir şirket için mi?
    /// </summary>
    public string? CompanyId { get; init; }
    
    /// <summary>
    /// Tarih aralığı - başlangıç
    /// </summary>
    public DateTime? StartDate { get; init; }
    
    /// <summary>
    /// Tarih aralığı - bitiş
    /// </summary>
    public DateTime? EndDate { get; init; }
    
    /// <summary>
    /// Sıralama kriteri
    /// </summary>
    public string SortBy { get; init; } = "ReportCount"; // ReportCount, Date, LastReportDate
    
    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public bool IsDescending { get; init; } = true;
}

/// <summary>
/// Şikayet edilen yorum DTO'su
/// </summary>
public record ReportedReviewDto
{
    /// <summary>
    /// Yorum ID'si
    /// </summary>
    public string ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket bilgileri
    /// </summary>
    public CompanyInfo Company { get; init; } = new();
    
    /// <summary>
    /// Yorumu yapan kullanıcı bilgileri
    /// </summary>
    public ReviewerInfo Reviewer { get; init; } = new();
    
    /// <summary>
    /// Yorum detayları
    /// </summary>
    public ReviewContent Content { get; init; } = new();
    
    /// <summary>
    /// Şikayet bilgileri
    /// </summary>
    public ReportInfo Reports { get; init; } = new();
    
    /// <summary>
    /// Admin aksiyonları
    /// </summary>
    public AdminActions Actions { get; init; } = new();
}

/// <summary>
/// Şirket bilgileri
/// </summary>
public record CompanyInfo
{
    public string CompanyId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
}

/// <summary>
/// Yorumu yapan kullanıcı bilgileri
/// </summary>
public record ReviewerInfo
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int TotalReviews { get; init; }
    public int ReportedReviews { get; init; }
}

/// <summary>
/// Yorum içeriği
/// </summary>
public record ReviewContent
{
    public string CommentType { get; init; } = string.Empty;
    public decimal OverallRating { get; init; }
    public string CommentText { get; init; } = string.Empty;
    public DateTime PostedDate { get; init; }
    public bool IsActive { get; init; }
    public bool IsDocumentVerified { get; init; }
}

/// <summary>
/// Şikayet bilgileri
/// </summary>
public record ReportInfo
{
    public int TotalReportCount { get; init; }
    public DateTime? FirstReportDate { get; init; }
    public DateTime? LastReportDate { get; init; }
    public List<ReportDetail> ReportDetails { get; init; } = new();
    public Dictionary<string, int> ReportReasonBreakdown { get; init; } = new();
}

/// <summary>
/// Tekil şikayet detayı
/// </summary>
public record ReportDetail
{
    public string ReportId { get; init; } = string.Empty;
    public string ReporterUsername { get; init; } = string.Empty;
    public string ReportReason { get; init; } = string.Empty;
    public string? ReportDetails { get; init; }
    public DateTime ReportedAt { get; init; }
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Admin aksiyonları
/// </summary>
public record AdminActions
{
    public bool CanHide { get; init; }
    public bool CanDelete { get; init; }
    public bool CanBanUser { get; init; }
    public List<PreviousAction> PreviousActions { get; init; } = new();
}

/// <summary>
/// Önceki admin aksiyonu
/// </summary>
public record PreviousAction
{
    public string ActionType { get; init; } = string.Empty;
    public string AdminUsername { get; init; } = string.Empty;
    public DateTime ActionDate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// GetReportedReviewsQuery handler
/// </summary>
public class GetReportedReviewsQueryHandler : IRequestHandler<GetReportedReviewsQuery, PagedList<ReportedReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetReportedReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<ReportedReviewDto>> Handle(GetReportedReviewsQuery request, CancellationToken cancellationToken)
    {
        // 1. Yetki kontrolü - sadece admin erişebilir
        var isAdmin = _currentUserService.Roles.Contains("SuperAdmin") || 
                     _currentUserService.Roles.Contains("Moderator");
        
        if (!isAdmin)
        {
            throw new ForbiddenAccessException("Bu sayfaya erişim yetkiniz yok.");
        }

        // 2. Şikayet edilen yorumları getir
        var reportsQuery = _unitOfWork.Reports.GetQueryable();

        // 3. Durum filtresi
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            reportsQuery = reportsQuery.Where(r => r.Status == request.Status);
        }

        // 4. Tarih filtresi
        if (request.StartDate.HasValue)
        {
            reportsQuery = reportsQuery.Where(r => r.ReportedAt >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            var endOfDay = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            reportsQuery = reportsQuery.Where(r => r.ReportedAt <= endOfDay);
        }

        // 5. Yorumları grupla ve şikayet sayısını hesapla
        var reviewReportGroups = await reportsQuery
            .GroupBy(r => r.ReviewId)
            .Select(g => new
            {
                ReviewId = g.Key,
                ReportCount = g.Count(),
                FirstReportDate = g.Min(r => r.ReportedAt),
                LastReportDate = g.Max(r => r.ReportedAt),
                Reports = g.ToList()
            })
            .ToListAsync(cancellationToken);

        // 6. Minimum şikayet sayısı filtresi
        if (request.MinReportCount.HasValue)
        {
            reviewReportGroups = reviewReportGroups
                .Where(g => g.ReportCount >= request.MinReportCount.Value)
                .ToList();
        }

        // 7. Yorum bilgilerini getir
        var reviewIds = reviewReportGroups.Select(g => g.ReviewId).ToList();
        var reviews = await _unitOfWork.Reviews.GetAsync(r => reviewIds.Contains(r.Id));
        var reviewDict = reviews.ToDictionary(r => r.Id);

        // 8. Şirket filtresi
        if (!string.IsNullOrWhiteSpace(request.CompanyId))
        {
            reviewReportGroups = reviewReportGroups
                .Where(g => reviewDict.ContainsKey(g.ReviewId) && 
                           reviewDict[g.ReviewId].CompanyId == request.CompanyId)
                .ToList();
        }

        // 9. Sıralama
        reviewReportGroups = request.SortBy?.ToLower() switch
        {
            "date" => request.IsDescending 
                ? reviewReportGroups.OrderByDescending(g => reviewDict.ContainsKey(g.ReviewId) ? reviewDict[g.ReviewId].CreatedAt : DateTime.MinValue).ToList()
                : reviewReportGroups.OrderBy(g => reviewDict.ContainsKey(g.ReviewId) ? reviewDict[g.ReviewId].CreatedAt : DateTime.MinValue).ToList(),
            
            "lastreportdate" => request.IsDescending 
                ? reviewReportGroups.OrderByDescending(g => g.LastReportDate).ToList()
                : reviewReportGroups.OrderBy(g => g.LastReportDate).ToList(),
            
            _ => request.IsDescending // ReportCount
                ? reviewReportGroups.OrderByDescending(g => g.ReportCount).ThenByDescending(g => g.LastReportDate).ToList()
                : reviewReportGroups.OrderBy(g => g.ReportCount).ThenByDescending(g => g.LastReportDate).ToList()
        };

        // 10. Sayfalama
        var (pageNumber, pageSize) = request.GetValidatedPaginationParams();
        var totalCount = reviewReportGroups.Count;
        
        var pagedGroups = reviewReportGroups
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 11. İlişkili verileri toplu getir
        var userIds = new List<string>();
        var companyIds = new List<string>();
        var reporterIds = new List<string>();

        foreach (var group in pagedGroups)
        {
            if (reviewDict.ContainsKey(group.ReviewId))
            {
                var review = reviewDict[group.ReviewId];
                userIds.Add(review.UserId);
                companyIds.Add(review.CompanyId);
            }
            reporterIds.AddRange(group.Reports.Select(r => r.ReporterUserId));
        }

        var users = await _unitOfWork.Users.GetAsync(u => userIds.Distinct().Contains(u.Id));
        var userDict = users.ToDictionary(u => u.Id);

        var companies = await _unitOfWork.Companies.GetAsync(c => companyIds.Distinct().Contains(c.Id));
        var companyDict = companies.ToDictionary(c => c.Id);

        var reporters = await _unitOfWork.Users.GetAsync(u => reporterIds.Distinct().Contains(u.Id));
        var reporterDict = reporters.ToDictionary(u => u.Id);

        // 12. Kullanıcı istatistikleri
        var userReviewStats = await GetUserReviewStats(userIds.Distinct().ToList(), cancellationToken);

        // 13. DTO'ları oluştur
        var dtos = new List<ReportedReviewDto>();

        foreach (var group in pagedGroups)
        {
            if (!reviewDict.ContainsKey(group.ReviewId))
                continue;

            var review = reviewDict[group.ReviewId];
            var user = userDict.GetValueOrDefault(review.UserId);
            var company = companyDict.GetValueOrDefault(review.CompanyId);

            if (user == null || company == null)
                continue;

            // Şikayet nedenlerini grupla
            var reasonBreakdown = group.Reports
                .GroupBy(r => r.ReportReason)
                .ToDictionary(g => g.Key, g => g.Count());

            // Şikayet detayları
            var reportDetails = group.Reports
                .OrderByDescending(r => r.ReportedAt)
                .Take(10) // Son 10 şikayet
                .Select(report => new ReportDetail
                {
                    ReportId = report.Id,
                    ReporterUsername = reporterDict.TryGetValue(report.ReporterUserId, out var reporter) 
                        ? reporter.AnonymousUsername 
                        : "Bilinmeyen Kullanıcı",
                    ReportReason = report.ReportReason,
                    ReportDetails = report.ReportDetails,
                    ReportedAt = report.ReportedAt,
                    Status = report.Status
                })
                .ToList();

            // Kullanıcı istatistiklerini al
            var hasStats = userReviewStats.TryGetValue(user.Id, out var stats);
            var totalReviews = hasStats ? stats.TotalReviews : 0;
            var reportedReviews = hasStats ? stats.ReportedReviews : 0;

            var dto = new ReportedReviewDto
            {
                ReviewId = review.Id,
                Company = new CompanyInfo
                {
                    CompanyId = company.Id,
                    CompanyName = company.Name
                },
                Reviewer = new ReviewerInfo
                {
                    UserId = user.Id,
                    Username = user.AnonymousUsername,
                    Email = user.Email, // Admin görebilir
                    TotalReviews = totalReviews,
                    ReportedReviews = reportedReviews
                },
                Content = new ReviewContent
                {
                    CommentType = review.CommentType,
                    OverallRating = review.OverallRating,
                    CommentText = review.CommentText,
                    PostedDate = review.CreatedAt,
                    IsActive = review.IsActive,
                    IsDocumentVerified = review.IsDocumentVerified
                },
                Reports = new ReportInfo
                {
                    TotalReportCount = group.ReportCount,
                    FirstReportDate = group.FirstReportDate,
                    LastReportDate = group.LastReportDate,
                    ReportDetails = reportDetails,
                    ReportReasonBreakdown = reasonBreakdown
                },
                Actions = new AdminActions
                {
                    CanHide = review.IsActive,
                    CanDelete = true,
                    CanBanUser = !user.IsBanned,
                    PreviousActions = new List<PreviousAction>() // TODO: Admin log tablosundan çekilebilir
                }
            };

            dtos.Add(dto);
        }

        return new PagedList<ReportedReviewDto>(dtos, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Kullanıcıların yorum istatistiklerini getirir
    /// </summary>
    private async Task<Dictionary<string, (int TotalReviews, int ReportedReviews)>> GetUserReviewStats(
        List<string> userIds, 
        CancellationToken cancellationToken)
    {
        var stats = new Dictionary<string, (int TotalReviews, int ReportedReviews)>();

        var userReviewCounts = await _unitOfWork.Reviews.GetQueryable()
            .Where(r => userIds.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalCount = g.Count(),
                ReportedCount = g.Count(r => r.ReportCount > 0)
            })
            .ToListAsync(cancellationToken);

        foreach (var stat in userReviewCounts)
        {
            stats[stat.UserId] = (TotalReviews: stat.TotalCount, ReportedReviews: stat.ReportedCount);
        }

        return stats;
    }
}

/// <summary>
/// GetReportedReviewsQuery validator
/// </summary>
public class GetReportedReviewsQueryValidator : AbstractValidator<GetReportedReviewsQuery>
{
    public GetReportedReviewsQueryValidator()
    {
        // Status
        When(x => !string.IsNullOrWhiteSpace(x.Status), () =>
        {
            RuleFor(x => x.Status!)
                .Must(BeValidStatus).WithMessage("Geçersiz şikayet durumu.");
        });

        // MinReportCount
        When(x => x.MinReportCount.HasValue, () =>
        {
            RuleFor(x => x.MinReportCount!.Value)
                .GreaterThan(0).WithMessage("Minimum şikayet sayısı 0'dan büyük olmalıdır.");
        });

        // CompanyId
        When(x => !string.IsNullOrWhiteSpace(x.CompanyId), () =>
        {
            RuleFor(x => x.CompanyId!)
                .Must(BeAValidGuid).WithMessage("Geçersiz şirket ID'si.");
        });

        // Tarih aralığı
        When(x => x.StartDate.HasValue && x.EndDate.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.StartDate!.Value <= x.EndDate!.Value)
                .WithMessage("Başlangıç tarihi, bitiş tarihinden sonra olamaz.");
        });

        // Sıralama
        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Geçersiz sıralama kriteri.");

        // Sayfalama
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası geçersiz.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50).WithMessage("Sayfa başına en fazla 50 kayıt gösterilebilir.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidStatus(string status)
    {
        var validStatuses = new[] { "Pending", "Reviewed", "ActionTaken", "Dismissed" };
        return validStatuses.Contains(status);
    }

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "reportcount", "date", "lastreportdate" };
        return validFields.Contains(sortBy.ToLower());
    }
}