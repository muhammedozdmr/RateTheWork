using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Queries.GetReviewStatistics;

/// <summary>
/// Yorum istatistiklerini getiren query
/// </summary>
public record GetReviewStatisticsQuery : IRequest<ReviewStatisticsDto>
{
    /// <summary>
    /// İstatistik periyodu (opsiyonel)
    /// </summary>
    public StatisticsPeriod Period { get; init; } = StatisticsPeriod.AllTime;
    
    /// <summary>
    /// Belirli bir şirket için mi? (opsiyonel)
    /// </summary>
    public string? CompanyId { get; init; }
    
    /// <summary>
    /// Belirli bir kullanıcı için mi? (opsiyonel)
    /// </summary>
    public string? UserId { get; init; }
}

/// <summary>
/// İstatistik periyodu
/// </summary>
public enum StatisticsPeriod
{
    Today,
    ThisWeek,
    ThisMonth,
    ThisYear,
    AllTime
}

/// <summary>
/// Yorum istatistikleri DTO'su
/// </summary>
public record ReviewStatisticsDto
{
    /// <summary>
    /// Toplam yorum sayısı
    /// </summary>
    public int TotalReviews { get; init; }
    
    /// <summary>
    /// Doğrulanmış yorum sayısı
    /// </summary>
    public int VerifiedReviews { get; init; }
    
    /// <summary>
    /// Doğrulama yüzdesi
    /// </summary>
    public double VerificationPercentage { get; init; }
    
    /// <summary>
    /// Ortalama puan
    /// </summary>
    public decimal AverageRating { get; init; }
    
    /// <summary>
    /// Toplam oy sayısı (upvote + downvote)
    /// </summary>
    public int TotalVotes { get; init; }
    
    /// <summary>
    /// Ortalama yorum uzunluğu (karakter)
    /// </summary>
    public double AverageCommentLength { get; init; }
    
    /// <summary>
    /// Puan dağılımı (1-5 yıldız)
    /// </summary>
    public Dictionary<int, RatingBreakdown> RatingDistribution { get; init; } = new();
    
    /// <summary>
    /// Yorum türü dağılımı
    /// </summary>
    public Dictionary<string, CommentTypeBreakdown> CommentTypeDistribution { get; init; } = new();
    
    /// <summary>
    /// Aylık trend (son 12 ay)
    /// </summary>
    public List<MonthlyTrend> MonthlyTrends { get; init; } = new();
    
    /// <summary>
    /// En aktif şirketler (genel istatistik ise)
    /// </summary>
    public List<TopCompany>? TopCompanies { get; init; }
    
    /// <summary>
    /// En aktif kullanıcılar (admin görüntüleyebilir)
    /// </summary>
    public List<TopReviewer>? TopReviewers { get; init; }
}

/// <summary>
/// Puan dağılımı detayı
/// </summary>
public record RatingBreakdown
{
    public int Rating { get; init; }
    public int Count { get; init; }
    public double Percentage { get; init; }
}

/// <summary>
/// Yorum türü dağılımı detayı
/// </summary>
public record CommentTypeBreakdown
{
    public string CommentType { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percentage { get; init; }
    public decimal AverageRating { get; init; }
}

/// <summary>
/// Aylık trend verisi
/// </summary>
public record MonthlyTrend
{
    public int Year { get; init; }
    public int Month { get; init; }
    public string MonthName { get; init; } = string.Empty;
    public int ReviewCount { get; init; }
    public decimal AverageRating { get; init; }
}

/// <summary>
/// En çok yorum alan şirketler
/// </summary>
public record TopCompany
{
    public string CompanyId { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public int ReviewCount { get; init; }
    public decimal AverageRating { get; init; }
}

/// <summary>
/// En aktif yorumcular
/// </summary>
public record TopReviewer
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public int ReviewCount { get; init; }
    public decimal AverageRating { get; init; }
    public int VerifiedCount { get; init; }
}

/// <summary>
/// GetReviewStatisticsQuery handler
/// </summary>
public class GetReviewStatisticsQueryHandler : IRequestHandler<GetReviewStatisticsQuery, ReviewStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetReviewStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ReviewStatisticsDto> Handle(GetReviewStatisticsQuery request, CancellationToken cancellationToken)
    {
        // 1. Temel query
        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.IsActive);

