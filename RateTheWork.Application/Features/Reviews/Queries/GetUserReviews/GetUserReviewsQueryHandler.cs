using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Application.Common.Mappings;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Reviews.Queries.GetUserReviews;

/// <summary>
/// Kullanıcının yaptığı yorumları getiren query
/// </summary>
public record GetUserReviewsQuery : PaginatedRequest, IRequest<PagedList<UserReviewDto>>
{
    /// <summary>
    /// Yorumları getirilecek kullanıcının ID'si (boş ise current user)
    /// </summary>
    public string? UserId { get; init; }
    
    /// <summary>
    /// Sadece aktif yorumlar mı?
    /// </summary>
    public bool OnlyActive { get; init; } = true;
    
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
/// Kullanıcı yorumu DTO'su
/// </summary>
public record UserReviewDto
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
    /// Yorum türü
    /// </summary>
    public string? CommentType { get; init; } = string.Empty;
    
    /// <summary>
    /// Verilen puan
    /// </summary>
    public decimal OverallRating { get; init; }
    
    /// <summary>
    /// Yorum metni
    /// </summary>
    public string? CommentText { get; init; } = string.Empty;
    
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
    /// Yorum aktif mi?
    /// </summary>
    public bool IsActive { get; init; }
    
    /// <summary>
    /// Yorum güncellenmiş mi?
    /// </summary>
    public bool IsEdited { get; init; }
    
    /// <summary>
    /// Şikayet sayısı
    /// </summary>
    public int ReportCount { get; init; }
}

/// <summary>
/// GetUserReviewsQuery handler
/// </summary>
public class GetUserReviewsQueryHandler : IRequestHandler<GetUserReviewsQuery, PagedList<UserReviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public GetUserReviewsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<PagedList<UserReviewDto>> Handle(GetUserReviewsQuery request, CancellationToken cancellationToken)
    {
        // 1. UserId kontrolü - boşsa current user'ı al
        var targetUserId = request.UserId ?? _currentUserService.UserId;
        
        if (string.IsNullOrEmpty(targetUserId))
        {
            throw new ForbiddenAccessException("Kullanıcı girişi yapmanız gerekmektedir.");
        }

        // 2. Kullanıcı var mı kontrolü
        var userExists = await _unitOfWork.Users.AnyAsync(u => u.Id == targetUserId);
        if (!userExists)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // 3. Yetki kontrolü - başkasının yorumlarını sadece admin görebilir
        var isCurrentUser = targetUserId == _currentUserService.UserId;
        var isAdmin = _currentUserService.Roles.Contains("SuperAdmin") || 
                     _currentUserService.Roles.Contains("Moderator");

        if (!isCurrentUser && !isAdmin)
        {
            throw new ForbiddenAccessException("Bu kullanıcının yorumlarını görüntüleme yetkiniz yok.");
        }

        // 4. Temel query
        var query = _unitOfWork.Reviews.GetQueryable()
            .Where(r => r.UserId == targetUserId);

        // 5. Aktif filtresi (admin tüm yorumları görebilir)
        if (request.OnlyActive && !isAdmin)
        {
            query = query.Where(r => r.IsActive);
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

        // 8. Veriyi getir
        var reviews = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // 9. Şirket bilgilerini toplu getir (N+1 önleme)
        var companyIds = reviews.Select(r => r.CompanyId).Distinct().ToList();
        var companies = await _unitOfWork.Companies.GetAsync(c => companyIds.Contains(c.Id));
        var companyDict = companies.ToDictionary(c => c.Id);

        // 10. DTO'ya dönüştür
        var reviewDtos = reviews.Select(review => new UserReviewDto
        {
            ReviewId = review.Id,
            CompanyId = review.CompanyId,
            CompanyName = companyDict.TryGetValue(review.CompanyId, out var company) 
                ? company.Name 
                : "Bilinmeyen Şirket",
            CommentType = review.CommentType.ToString(),
            OverallRating = review.OverallRating,
            CommentText = review.CommentText,
            PostedDate = review.CreatedAt,
            Upvotes = review.Upvotes,
            Downvotes = review.Downvotes,
            NetVotes = review.Upvotes - review.Downvotes,
            IsVerified = review.IsDocumentVerified,
            IsActive = review.IsActive,
            IsEdited = review.ModifiedAt.HasValue,
            ReportCount = review.ReportCount
        }).ToList();

        return new PagedList<UserReviewDto>(reviewDtos, totalCount, pageNumber, pageSize);
    }
}

/// <summary>
/// GetUserReviewsQuery validator
/// </summary>
public class GetUserReviewsQueryValidator : AbstractValidator<GetUserReviewsQuery>
{
    public GetUserReviewsQueryValidator()
    {
        // UserId - opsiyonel ama varsa geçerli olmalı
        When(x => !string.IsNullOrWhiteSpace(x.UserId), () =>
        {
            RuleFor(x => x.UserId!)
                .Must(BeAValidGuid).WithMessage("Geçersiz kullanıcı ID'si.");
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

    private bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "date", "rating", "votes" };
        return validFields.Contains(sortBy.ToLower());
    }
}
