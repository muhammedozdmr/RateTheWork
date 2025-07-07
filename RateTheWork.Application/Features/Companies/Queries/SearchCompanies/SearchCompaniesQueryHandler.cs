using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Mappings;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Queries.SearchCompanies;
/// <summary>
/// Şirket arama query'si
/// </summary>
public record SearchCompaniesQuery : PaginatedRequest, IRequest<PagedList<CompanySearchDto>>
{
    /// <summary>
    /// Arama terimi (şirket adı, sektör vb.)
    /// </summary>
    public string? SearchTerm { get; init; }
    
    /// <summary>
    /// Sektör filtresi
    /// </summary>
    public string? Sector { get; init; }
    
    /// <summary>
    /// Minimum puan filtresi
    /// </summary>
    public decimal? MinRating { get; init; }
    
    /// <summary>
    /// Sıralama kriteri
    /// </summary>
    public string SortBy { get; init; } = "Name"; // Name, Rating, ReviewCount
    
    /// <summary>
    /// Sıralama yönü
    /// </summary>
    public bool IsDescending { get; init; } = false;
}

/// <summary>
/// Şirket arama sonucu DTO'su
/// </summary>
public record CompanySearchDto
{
    /// <summary>
    /// Şirket ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket adı
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Sektör
    /// </summary>
    public string Sector { get; init; } = string.Empty;
    
    /// <summary>
    /// Logo URL'i
    /// </summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>
    /// Ortalama puan
    /// </summary>
    public decimal AverageRating { get; init; }
    
    /// <summary>
    /// Toplam yorum sayısı
    /// </summary>
    public int TotalReviews { get; init; }
    
    /// <summary>
    /// Şehir (adres içinden parse edilecek)
    /// </summary>
    public string City { get; init; } = string.Empty;
    
    /// <summary>
    /// Web sitesi
    /// </summary>
    public string WebsiteUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Onaylı şirket mi?
    /// </summary>
    public bool IsVerified { get; init; }
}
/// <summary>
/// SearchCompanies query handler
/// </summary>
public class SearchCompaniesQueryHandler : IRequestHandler<SearchCompaniesQuery, PagedList<CompanySearchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchCompaniesQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedList<CompanySearchDto>> Handle(SearchCompaniesQuery request, CancellationToken cancellationToken)
    {
        // 1. Temel query - sadece onaylı şirketler
        var query = _unitOfWork.Companies.GetQueryable()
            .Where(c => c.IsApproved && !c.IsDeleted);

        // 2. Arama terimi filtresi
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c => 
                c.Name.ToLower().Contains(searchTerm) ||
                c.Sector.ToLower().Contains(searchTerm) ||
                c.Address.ToLower().Contains(searchTerm));
        }

        // 3. Sektör filtresi
        if (!string.IsNullOrWhiteSpace(request.Sector))
        {
            query = query.Where(c => c.Sector == request.Sector);
        }

        // 4. Minimum puan filtresi
        if (request.MinRating.HasValue && request.MinRating > 0)
        {
            query = query.Where(c => c.AverageRating >= request.MinRating.Value);
        }

        // 5. Sıralama
        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.IsDescending 
                ? query.OrderByDescending(c => c.AverageRating).ThenBy(c => c.Name)
                : query.OrderBy(c => c.AverageRating).ThenBy(c => c.Name),
            
            "reviewcount" => request.IsDescending 
                ? query.OrderByDescending(c => c.TotalReviews).ThenBy(c => c.Name)
                : query.OrderBy(c => c.TotalReviews).ThenBy(c => c.Name),
            
            _ => request.IsDescending 
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name)
        };

        // 6. Sayfalama parametrelerini validate et
        var (pageNumber, pageSize) = request.GetValidatedPaginationParams();

        // 7. Toplam kayıt sayısını al
        var totalCount = await query.CountAsync(cancellationToken);

        // 8. Sayfalı veriyi getir ve DTO'ya map'le
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanySearchDto
            {
                CompanyId = c.Id,
                Name = c.Name,
                Sector = c.Sector,
                LogoUrl = c.LogoUrl,
                AverageRating = c.AverageRating,
                TotalReviews = c.TotalReviews,
                City = ExtractCityFromAddress(c.Address),
                WebsiteUrl = c.WebsiteUrl,
                IsVerified = c.IsApproved
            })
            .ToListAsync(cancellationToken);

        return new PagedList<CompanySearchDto>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Adresten şehir bilgisini çıkarır
    /// </summary>
    private static string ExtractCityFromAddress(string address)
    {
        // Basit bir implementasyon - adresten son virgülden sonraki kısmı al
        var parts = address.Split(',');
        return parts.Length > 0 ? parts[^1].Trim() : "Bilinmiyor";
    }
}

/// <summary>
/// SearchCompanies validation kuralları
/// </summary>
public class SearchCompaniesQueryValidator : AbstractValidator<SearchCompaniesQuery>
{
    public SearchCompaniesQueryValidator()
    {
        // Arama terimi
        When(x => !string.IsNullOrWhiteSpace(x.SearchTerm), () =>
        {
            RuleFor(x => x.SearchTerm)
                .MinimumLength(2).WithMessage("Arama terimi en az 2 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("Arama terimi 100 karakterden uzun olamaz.");
        });

        // Sektör
        When(x => !string.IsNullOrWhiteSpace(x.Sector), () =>
        {
            RuleFor(x => x.Sector)
                .Must(BeValidSector).WithMessage("Geçersiz sektör.");
        });

        // Minimum puan
        When(x => x.MinRating.HasValue, () =>
        {
            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5).WithMessage("Puan 0-5 arasında olmalıdır.");
        });

        // Sıralama
        RuleFor(x => x.SortBy)
            .Must(BeValidSortField).WithMessage("Geçersiz sıralama kriteri.");

        // Sayfalama
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1'den küçük olamaz.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1-100 arasında olmalıdır.");
    }

    private bool BeValidSector(string? sector)
    {
        return sector != null && Domain.Enums.Sectors.GetAll().Contains(sector);
    }

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "name", "rating", "reviewcount" };
        return validFields.Contains(sortBy.ToLower());
    }
}
