using RateTheWork.Domain.Common;
using RateTheWork.Domain.Events.Company;
using RateTheWork.Domain.Exceptions;

namespace RateTheWork.Domain.Entities;

/// <summary>
/// Şirket entity'si - Platformdaki şirket bilgilerini temsil eder.
/// ApprovableBaseEntity'den türer ve admin onayı gerektirir.
/// </summary>
public class Company : ApprovableBaseEntity, IAggregateRoot
{
    // Constants
    private const int MaxNameLength = 200;
    private const int MaxAddressLength = 500;
    private const int TaxIdLength = 10;
    private const int MersisNoLength = 16;

    // Properties - Şirket Bilgileri
    public string Name { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string MersisNo { get; private set; } = string.Empty;
    public string Sector { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string WebsiteUrl { get; private set; } = string.Empty;
    public string? LogoUrl { get; private set; }

    // Properties - Sosyal Medya
    public string? LinkedInUrl { get; private set; }
    public string? XUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    public string? FacebookUrl { get; private set; }

    // Properties - İstatistikler
    public decimal AverageRating { get; private set; } = 0;
    public int TotalReviews { get; private set; } = 0;

    /// <summary>
    /// EF Core için parametresiz private constructor
    /// </summary>
    private Company() : base()
    {
    }

    /// <summary>
    /// Yeni şirket oluşturur (Factory method)
    /// </summary>
    public static Company Create(
        string name,
        string taxId,
        string mersisNo,
        string sector,
        string address,
        string phoneNumber,
        string email,
        string websiteUrl)
    {
        // Validasyonlar
        ValidateName(name);
        ValidateTaxId(taxId);
        ValidateMersisNo(mersisNo);
        ValidateSector(sector);
        ValidateAddress(address);
        ValidateEmail(email);
        ValidateWebsiteUrl(websiteUrl);

        var company = new Company
        {
            Name = name,
            TaxId = taxId,
            MersisNo = mersisNo,
            Sector = sector,
            Address = address,
            PhoneNumber = phoneNumber,
            Email = email.ToLowerInvariant(),
            WebsiteUrl = websiteUrl,
            AverageRating = 0,
            TotalReviews = 0
        };

        // Domain Event
        company.AddDomainEvent(new CompanyCreatedEvent(
            company.Id,
            company.Name,
            company.TaxId,
            company.MersisNo,
            company.Sector,
            DateTime.UtcNow
        ));

        return company;
    }

    /// <summary>
    /// Şirket bilgilerini günceller
    /// </summary>
    public void UpdateInfo(
        string name,
        string sector,
        string address,
        string phoneNumber,
        string email,
        string websiteUrl,
        string updatedBy)
    {
        ValidateName(name);
        ValidateSector(sector);
        ValidateAddress(address);
        ValidateEmail(email);
        ValidateWebsiteUrl(websiteUrl);

        var updatedFields = new List<string>();

        if (Name != name)
        {
            Name = name;
            updatedFields.Add(nameof(Name));
        }

        if (Sector != sector)
        {
            Sector = sector;
            updatedFields.Add(nameof(Sector));
        }

        if (Address != address)
        {
            Address = address;
            updatedFields.Add(nameof(Address));
        }

        if (PhoneNumber != phoneNumber)
        {
            PhoneNumber = phoneNumber;
            updatedFields.Add(nameof(PhoneNumber));
        }

        if (Email != email.ToLowerInvariant())
        {
            Email = email.ToLowerInvariant();
            updatedFields.Add(nameof(Email));
        }

        if (WebsiteUrl != websiteUrl)
        {
            WebsiteUrl = websiteUrl;
            updatedFields.Add(nameof(WebsiteUrl));
        }

        if (updatedFields.Any())
        {
            SetModifiedAudit(updatedBy);

            // Domain Event
            AddDomainEvent(new CompanyInfoUpdatedEvent(
                Id,
                updatedFields.ToArray(),
                updatedBy,
                DateTime.UtcNow
            ));
        }
    }

    /// <summary>
    /// Logo URL'ini günceller
    /// </summary>
    public void UpdateLogo(string logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            throw new ArgumentNullException(nameof(logoUrl));

        LogoUrl = logoUrl;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyLogoUpdatedEvent(
            Id,
            logoUrl,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Sosyal medya linklerini günceller
    /// </summary>
    public void UpdateSocialMediaLinks(
        string? linkedInUrl = null,
        string? xUrl = null,
        string? instagramUrl = null,
        string? facebookUrl = null)
    {
        if (linkedInUrl != null) LinkedInUrl = linkedInUrl;
        if (xUrl != null) XUrl = xUrl;
        if (instagramUrl != null) InstagramUrl = instagramUrl;
        if (facebookUrl != null) FacebookUrl = facebookUrl;
        
        SetModifiedDate();
    }

    /// <summary>
    /// Yorum istatistiklerini günceller
    /// </summary>
    public void UpdateReviewStatistics(decimal averageRating, int totalReviews)
    {
        var oldRating = AverageRating;
        
        AverageRating = Math.Round(averageRating, 2);
        TotalReviews = totalReviews;
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyRatingUpdatedEvent(
            Id,
            oldRating,
            AverageRating,
            TotalReviews,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Yeni yorum eklendiğinde çağrılır
    /// </summary>
    public void AddReview(decimal rating)
    {
        var oldRating = AverageRating;
        var totalRating = AverageRating * TotalReviews + rating;
        TotalReviews++;
        AverageRating = Math.Round(totalRating / TotalReviews, 2);
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyRatingUpdatedEvent(
            Id,
            oldRating,
            AverageRating,
            TotalReviews,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// Yorum silindiğinde çağrılır
    /// </summary>
    public void RemoveReview(decimal rating)
    {
        var oldRating = AverageRating;
        
        if (TotalReviews <= 1)
        {
            AverageRating = 0;
            TotalReviews = 0;
        }
        else
        {
            var totalRating = AverageRating * TotalReviews - rating;
            TotalReviews--;
            AverageRating = Math.Round(totalRating / TotalReviews, 2);
        }
        
        SetModifiedDate();

        // Domain Event
        AddDomainEvent(new CompanyRatingUpdatedEvent(
            Id,
            oldRating,
            AverageRating,
            TotalReviews,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// ApprovableBaseEntity Approve override
    /// </summary>
    public override void Approve(string approvedBy, string? notes = null)
    {
        base.Approve(approvedBy, notes);

        // Domain Event
        AddDomainEvent(new CompanyApprovedEvent(
            Id,
            approvedBy,
            notes,
            DateTime.UtcNow
        ));
    }

    /// <summary>
    /// ApprovableBaseEntity Reject override
    /// </summary>
    public override void Reject(string rejectedBy, string reason)
    {
        base.Reject(rejectedBy, reason);

        // Domain Event
        AddDomainEvent(new CompanyRejectedEvent(
            Id,
            rejectedBy,
            reason,
            DateTime.UtcNow
        ));
    }

    // Private validation methods
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        if (name.Length > MaxNameLength)
            throw new BusinessRuleException($"Şirket adı {MaxNameLength} karakterden uzun olamaz.");
    }

    private static void ValidateTaxId(string taxId)
    {
        if (string.IsNullOrWhiteSpace(taxId))
            throw new ArgumentNullException(nameof(taxId));

        if (taxId.Length != TaxIdLength || !taxId.All(char.IsDigit))
            throw new BusinessRuleException($"Vergi numarası {TaxIdLength} haneli olmalı ve sadece rakamlardan oluşmalıdır.");
    }

    private static void ValidateMersisNo(string mersisNo)
    {
        if (string.IsNullOrWhiteSpace(mersisNo))
            throw new ArgumentNullException(nameof(mersisNo));

        if (mersisNo.Length != MersisNoLength)
            throw new BusinessRuleException($"MERSİS numarası {MersisNoLength} haneli olmalıdır.");
    }

    private static void ValidateSector(string sector)
    {
        if (string.IsNullOrWhiteSpace(sector))
            throw new ArgumentNullException(nameof(sector));

        var validSectors = new[] { "Teknoloji", "Finans", "Sağlık", "Eğitim", "Perakende", 
            "Üretim", "İnşaat", "Turizm", "Medya", "Telekomünikasyon", "Enerji", 
            "Otomotiv", "Yiyecek & İçecek", "Lojistik", "Gayrimenkul", "Diğer" };

        if (!validSectors.Contains(sector))
            throw new BusinessRuleException("Geçersiz sektör.");
    }

    private static void ValidateAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentNullException(nameof(address));

        if (address.Length > MaxAddressLength)
            throw new BusinessRuleException($"Adres {MaxAddressLength} karakterden uzun olamaz.");
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentNullException(nameof(email));

        if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new BusinessRuleException("Geçersiz email formatı.");
    }

    private static void ValidateWebsiteUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || 
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessRuleException("Geçersiz website URL'i.");
        }
    }
}