using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using RateTheWork.Application.Common.Mappings.DTOs;
using RateTheWork.Domain.Entities;

namespace RateTheWork.Application.Common.Mappings;

/// <summary>
/// AutoMapper profil sınıfı - Tüm entity-DTO mapping'lerini tanımlar.
/// DI container'a otomatik olarak register edilir.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Mapping konfigürasyonlarını oluşturur
    /// </summary>
    public MappingProfile()
    { 
        /// <summary>
        /// User entity'sini UserDto'ya map'ler.
        /// Hassas bilgiler (şifreli alanlar) DTO'ya dahil edilmez.
        /// </summary>
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.AnonymousUsername))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsBanned));

        /// <summary>
        /// User entity'sini UserProfileDto'ya map'ler.
        /// Kullanıcının kendi profilini görüntülerken kullanılır.
        /// </summary>
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.AnonymousUsername))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.IsBanned))
            .ForMember(dest => dest.MemberSince, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => 
                src.IsEmailVerified && src.IsPhoneVerified && src.IsTcIdentityVerified))
            .ForMember(dest => dest.IsEmailVerified, opt => opt.MapFrom(src => src.IsEmailVerified))
            .ForMember(dest => dest.IsPhoneVerified, opt => opt.MapFrom(src => src.IsPhoneVerified))
            .ForMember(dest => dest.IsTcIdentityVerified, opt => opt.MapFrom(src => src.IsTcIdentityVerified))
            // Şifreli alanlar handler'da decrypt edilecek, burada null bırakıyoruz
            .ForMember(dest => dest.FirstName, opt => opt.Ignore())
            .ForMember(dest => dest.LastName, opt => opt.Ignore())
            // İstatistikler handler'da hesaplanacak
            .ForMember(dest => dest.TotalReviews, opt => opt.Ignore())
            .ForMember(dest => dest.TotalBadges, opt => opt.Ignore());

        // ========== COMPANY MAPPINGS ==========
        
        /// <summary>
        /// Company entity'sini CompanyDto'ya map'ler.
        /// Liste görünümlerinde kullanılır.
        /// </summary>
        CreateMap<Company, CompanyDto>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.TotalReviews))
            // City bilgisi Address'ten parse edilecek veya handler'da set edilecek
            .ForMember(dest => dest.City, opt => opt.Ignore());

        /// <summary>
        /// Company entity'sini CompanyDetailDto'ya map'ler.
        /// Detay sayfasında tüm bilgileri gösterir.
        /// </summary>
        CreateMap<Company, CompanyDetailDto>()
            .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.AverageRating))
            .ForMember(dest => dest.ReviewCount, opt => opt.MapFrom(src => src.TotalReviews))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsApproved))
            .ForMember(dest => dest.VerificationDate, opt => opt.MapFrom(src => src.ApprovedAt));

        // ========== REVIEW MAPPINGS ==========
        
        /// <summary>
        /// Review entity'sini ReviewDto'ya map'ler.
        /// Yorum listelerinde kullanılır.
        /// </summary>
        CreateMap<Review, ReviewDto>()
            .ForMember(dest => dest.ReviewId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AuthorUsername, opt => opt.Ignore()) // Handler'da User'dan alınacak
            .ForMember(dest => dest.CompanyName, opt => opt.Ignore()) // Handler'da Company'den alınacak
            .ForMember(dest => dest.PostedDate, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsVerified, opt => opt.MapFrom(src => src.IsDocumentVerified))
            .ForMember(dest => dest.NetVotes, opt => opt.MapFrom(src => src.Upvotes - src.Downvotes));

        /// <summary>
        /// Review entity'sini ReviewDetailDto'ya map'ler.
        /// Yorum detay sayfasında kullanılır.
        /// </summary>
        CreateMap<Review, ReviewDetailDto>()
            .IncludeBase<Review, ReviewDto>() // ReviewDto mapping'ini dahil et
            .ForMember(dest => dest.HasDocument, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.DocumentUrl)))
            .ForMember(dest => dest.LastEditDate, opt => opt.MapFrom(src => src.ModifiedAt));

        // ========== NOTIFICATION MAPPINGS ==========
        
        /// <summary>
        /// Notification entity'sini NotificationDto'ya map'ler.
        /// </summary>
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.NotificationId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedAt));

        // ========== BADGE MAPPINGS ==========
        
        /// <summary>
        /// Badge entity'sini BadgeDto'ya map'ler.
        /// </summary>
        CreateMap<Badge, BadgeDto>()
            .ForMember(dest => dest.BadgeId, opt => opt.MapFrom(src => src.Id));

        /// <summary>
        /// UserBadge entity'sini UserBadgeDto'ya map'ler.
        /// Join tablosu için kullanılır.
        /// </summary>
        CreateMap<UserBadge, UserBadgeDto>()
            .ForMember(dest => dest.BadgeName, opt => opt.Ignore()) // Handler'da Badge'den alınacak
            .ForMember(dest => dest.BadgeDescription, opt => opt.Ignore()) // Handler'da Badge'den alınacak
            .ForMember(dest => dest.BadgeIconUrl, opt => opt.Ignore()); // Handler'da Badge'den alınacak

        // ========== ADMIN MAPPINGS ==========
        
        /// <summary>
        /// VerificationRequest entity'sini VerificationRequestDto'ya map'ler.
        /// Admin panelinde kullanılır.
        /// </summary>
        CreateMap<VerificationRequest, VerificationRequestDto>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ReviewerUsername, opt => opt.Ignore()) // Handler'da User'dan alınacak
            .ForMember(dest => dest.CompanyName, opt => opt.Ignore()) // Handler'da Review -> Company'den alınacak
            .ForMember(dest => dest.ReviewDate, opt => opt.Ignore()); // Handler'da Review'dan alınacak

        // ========== AUDIT LOG MAPPINGS ==========
        
        /// <summary>
        /// AuditLog entity'sini AuditLogDto'ya map'ler.
        /// Admin panelinde audit trail için kullanılır.
        /// </summary>
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.LogId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.AdminUsername, opt => opt.Ignore()) // Handler'da AdminUser'dan alınacak
            .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src => src.CreatedAt));
    }
}

/// <summary>
/// AutoMapper extension metodları
/// </summary>
public static class MappingExtensions
{
    /// <summary>
    /// IQueryable üzerinde projection yapar - veritabanından sadece gerekli kolonları çeker
    /// </summary>
    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
        this IQueryable queryable, 
        IConfigurationProvider configuration,
        CancellationToken cancellationToken = default)
    {
        return queryable
            .ProjectTo<TDestination>(configuration)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Sayfalı projection - performans için optimize edilmiş
    /// </summary>
    public static async Task<PagedList<TDestination>> ProjectToPagedListAsync<TDestination>(
        this IQueryable<object> queryable,
        IConfigurationProvider configuration,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var count = await queryable.CountAsync(cancellationToken);
        
        var items = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TDestination>(configuration)
            .ToListAsync(cancellationToken);

        return new PagedList<TDestination>(items, count, pageNumber, pageSize);
    }
}