        // 2. Şirket filtresi
        if (!string.IsNullOrEmpty(request.CompanyId))
        {
            query = query.Where(r => r.CompanyId == request.CompanyId);
        }

        // 3. Kullanıcı filtresi
        if (!string.IsNullOrEmpty(request.UserId))
        {
            query = query.Where(r => r.UserId == request.UserId);
        }

        // 4. Tarih filtresi
        var startDate = GetStartDate(request.Period);
        if (startDate.HasValue)
        {
            query = query.Where(r => r.CreatedAt >= startDate.Value);
        }

        // 5. Temel istatistikler
        var reviews = await query.ToListAsync(cancellationToken);
        
        var totalReviews = reviews.Count;
        var verifiedReviews = reviews.Count(r => r.IsDocumentVerified);
        var verificationPercentage = totalReviews > 0 ? (verifiedReviews * 100.0 / totalReviews) : 0;
        var averageRating = totalReviews > 0 ? reviews.Average(r => r.OverallRating) : 0;
        var totalVotes = reviews.Sum(r => r.Upvotes + r.Downvotes);
        var averageCommentLength = totalReviews > 0 ? reviews.Average(r => r.CommentText.Length) : 0;

        // 6. Puan dağılımı
        var ratingDistribution = new Dictionary<int, RatingBreakdown>();
        for (int i = 1; i <= 5; i++)
        {
            var count = reviews.Count(r => Math.Floor(r.OverallRating) == i);
            ratingDistribution[i] = new RatingBreakdown
            {
                Rating = i,
                Count = count,
                Percentage = totalReviews > 0 ? (count * 100.0 / totalReviews) : 0
            };
        }

        // 7. Yorum türü dağılımı
        var commentTypeGroups = reviews.GroupBy(r => r.CommentType)
            .Select(g => new CommentTypeBreakdown
            {
                CommentType = g.Key.ToString(),
                Count = g.Count(),
                Percentage = totalReviews > 0 ? (g.Count() * 100.0 / totalReviews) : 0,
                AverageRating = g.Average(r => r.OverallRating)
            })
            .ToDictionary(c => c.CommentType);

        // 8. Aylık trendler (son 12 ay)
        var monthlyTrends = await GetMonthlyTrends(request.CompanyId, request.UserId, cancellationToken);

        // 9. En aktif şirketler (sadece genel istatistiklerde ve company filtresi yoksa)
        List<TopCompany>? topCompanies = null;
        if (string.IsNullOrEmpty(request.CompanyId) && string.IsNullOrEmpty(request.UserId))
        {
            topCompanies = await GetTopCompanies(startDate, cancellationToken);
        }

        // 10. En aktif kullanıcılar (sadece admin ve user filtresi yoksa)
        List<TopReviewer>? topReviewers = null;
        var isAdmin = _currentUserService.Roles.Contains("SuperAdmin") || 
                     _currentUserService.Roles.Contains("Moderator");
        
        if (isAdmin && string.IsNullOrEmpty(request.UserId))
        {
            topReviewers = await GetTopReviewers(request.CompanyId, startDate, cancellationToken);
        }

