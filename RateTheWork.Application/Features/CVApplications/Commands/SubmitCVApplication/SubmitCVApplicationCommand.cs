using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.CVApplications.Commands.SubmitCVApplication;

/// <summary>
/// CV başvurusu gönderme komutu
/// </summary>
public record SubmitCVApplicationCommand : IRequest<SubmitCVApplicationResult>
{
    /// <summary>
    /// Başvuru yapılacak şirket ID'si
    /// </summary>
    public string CompanyId { get; init; } = string.Empty;
    
    /// <summary>
    /// Başvuru yapılacak departman ID'leri (max 3)
    /// </summary>
    public List<string> DepartmentIds { get; init; } = new();
    
    /// <summary>
    /// Başvuran adı (kullanıcı profil bilgilerinden gelmiyorsa)
    /// </summary>
    public string? ApplicantName { get; init; }
    
    /// <summary>
    /// Başvuran email (kullanıcı profil bilgilerinden gelmiyorsa)
    /// </summary>
    public string? ApplicantEmail { get; init; }
    
    /// <summary>
    /// Başvuran telefon (kullanıcı profil bilgilerinden gelmiyorsa)
    /// </summary>
    public string? ApplicantPhone { get; init; }
    
    /// <summary>
    /// Başvuran web sitesi
    /// </summary>
    public string? ApplicantWebsite { get; init; }
    
    /// <summary>
    /// CV dosya URL'i
    /// </summary>
    public string CVFileUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Motivasyon mektubu URL'i (opsiyonel)
    /// </summary>
    public string? MotivationLetterUrl { get; init; }
    
    /// <summary>
    /// Kayıtlı bilgileri kullan
    /// </summary>
    public bool UseRegisteredInfo { get; init; } = false;
    
    /// <summary>
    /// Ek bilgiler
    /// </summary>
    public Dictionary<string, string>? AdditionalInfo { get; init; }
}

/// <summary>
/// CV başvurusu gönderme sonucu
/// </summary>
public record SubmitCVApplicationResult
{
    /// <summary>
    /// Başvuru ID'si
    /// </summary>
    public string ApplicationId { get; init; } = string.Empty;
    
    /// <summary>
    /// Başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Mesaj
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Başvuru son tarihi (90 gün sonra)
    /// </summary>
    public DateTime ExpiryDate { get; init; }
}

