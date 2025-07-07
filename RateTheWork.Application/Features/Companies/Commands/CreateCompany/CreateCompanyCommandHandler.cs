using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.Companies.Commands.CreateCompany;
/// <summary>
/// Yeni şirket ekleme komutu
/// </summary>
public record CreateCompanyCommand : IRequest<CreateCompanyResult>
{
    /// <summary>
    /// Şirket tam adı
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Vergi kimlik numarası
    /// </summary>
    public string TaxId { get; init; } = string.Empty;
    
    /// <summary>
    /// MERSİS numarası
    /// </summary>
    public string MersisNo { get; init; } = string.Empty;
    
    /// <summary>
    /// Sektör
    /// </summary>
    public string Sector { get; init; } = string.Empty;
    
    /// <summary>
    /// Tam adres
    /// </summary>
    public string Address { get; init; } = string.Empty;
    
    /// <summary>
    /// Telefon numarası
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Email adresi
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Web sitesi
    /// </summary>
    public string WebsiteUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Logo URL (opsiyonel)
    /// </summary>
    public string? LogoUrl { get; init; }
    
    /// <summary>
    /// LinkedIn URL (opsiyonel)
    /// </summary>
    public string? LinkedInUrl { get; init; }
    
    /// <summary>
    /// X (Twitter) URL (opsiyonel)
    /// </summary>
    public string? XUrl { get; init; }
    
    /// <summary>
    /// Instagram URL (opsiyonel)
    /// </summary>
    public string? InstagramUrl { get; init; }
    
    /// <summary>
    /// Facebook URL (opsiyonel)
    /// </summary>
    public string? FacebookUrl { get; init; }
}

/// <summary>
/// Şirket ekleme sonucu
/// </summary>
public record CreateCompanyResult
{
    /// <summary>
    /// Oluşturulan şirketin ID'si
    /// </summary>
    public string? CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Admin onayı bekliyor mu?
    /// </summary>
    public bool PendingApproval { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
/// <summary>
/// CreateCompany command handler
/// </summary>
public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CreateCompanyResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateCompanyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateCompanyResult> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı girişi kontrolü
        if (!_currentUserService.IsAuthenticated)
        {
            throw new ForbiddenAccessException("Şirket eklemek için giriş yapmalısınız.");
        }

        // 2. Email doğrulaması kontrolü
        if (!_currentUserService.IsAuthenticated || !bool.Parse(_currentUserService.Roles.FirstOrDefault(r => r == "EmailVerified") ?? "false"))
        {
            throw new BusinessRuleException("EMAIL_NOT_VERIFIED", "Şirket eklemek için email adresinizi doğrulamalısınız.");
        }

        // 3. Vergi numarası duplicate kontrolü
        if (await _unitOfWork.Companies.IsTaxIdTakenAsync(request.TaxId))
        {
            throw new BusinessRuleException("TAX_ID_EXISTS", "Bu vergi numarası ile kayıtlı bir şirket zaten mevcut.");
        }

        // 4. MERSİS numarası duplicate kontrolü
        if (await _unitOfWork.Companies.IsMersisNoTakenAsync(request.MersisNo))
        {
            throw new BusinessRuleException("MERSIS_EXISTS", "Bu MERSİS numarası ile kayıtlı bir şirket zaten mevcut.");
        }

        // 5. Yeni şirket oluştur
        var company = new Company(
            name: request.Name.Trim(),
            taxId: request.TaxId.Trim(),
            mersisNo: request.MersisNo.Trim(),
            sector: request.Sector,
            address: request.Address.Trim(),
            phoneNumber: request.PhoneNumber.Trim(),
            email: request.Email.Trim().ToLowerInvariant(),
            websiteUrl: request.WebsiteUrl.Trim()
        );

        // 6. Opsiyonel alanları set et
        if (!string.IsNullOrWhiteSpace(request.LogoUrl))
            company.LogoUrl = request.LogoUrl.Trim();
            
        if (!string.IsNullOrWhiteSpace(request.LinkedInUrl))
            company.LinkedInUrl = request.LinkedInUrl.Trim();
            
        if (!string.IsNullOrWhiteSpace(request.XUrl))
            company.XUrl = request.XUrl.Trim();
            
        if (!string.IsNullOrWhiteSpace(request.InstagramUrl))
            company.InstagramUrl = request.InstagramUrl.Trim();
            
        if (!string.IsNullOrWhiteSpace(request.FacebookUrl))
            company.FacebookUrl = request.FacebookUrl.Trim();

        // 7. Audit bilgileri
        company.CreatedBy = _currentUserService.UserId;
        company.IsApproved = false; // Admin onayı bekleyecek

        // 8. Veritabanına kaydet
        await _unitOfWork.Companies.AddAsync(company);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Admin'lere bildirim gönder
        // TODO: Send notification to admins for company approval
        // await _notificationService.NotifyAdminsForCompanyApproval(company.Id);

        return new CreateCompanyResult
        {
            CompanyId = company.Id,
            Success = true,
            PendingApproval = true,
            Message = "Şirket başarıyla eklendi. Admin onayından sonra yayınlanacaktır."
        };
    }
}

/// <summary>
/// CreateCompany validation kuralları
/// </summary>
public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        // Şirket adı
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Şirket adı zorunludur.")
            .MinimumLength(3).WithMessage("Şirket adı en az 3 karakter olmalıdır.")
            .MaximumLength(200).WithMessage("Şirket adı 200 karakterden uzun olamaz.");

        // Vergi numarası
        RuleFor(x => x.TaxId)
            .NotEmpty().WithMessage("Vergi numarası zorunludur.")
            .Length(10).WithMessage("Vergi numarası 10 haneli olmalıdır.")
            .Matches(@"^\d{10}$").WithMessage("Vergi numarası sadece rakamlardan oluşmalıdır.");

        // MERSİS numarası
        RuleFor(x => x.MersisNo)
            .NotEmpty().WithMessage("MERSİS numarası zorunludur.")
            .Length(16).WithMessage("MERSİS numarası 16 haneli olmalıdır.")
            .Matches(@"^\d{16}$").WithMessage("MERSİS numarası sadece rakamlardan oluşmalıdır.");

        // Sektör
        RuleFor(x => x.Sector)
            .NotEmpty().WithMessage("Sektör bilgisi zorunludur.")
            .Must(BeValidSector).WithMessage("Geçerli bir sektör seçiniz.");

        // Adres
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MinimumLength(20).WithMessage("Adres en az 20 karakter olmalıdır.")
            .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");

        // Telefon
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur.")
            .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz.");

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.")
            .MaximumLength(100).WithMessage("Email adresi 100 karakterden uzun olamaz.");

        // Website
        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Web sitesi zorunludur.")
            .Must(BeAValidUrl).WithMessage("Geçerli bir web sitesi URL'i giriniz.");

        // Opsiyonel URL'ler
        When(x => !string.IsNullOrWhiteSpace(x.LogoUrl), () =>
        {
            RuleFor(x => x.LogoUrl)
                .Must(BeAValidUrl).WithMessage("Geçerli bir logo URL'i giriniz.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl), () =>
        {
            RuleFor(x => x.LinkedInUrl)
                .Must(BeAValidUrl).WithMessage("Geçerli bir LinkedIn URL'i giriniz.")
                .Must(url => url!.Contains("linkedin.com")).WithMessage("LinkedIn URL'i linkedin.com içermelidir.");
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
}
