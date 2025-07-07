using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Queries.SearchReviews;

/// <summary>
/// Yorum arama query'si
/// </summary>
public record SearchReviewsQuery : PaginatedRequest, IRequest<PagedList<SearchReviewDto>>
{
    /// <summary>
    /// Arama terimi (yorum metni içinde arama)
    /// </summary>
    public string? SearchTerm { get; init; }
    
    /// <summary>
    /// Şirket adı filtresi
    /// </summary>
    public string? CompanyName { get; init; }
    
    /// <summary>
    /// Yorum türü filtresi
    /// </summary>
    public string? CommentType { get; init; }
    
    /// <summary>
    /// Minimum puan filtresi
    /// </summary>
    public decimal? MinRating { get; init; }
    
    /// <summary>
    /// Maximum puan filtresi
    /// </summary>
    public decimal? MaxRating { get; init; }
    
    /// <summary>
    /// Başlangıç tarihi filtresi
    /// </summary>
    public DateTime? StartDate { get; init; }
    
    /// <summary>
    /// Bitiş tarihi filtresi
    /// </summary>
    public DateTime? EndDate { get; init; }
    
    /// <summary>
    /// Sadece doğrulanmış yorumlar mı?
    /// </summary>
    public bool? VerifiedOnly { get; init; }
    
    /// <summary>
    /// Sıralama kriteri
    /// </summary>
    public string SortBy { get; init; } = "Relevance"; // Relevance, Date, Rating, Votes
    
    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public bool IsDescending { get; init; } = true;
}

/// <summary>
/// Arama sonucu yorum DTO'su
/// </summary>
public record SearchReviewDto
{
    /// <summary>
    /// Yorum ID'si
    /// </summary>
    public string? ReviewId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket adı
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket logosu
    /// </summary>
    public string? CompanyLogoUrl { get; init; }
    
    /// <summary>
    /// Yorumu yapan kullanıcının anonim adı
    /// </summary>
    public string AuthorUsername { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum türü
    /// </summary>
    public string? CommentType { get; init; } = string.Empty;
    
    /// <summary>
    /// Verilen puan
    /// </summary>
    public decimal OverallRating { get; init; }
    
    /// <summary>
    /// Yorum metni (özet - ilk 200 karakter)
    /// </summary>
    public string? CommentSummary { get; init; } = string.Empty;
    
    /// <summary>
    /// Yorum tarihi
    /// </summary>
    public DateTime PostedDate { get; init; }
    
    /// <summary>
    /// Net oy (upvotes - downvotes)
    /// </summary>
    public int NetVotes { get; init; }
    
    /// <summary>
    /// Belge ile doğrulanmış mı?
    /// </summary>
    public bool IsVerified { get; init; }
    
    /// <summary>
    /// Arama terimi ile eşleşme skoru (relevance için)
    /// </summary>
    public double? MatchScore { get; init; }
}

/// <summary>
/// SearchReviewsQuery handler
/// </summary>
public class SearchReviewsQueryHandler : IRequestHandler<SearchReviewsQuery, PagedList<SearchReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public SearchReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<SearchReviewDto>> Handle(SearchReviewsQuery request, CancellationToken cancellationToken)
    {
        // 1. Temel query - sadece aktif yorumlar ve onaylı şirketler
        var query = from review in _unitOfWork.Reviews.GetQueryable()
                   join company in _unitOfWork.Companies.GetQueryable() 
                       on review.CompanyId equals company.Id
                   where review.IsActive && company.IsApproved
                   select new { review, company };

        // 2. Arama terimi filtresi
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTermLower = request.SearchTerm.ToLower();
            query = query.Where(x => 
                x.review.CommentText.ToLower().Contains(searchTermLower) ||
                x.company.Name.ToLower().Contains(searchTermLower));
        }