        return new ReviewStatisticsDto
        {
            TotalReviews = totalReviews,
            VerifiedReviews = verifiedReviews,
            VerificationPercentage = verificationPercentage,
            AverageRating = averageRating,
            TotalVotes = totalVotes,
            AverageCommentLength = averageCommentLength,
            RatingDistribution = ratingDistribution,
            CommentTypeDistribution = commentTypeGroups,
            MonthlyTrends = monthlyTrends,
            TopCompanies = topCompanies,
            TopReviewers = topReviewers
        };
    }

    private DateTime? GetStartDate(StatisticsPeriod period)
    {
        var now = DateTime.UtcNow;
        return period switch
        {
            StatisticsPeriod.Today => now.Date,
            StatisticsPeriod.ThisWeek => now.AddDays(-(int)now.DayOfWeek),
            StatisticsPeriod.ThisMonth => new DateTime(now.Year, now.Month, 1),
            StatisticsPeriod.ThisYear => new DateTime(now.Year, 1, 1),
            _ => null
        };
    }

    private async Task<List<MonthlyTrend>> GetMonthlyTrends(string? companyId, string? userId, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddMonths(-11).Date;
        var endDate = DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);

        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.IsActive && r.CreatedAt >= startDate && r.CreatedAt <= endDate);

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(r => r.CompanyId == companyId);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        var monthlyData = await query
            .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count(),
                AverageRating = g.Average(r => r.OverallRating)
            })
            .ToListAsync(cancellationToken);

        var trends = new List<MonthlyTrend>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var data = monthlyData.FirstOrDefault(m => m.Year == currentDate.Year && m.Month == currentDate.Month);
            trends.Add(new MonthlyTrend
            {
                Year = currentDate.Year,
                Month = currentDate.Month,
                MonthName = currentDate.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR")),
                ReviewCount = data?.Count ?? 0,
                AverageRating = data?.AverageRating ?? 0
            });
            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private async Task<List<TopCompany>> GetTopCompanies(DateTime? startDate, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.IsActive);

        if (startDate.HasValue)
            query = query.Where(r => r.CreatedAt >= startDate.Value);

        var companyStats = await query
            .GroupBy(r => r.CompanyId)
            .Select(g => new
            {
                CompanyId = g.Key,
                Count = g.Count(),
                AverageRating = g.Average(r => r.OverallRating)
            })
            .OrderByDescending(c => c.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Şirket bilgilerini getir
        var companyIds = companyStats.Select(c => c.CompanyId).ToList();
        var companies = await _unitOfWork.Companies.GetAsync(c => companyIds.Contains(c.Id));
        var companyDict = companies.ToDictionary(c => c.Id);

        return companyStats
            .Where(stat => companyDict.ContainsKey(stat.CompanyId))
            .Select(stat => new TopCompany
            {
                CompanyId = stat.CompanyId,
                CompanyName = companyDict[stat.CompanyId].Name,
                ReviewCount = stat.Count,
                AverageRating = stat.AverageRating
            })
            .ToList();
    }

    private async Task<List<TopReviewer>> GetTopReviewers(string? companyId, DateTime? startDate, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.IsActive);

        if (!string.IsNullOrEmpty(companyId))
            query = query.Where(r => r.CompanyId == companyId);

        if (startDate.HasValue)
            query = query.Where(r => r.CreatedAt >= startDate.Value);

        var userStats = await query
            .GroupBy(r => r.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count(),
                AverageRating = g.Average(r => r.OverallRating),
                VerifiedCount = g.Count(r => r.IsDocumentVerified)
            })
            .OrderByDescending(u => u.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Kullanıcı bilgilerini getir
        var userIds = userStats.Select(u => u.UserId).ToList();
        var users = await _unitOfWork.Users.GetAsync(u => userIds.Contains(u.Id));
        var userDict = users.ToDictionary(u => u.Id);

        return userStats
            .Where(stat => userDict.ContainsKey(stat.UserId))
            .Select(stat => new TopReviewer
            {
                UserId = stat.UserId,
                Username = userDict[stat.UserId].AnonymousUsername,
                ReviewCount = stat.Count,
                AverageRating = stat.AverageRating,
                VerifiedCount = stat.VerifiedCount
            })
            .ToList();
    }
}

/// <summary>
/// GetReviewStatisticsQuery validator
/// </summary>
public class GetReviewStatisticsQueryValidator : AbstractValidator<GetReviewStatisticsQuery>
{
    public GetReviewStatisticsQueryValidator()
    {
        // Period
        RuleFor(x => x.Period)
            .IsInEnum().WithMessage("Geçersiz istatistik periyodu.");

        // CompanyId
        When(x => !string.IsNullOrWhiteSpace(x.CompanyId), () =>
        {
            RuleFor(x => x.CompanyId!)
                .Must(BeAValidGuid).WithMessage("Geçersiz şirket ID'si.");
        });

        // UserId
        When(x => !string.IsNullOrWhiteSpace(x.UserId), () =>
        {
            RuleFor(x => x.UserId!)
                .Must(BeAValidGuid).WithMessage("Geçersiz kullanıcı ID'si.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