/// <summary>
/// CV başvurusu gönderme handler
/// </summary>
public class SubmitCVApplicationCommandHandler : IRequestHandler<SubmitCVApplicationCommand, SubmitCVApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAICVAnalysisService _aiCVService;

    public SubmitCVApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAICVAnalysisService aiCVService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _aiCVService = aiCVService;
    }

    public async Task<SubmitCVApplicationResult> Handle(SubmitCVApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcı doğrulama
        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new UnauthorizedException("Bu işlem için giriş yapmalısınız.");
        }

        // 2. Şirket kontrolü
        var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
        if (company == null)
        {
            throw new NotFoundException("Şirket bulunamadı.");
        }

        if (!company.IsActive)
        {
            throw new BusinessRuleException("INACTIVE_COMPANY", "Bu şirket şu anda başvuru kabul etmiyor.");
        }

        // 3. Departman kontrolü
        var departments = new List<Department>();
        foreach (var deptId in request.DepartmentIds)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(deptId);
            if (department == null || department.CompanyId != request.CompanyId)
            {
                throw new NotFoundException($"Departman bulunamadı: {deptId}");
            }
            
            if (!department.IsActive)
            {
                throw new BusinessRuleException("INACTIVE_DEPARTMENT", $"'{department.Name}' departmanı şu anda başvuru kabul etmiyor.");
            }
            
            departments.Add(department);
        }

        // 4. Daha önce başvuru yapılmış mı kontrol et (30 gün içinde)
        var hasRecentApplication = await _unitOfWork.CVApplications.HasUserAppliedToCompanyAsync(
            _currentUserService.UserId, 
            request.CompanyId, 
            DateTime.UtcNow.AddDays(-30));

        if (hasRecentApplication)
        {
            throw new BusinessRuleException("RECENT_APPLICATION", "Bu şirkete son 30 gün içinde başvuru yapmışsınız.");
        }

        // 5. Başvuran bilgilerini hazırla
        string applicantName;
        string applicantEmail;
        string applicantPhone;

        if (request.UseRegisteredInfo)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId);
            if (user == null)
            {
                throw new NotFoundException("Kullanıcı bulunamadı.");
            }

            applicantName = user.FullName ?? user.AnonymousUsername;
            applicantEmail = user.Email;
            applicantPhone = user.PhoneNumber ?? request.ApplicantPhone ?? "";
        }
        else
        {
            applicantName = request.ApplicantName ?? "";
            applicantEmail = request.ApplicantEmail ?? "";
            applicantPhone = request.ApplicantPhone ?? "";
        }

        // 6. CV başvurusunu oluştur
        var cvApplication = CVApplication.Create(
            userId: _currentUserService.UserId,
            companyId: request.CompanyId,
            departmentIds: request.DepartmentIds,
            applicantName: applicantName,
            applicantEmail: applicantEmail,
            applicantPhone: applicantPhone,
            cvFileUrl: request.CVFileUrl,
            applicantWebsite: request.ApplicantWebsite,
            motivationLetterUrl: request.MotivationLetterUrl,
            additionalInfo: request.AdditionalInfo
        );

        // 7. Veritabanına kaydet
        await _unitOfWork.CVApplications.AddAsync(cvApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. AI ile CV analizi başlat (arka planda)
        _ = Task.Run(async () =>
        {
            try
            {
                var analysisResult = await _aiCVService.AnalyzeCV(
                    request.CVFileUrl, 
                    request.DepartmentIds);
                
                // Analiz sonuçlarını kaydet (ileride kullanılmak üzere)
            }
            catch
            {
                // Hata durumunda log at ama başvuruyu etkileme
            }
        });

        return new SubmitCVApplicationResult
        {
            ApplicationId = cvApplication.Id,
            Success = true,
            Message = $"CV başvurunuz {departments.Count} departmana başarıyla gönderildi.",
            ExpiryDate = cvApplication.ExpiryDate
        };
    }
}

/// <summary>
/// CV başvurusu validasyon kuralları
/// </summary>
public class SubmitCVApplicationCommandValidator : AbstractValidator<SubmitCVApplicationCommand>
{
    public SubmitCVApplicationCommandValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty().WithMessage("Şirket seçilmelidir.")
            .Must(BeAValidGuid).WithMessage("Geçersiz şirket ID'si.");

        RuleFor(x => x.DepartmentIds)
            .NotEmpty().WithMessage("En az bir departman seçilmelidir.")
            .Must(x => x.Count <= 3).WithMessage("En fazla 3 departmana başvuru yapabilirsiniz.")
            .Must(x => x.Distinct().Count() == x.Count).WithMessage("Aynı departmana birden fazla başvuru yapamazsınız.");

        RuleFor(x => x.CVFileUrl)
            .NotEmpty().WithMessage("CV dosyası yüklenmelidir.")
            .Must(BeAValidUrl).WithMessage("Geçersiz CV dosya URL'i.");

        When(x => !x.UseRegisteredInfo, () =>
        {
            RuleFor(x => x.ApplicantName)
                .NotEmpty().WithMessage("İsim girilmelidir.")
                .MinimumLength(3).WithMessage("İsim en az 3 karakter olmalıdır.")
                .MaximumLength(100).WithMessage("İsim 100 karakteri aşamaz.");

            RuleFor(x => x.ApplicantEmail)
                .NotEmpty().WithMessage("Email girilmelidir.")
                .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

            RuleFor(x => x.ApplicantPhone)
                .NotEmpty().WithMessage("Telefon numarası girilmelidir.")
                .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ApplicantWebsite), () =>
        {
            RuleFor(x => x.ApplicantWebsite)
                .Must(BeAValidUrl).WithMessage("Geçerli bir web sitesi adresi giriniz.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.MotivationLetterUrl), () =>
        {
            RuleFor(x => x.MotivationLetterUrl)
                .Must(BeAValidUrl).WithMessage("Geçersiz motivasyon mektubu URL'i.");
        });
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }

    private bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return true;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}