        // 3. Şirket adı filtresi
        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var companyNameLower = request.CompanyName.ToLower();
            query = query.Where(x => x.company.Name.ToLower().Contains(companyNameLower));
        }

        // 4. Yorum türü filtresi
        if (!string.IsNullOrWhiteSpace(request.CommentType))
        {
            query = query.Where(x => x.review.CommentType == request.CommentType);
        }

        // 5. Puan aralığı filtresi
        if (request.MinRating.HasValue)
        {
            query = query.Where(x => x.review.OverallRating >= request.MinRating.Value);
        }
        
        if (request.MaxRating.HasValue)
        {
            query = query.Where(x => x.review.OverallRating <= request.MaxRating.Value);
        }

        // 6. Tarih aralığı filtresi
        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.review.CreatedAt >= request.StartDate.Value);
        }
        
        if (request.EndDate.HasValue)
        {
            // Gün sonuna kadar dahil etmek için
            var endOfDay = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.review.CreatedAt <= endOfDay);
        }

        // 7. Doğrulanmış yorumlar filtresi
        if (request.VerifiedOnly.HasValue && request.VerifiedOnly.Value)
        {
            query = query.Where(x => x.review.IsDocumentVerified);
        }

        // 8. Sıralama
        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.IsDescending 
                ? query.OrderByDescending(x => x.review.OverallRating).ThenByDescending(x => x.review.CreatedAt)
                : query.OrderBy(x => x.review.OverallRating).ThenByDescending(x => x.review.CreatedAt),
            
            "votes" => request.IsDescending 
                ? query.OrderByDescending(x => x.review.Upvotes - x.review.Downvotes).ThenByDescending(x => x.review.CreatedAt)
                : query.OrderBy(x => x.review.Upvotes - x.review.Downvotes).ThenByDescending(x => x.review.CreatedAt),
            
            "date" => request.IsDescending 
                ? query.OrderByDescending(x => x.review.CreatedAt)
                : query.OrderBy(x => x.review.CreatedAt),
            
            _ => request.IsDescending // Relevance - en yeni ve popüler olanlar önce
                ? query.OrderByDescending(x => x.review.Upvotes - x.review.Downvotes)
                       .ThenByDescending(x => x.review.CreatedAt)
                : query.OrderBy(x => x.review.Upvotes - x.review.Downvotes)
                       .ThenByDescending(x => x.review.CreatedAt)
        };

        // 9. Sayfalama
        var (pageNumber, pageSize) = request.GetValidatedPaginationParams();
        var totalCount = await query.CountAsync(cancellationToken);

        // 10. Veriyi getir
        var results = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // 11. Kullanıcı bilgilerini toplu getir
        var userIds = results.Select(r => r.review.UserId).Distinct().ToList();
        var users = await _unitOfWork.Users.GetAsync(u => userIds.Contains(u.Id));
        var userDict = users.ToDictionary(u => u.Id);

        // 12. DTO'ya dönüştür
        var reviewDtos = results.Select(result => new SearchReviewDto
        {
            ReviewId = result.review.Id,
            CompanyId = result.company.Id,
            CompanyName = result.company.Name,
            CompanyLogoUrl = result.company.LogoUrl,
            AuthorUsername = userDict.TryGetValue(result.review.UserId, out var user) 
                ? user.AnonymousUsername 
                : "Anonim Kullanıcı",
            CommentType = result.review.CommentType,
            OverallRating = result.review.OverallRating,
            CommentSummary = TruncateText(result.review.CommentText, 200),
            PostedDate = result.review.CreatedAt,
            NetVotes = result.review.Upvotes - result.review.Downvotes,
            IsVerified = result.review.IsDocumentVerified,
            MatchScore = CalculateMatchScore(result.review, result.company, request.SearchTerm)
        }).ToList();

        return new PagedList<SearchReviewDto>(reviewDtos, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Metni belirtilen uzunlukta keser
    /// </summary>
    private static string? TruncateText(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        // Kelime ortasında kesmeyi önlemek için son boşluğu bul
        var truncated = text.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        
        if (lastSpace > 0)
            truncated = truncated.Substring(0, lastSpace);

        return truncated + "...";
    }

    /// <summary>
    /// Arama terimi ile eşleşme skorunu hesaplar
    /// </summary>
    private static double? CalculateMatchScore(Domain.Entities.Review review, Domain.Entities.Company company, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return null;

        var searchTermLower = searchTerm.ToLower();
        double score = 0;

        // Yorum metninde geçme sayısı
        var commentMatches = review.CommentText.ToLower().Split(' ')
            .Count(word => word.Contains(searchTermLower));
        score += commentMatches * 1.0;

        // Şirket adında geçme (daha yüksek ağırlık)
        if (company.Name.ToLower().Contains(searchTermLower))
            score += 5.0;

        // Tam kelime eşleşmesi bonus
        if (review.CommentText.ToLower().Split(' ').Any(word => word == searchTermLower))
            score += 2.0;

        return score;
    }
}

/// <summary>
/// SearchReviewsQuery validator
/// </summary>
public class SearchReviewsQueryValidator : AbstractValidator<SearchReviewsQuery>
{
    public SearchReviewsQueryValidator()
    {
        // En az bir filtre olmalı
        RuleFor(x => x)
            .Must(HaveAtLeastOneFilter)
            .WithMessage("En az bir arama kriteri belirtmelisiniz.");

        // Arama terimi
        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm!)
                .MinimumLength(2).WithMessage("Arama terimi en az 2 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Arama terimi en fazla 100 karakter olabilir.");
        });

        // Yorum türü
        When(x => !string.IsNullOrWhiteSpace(x.CommentType), () =>
        {
            RuleFor(x => x.CommentType!)
                .Must(BeValidCommentType).WithMessage("Geçersiz yorum kategorisi.");
        });

        // Puan aralığı
        When(x => x.MinRating.HasValue, () =>
        {
            RuleFor(x => x.MinRating!.Value)
                .InclusiveBetween(1, 5).WithMessage("Minimum puan 1-5 arasında olmalıdır.");
        });

        When(x => x.MaxRating.HasValue, () =>
        {
            RuleFor(x => x.MaxRating!.Value)
                .InclusiveBetween(1, 5).WithMessage("Maximum puan 1-5 arasında olmalıdır.");
        });

        // Min-Max karşılaştırması
        When(x => x.MinRating.HasValue && x.MaxRating.HasValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.MinRating!.Value <= x.MaxRating!.Value)
                .WithMessage("Minimum puan, maximum puandan büyük olamaz.");
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
            .InclusiveBetween(1, 50).WithMessage("Sayfa başına en fazla 50 yorum gösterilebilir.");
    }

    private bool HaveAtLeastOneFilter(SearchReviewsQuery query)
    {
        return !string.IsNullOrWhiteSpace(query.SearchTerm) ||
               !string.IsNullOrWhiteSpace(query.CompanyName) ||
               !string.IsNullOrWhiteSpace(query.CommentType) ||
               query.MinRating.HasValue ||
               query.MaxRating.HasValue ||
               query.StartDate.HasValue ||
               query.EndDate.HasValue ||
               query.VerifiedOnly.HasValue;
    }

    private bool BeValidCommentType(string commentType)
    {
        return Domain.Enums.CommentTypes.IsValid(commentType);
    }

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "relevance", "date", "rating", "votes" };
        return validFields.Contains(sortBy.ToLower());
    }
}
