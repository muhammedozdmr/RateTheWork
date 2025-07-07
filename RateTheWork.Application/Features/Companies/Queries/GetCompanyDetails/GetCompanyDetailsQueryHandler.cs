using AutoMapper;
using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings.DTOs;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Queries.GetCompanyDetails;

/// <summary>
/// Şirket detaylarını getiren query
/// </summary>
public record GetCompanyDetailsQuery : IRequest<CompanyDetailDto>
{
    /// <summary>
    /// Detayları getirilecek şirketin ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
}

/// <summary>
/// GetCompanyDetailsQuery handler
/// </summary>
public class GetCompanyDetailsQueryHandler : IRequestHandler<GetCompanyDetailsQuery, CompanyDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyDetailsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyDetailDto> Handle(GetCompanyDetailsQuery request, CancellationToken cancellationToken)
    {
        // 1. Şirketi getir
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        
        if (company == null)
        {
            throw new NotFoundException("Şirket bulunamadı.");
        }

        // 2. Sadece onaylanmış şirketler görüntülenebilir
        if (!company.IsApproved)
        {
            throw new ForbiddenAccessException("Bu şirket henüz onaylanmamıştır.");
        }

        // 3. Entity'den DTO'ya map'le
        var companyDetail = _mapper.Map<CompanyDetailDto>(company);

        // 4. Vergi numarasını maskele (güvenlik için)
        // Örnek: 1234567890 -> 123****890
        if (!string.IsNullOrEmpty(company.TaxId) && company.TaxId.Length >= 10)
        {
            var masked = company.TaxId.Substring(0, 3) + 
                        new string('*', company.TaxId.Length - 6) + 
                        company.TaxId.Substring(company.TaxId.Length - 3);
            
            companyDetail = companyDetail with { MaskedTaxId = masked };
        }

        // 5. Şehir bilgisini adres içinden çıkar
        var addressParts = company.Address.Split(',');
        if (addressParts.Length > 0)
        {
            var city = addressParts[addressParts.Length - 1].Trim();
            companyDetail = companyDetail with { City = city };
        }

        // 6. Yorum istatistiklerini getir
        var reviewStats = await GetReviewStatisticsAsync(company.Id, cancellationToken);
        companyDetail = companyDetail with
        {
            TotalReviews = reviewStats.TotalCount,
            RatingBreakdown = reviewStats.RatingBreakdown,
            CommentTypeBreakdown = reviewStats.CommentTypeBreakdown,
            RecentReviewsCount = reviewStats.RecentCount,
            VerifiedReviewsPercentage = reviewStats.VerifiedPercentage
        };

        // 7. Kullanıcı bu şirkete yorum yapmış mı?
        if (_currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var userReviewTypes = await GetUserReviewTypesAsync(company.Id, _currentUserService.UserId, cancellationToken);
            companyDetail = companyDetail with { UserReviewedTypes = userReviewTypes };
        }

        return companyDetail;
    }

    /// <summary>
    /// Şirket yorum istatistiklerini hesaplar
    /// </summary>
    private async Task<ReviewStatistics> GetReviewStatisticsAsync(string? companyId, CancellationToken cancellationToken)
    {
        var reviews = await _unitOfWork.Reviews.GetAsync(r => 
            r.CompanyId == companyId && r.IsActive);

        var totalCount = reviews.Count;
        var verifiedCount = reviews.Count(r => r.IsDocumentVerified);
        var recentCount = reviews.Count(r => r.CreatedAt >= DateTime.UtcNow.AddMonths(-3));

        // Puan dağılımı
        var ratingBreakdown = reviews
            .GroupBy(r => Math.Floor(r.OverallRating))
            .ToDictionary(g => (int)g.Key, g => g.Count());

        // Yorum türü dağılımı
        var commentTypeBreakdown = reviews
            .GroupBy(r => r.CommentType)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ReviewStatistics
        {
            TotalCount = totalCount,
            VerifiedCount = verifiedCount,
            VerifiedPercentage = totalCount > 0 ? (verifiedCount * 100.0 / totalCount) : 0,
            RecentCount = recentCount,
            RatingBreakdown = ratingBreakdown,
            CommentTypeBreakdown = commentTypeBreakdown
        };
    }

    /// <summary>
    /// Kullanıcının bu şirkete yaptığı yorum türlerini getirir
    /// </summary>
    private async Task<List<string>> GetUserReviewTypesAsync(string? companyId, string userId, CancellationToken cancellationToken)
    {
        var userReviews = await _unitOfWork.Reviews.GetAsync(r => 
            r.CompanyId == companyId && 
            r.UserId == userId && 
            r.IsActive);

        return userReviews.Select(r => r.CommentType).ToList();
    }

    /// <summary>
    /// Yorum istatistikleri için yardımcı sınıf
    /// </summary>
    private record ReviewStatistics
    {
        public int TotalCount { get; init; }
        public int VerifiedCount { get; init; }
        public double VerifiedPercentage { get; init; }
        public int RecentCount { get; init; }
        public Dictionary<int, int> RatingBreakdown { get; init; } = new();
        public Dictionary<string, int> CommentTypeBreakdown { get; init; } = new();
    }
}

/// <summary>
/// GetCompanyDetailsQuery için FluentValidation validator
/// </summary>
public class GetCompanyDetailsQueryValidator : AbstractValidator<GetCompanyDetailsQuery>
{
    public GetCompanyDetailsQueryValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen şirket listesine dönüp tekrar deneyiniz.")
            .Must(BeAValidGuid).WithMessage("Geçersiz şirket seçimi. Lütfen şirket listesinden bir şirket seçiniz.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
