using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Commands.UpdateCompany;

/// <summary>
/// Şirket güncelleme komutu
/// </summary>
public record UpdateCompanyCommand : IRequest<UpdateCompanyResult>
{
    /// <summary>
    /// Güncellenecek şirketin ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Şirket adı (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? Name { get; init; }
    
    /// <summary>
    /// Sektör (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? Sector { get; init; }
    
    /// <summary>
    /// Adres (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Telefon numarası (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? PhoneNumber { get; init; }
    
    /// <summary>
    /// Email adresi (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// Web sitesi (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? WebsiteUrl { get; init; }
    
    /// <summary>
    /// Logo URL (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>
    /// LinkedIn URL (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? LinkedInUrl { get; init; }
    
    /// <summary>
    /// X (Twitter) URL (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? XUrl { get; init; }
    
    /// <summary>
    /// Instagram URL (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? InstagramUrl { get; init; }
    
    /// <summary>
    /// Facebook URL (değiştirilmek istenmiyorsa null)
    /// </summary>
    public string? FacebookUrl { get; init; }
}

/// <summary>
/// Şirket güncelleme sonucu
/// </summary>
public record UpdateCompanyResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Güncellenen alan sayısı
    /// </summary>
    public int UpdatedFieldsCount { get; init; }
    
    /// <summary>
    /// Tekrar onay gerekiyor mu?
    /// </summary>
    public bool RequiresReApproval { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// UpdateCompany command handler
/// </summary>
public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, UpdateCompanyResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateCompanyResult> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        // 1. Şirketi getir
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        
        if (company == null)
        {
            throw new NotFoundException("Şirket bulunamadı.");
        }

        // 2. Yetki kontrolü - Sadece şirketi ekleyen veya admin güncelleyebilir
        var isAdmin = _currentUserService.Roles.Contains(AdminRoles.SuperAdmin) || 
                     _currentUserService.Roles.Contains(AdminRoles.ContentManager);
        
        if (!isAdmin && company.CreatedBy != _currentUserService.UserId)
        {
            throw new ForbiddenAccessException("Bu şirketi güncelleme yetkiniz bulunmamaktadır.");
        }

        var updatedFieldsCount = 0;
        var requiresReApproval = false;

        // 3. Şirket adı güncelleme (kritik değişiklik - yeniden onay gerektirir)
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != company.Name)
        {
            company.Name = request.Name.Trim();
            updatedFieldsCount++;
            requiresReApproval = !isAdmin; // Admin değilse yeniden onay gerekir
        }

        // 4. Sektör güncelleme
        if (!string.IsNullOrWhiteSpace(request.Sector) && request.Sector != company.Sector)
        {
            company.Sector = request.Sector;
            updatedFieldsCount++;
        }

        // 5. Adres güncelleme
        if (!string.IsNullOrWhiteSpace(request.Address) && request.Address != company.Address)
        {
            company.Address = request.Address.Trim();
            updatedFieldsCount++;
        }

        // 6. Telefon güncelleme
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) && request.PhoneNumber != company.PhoneNumber)
        {
            company.PhoneNumber = request.PhoneNumber.Trim();
            updatedFieldsCount++;
        }

        // 7. Email güncelleme
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != company.Email)
        {
            company.Email = request.Email.Trim().ToLowerInvariant();
            updatedFieldsCount++;
        }

        // 8. Website güncelleme
        if (!string.IsNullOrWhiteSpace(request.WebsiteUrl) && request.WebsiteUrl != company.WebsiteUrl)
        {
            company.WebsiteUrl = request.WebsiteUrl.Trim();
            updatedFieldsCount++;
        }

        // 9. Logo güncelleme
        if (request.LogoUrl != null && request.LogoUrl != company.LogoUrl)
        {
            company.LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim();
            updatedFieldsCount++;
        }

        // 10. Sosyal medya linkleri güncelleme
        if (request.LinkedInUrl != null && request.LinkedInUrl != company.LinkedInUrl)
        {
            company.LinkedInUrl = string.IsNullOrWhiteSpace(request.LinkedInUrl) ? null : request.LinkedInUrl.Trim();
            updatedFieldsCount++;
        }

        if (request.XUrl != null && request.XUrl != company.XUrl)
        {
            company.XUrl = string.IsNullOrWhiteSpace(request.XUrl) ? null : request.XUrl.Trim();
            updatedFieldsCount++;
        }

        if (request.InstagramUrl != null && request.InstagramUrl != company.InstagramUrl)
        {
            company.InstagramUrl = string.IsNullOrWhiteSpace(request.InstagramUrl) ? null : request.InstagramUrl.Trim();
            updatedFieldsCount++;
        }

        if (request.FacebookUrl != null && request.FacebookUrl != company.FacebookUrl)
        {
            company.FacebookUrl = string.IsNullOrWhiteSpace(request.FacebookUrl) ? null : request.FacebookUrl.Trim();
            updatedFieldsCount++;
        }

        // 11. Hiç değişiklik yoksa
        if (updatedFieldsCount == 0)
        {
            return new UpdateCompanyResult
            {
                Success = true,
                UpdatedFieldsCount = 0,
                RequiresReApproval = false,
                Message = "Güncellenecek bir değişiklik bulunamadı."
            };
        }

        // 12. Güncelleme bilgilerini set et
        company.ModifiedAt = DateTime.UtcNow;
        company.ModifiedBy = _currentUserService.UserId;

        // 13. Yeniden onay gerekiyorsa
        if (requiresReApproval)
        {
            company.IsApproved = false;
            company.ApprovedBy = null;
            company.ApprovedAt = null;
            company.ApprovalNotes = "Şirket bilgileri güncellendi, yeniden onay bekliyor.";
        }

        // 14. Değişiklikleri kaydet
        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 15. Audit log oluştur
        var auditLog = new Domain.Entities.AuditLog(
            adminUserId: _currentUserService.UserId!,
            actionType: "CompanyUpdated",
            entityType: "Company",
            entityId: company.Id
        )
        {
            Details = $"Güncellenen alan sayısı: {updatedFieldsCount}"
        };
        
        await _unitOfWork.AuditLogs.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var message = requiresReApproval 
            ? "Şirket bilgileri güncellendi. Kritik değişiklikler nedeniyle yeniden onay bekliyor." 
            : "Şirket bilgileri başarıyla güncellendi.";

        return new UpdateCompanyResult
        {
            Success = true,
            UpdatedFieldsCount = updatedFieldsCount,
            RequiresReApproval = requiresReApproval,
            Message = message
        };
    }
}

/// <summary>
/// UpdateCompany validation kuralları
/// </summary>
public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        // Şirket ID
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("İşleminiz gerçekleştirilemedi. Lütfen sayfayı yenileyip tekrar deneyin.")
            .Must(BeAValidGuid).WithMessage("Geçersiz işlem. Lütfen şirket listesine dönüp tekrar deneyin.");

        // En az bir alan güncellenmeli
        RuleFor(x => x)
            .Must(HaveAtLeastOneFieldToUpdate)
            .WithMessage("Güncellemek için en az bir alanı değiştirmelisiniz.");

        // Şirket adı
        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name!)
                .MinimumLength(3).WithMessage("Şirket adı en az 3 karakter olmalıdır.")
                .MaximumLength(200).WithMessage("Şirket adı 200 karakteri aşamaz.");
        });

        // Sektör
        When(x => !string.IsNullOrWhiteSpace(x.Sector), () =>
        {
            RuleFor(x => x.Sector!)
                .Must(BeValidSector).WithMessage("Geçerli bir sektör seçiniz.");
        });

        // Adres
        When(x => !string.IsNullOrWhiteSpace(x.Address), () =>
        {
            RuleFor(x => x.Address!)
                .MinimumLength(20).WithMessage("Adres en az 20 karakter olmalıdır.")
                .MaximumLength(500).WithMessage("Adres 500 karakteri aşamaz.");
        });

        // Telefon
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz.");
        });

        // Email
        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email!)
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
                .MaximumLength(100).WithMessage("Email adresi 100 karakteri aşamaz.");
        });

        // Website
        When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl), () =>
        {
            RuleFor(x => x.WebsiteUrl!)
                .Must(BeAValidUrl).WithMessage("Geçerli bir web sitesi adresi giriniz.");
        });

        // Opsiyonel URL'ler
        When(x => !string.IsNullOrWhiteSpace(x.LogoUrl), () =>
        {
            RuleFor(x => x.LogoUrl!)
                .Must(BeAValidUrl).WithMessage("Geçerli bir logo adresi giriniz.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl), () =>
        {
            RuleFor(x => x.LinkedInUrl!)
                .Must(BeAValidUrl).WithMessage("Geçerli bir LinkedIn adresi giriniz.")
                .Must(url => url!.Contains("linkedin.com")).WithMessage("LinkedIn adresi linkedin.com içermelidir.");
        });
        
        When(x => !string.IsNullOrWhiteSpace(x.XUrl), () =>
        {
            RuleFor(x => x.XUrl)
                .Must(BeAValidUrl).WithMessage("Geçerli bir X URL'i giriniz.")
                .Must(url => url!.Contains("x.com")).WithMessage("X URL'i x.com içermelidir.");
        });
        
        When(x => !string.IsNullOrWhiteSpace(x.FacebookUrl), () =>
        {
            RuleFor(x => x.FacebookUrl)
                .Must(BeAValidUrl).WithMessage("Geçerli bir Facebook URL'i giriniz.")
                .Must(url => url!.Contains("facebook.com")).WithMessage("Facebook URL'i facebook.com içermelidir.");
        });
        
        When(x => !string.IsNullOrWhiteSpace(x.InstagramUrl), () =>
        {
            RuleFor(x => x.InstagramUrl)
                .Must(BeAValidUrl).WithMessage("Geçerli bir Instagram URL'i giriniz.")
                .Must(url => url!.Contains("instagram.com")).WithMessage("Instagram URL'i instagram.com içermelidir.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeValidSector(string sector)
    {
        return Sectors.GetAll().Contains(sector);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private bool HaveAtLeastOneFieldToUpdate(UpdateCompanyCommand command)
    {
        return command.Name != null ||
               command.Sector != null ||
               command.Address != null ||
               command.PhoneNumber != null ||
               command.Email != null ||
               command.WebsiteUrl != null ||
               command.LogoUrl != null ||
               command.LinkedInUrl != null ||
               command.XUrl != null ||
               command.InstagramUrl != null ||
               command.FacebookUrl != null;
    }
}
