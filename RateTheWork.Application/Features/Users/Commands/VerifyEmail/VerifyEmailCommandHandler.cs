using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Domain.Interfaces;
using ValidationException = RateTheWork.Application.Common.Exceptions.ValidationException;

namespace RateTheWork.Application.Features.Users.Commands.VerifyEmail;
/// <summary>
/// Email doğrulama komutu
/// </summary>
public record VerifyEmailCommand : IRequest<VerifyEmailResult>
{
    /// <summary>
    /// Kullanıcı ID'si
    /// </summary>
    public string UserId { get; init; } = string.Empty;
    
    /// <summary>
    /// Email doğrulama token'ı
    /// </summary>
    public string VerificationToken { get; init; } = string.Empty;
}

/// <summary>
/// Email doğrulama sonucu
/// </summary>
public record VerifyEmailResult
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// VerifyEmail command handler
/// </summary>
public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        // 1. Kullanıcıyı bul
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // 2. Email zaten doğrulanmış mı?
        if (user.IsEmailVerified)
        {
            return new VerifyEmailResult
            {
                Success = true,
                Message = "Email adresiniz zaten doğrulanmış."
            };
        }

        // 3. Token kontrolü
        if (string.IsNullOrEmpty(user.EmailVerificationToken) || 
            user.EmailVerificationToken != request.VerificationToken)
        {
            throw new ValidationException("Geçersiz doğrulama kodu.");
        }

        // 4. Token süresi dolmuş mu?
        if (user.EmailVerificationTokenExpiry.HasValue && 
            user.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
        {
            throw new ValidationException("Doğrulama kodunun süresi dolmuş. Lütfen yeni kod talep edin.");
        }

        // 5. Email'i doğrula
        user.IsEmailVerified = true;
        user.EmailVerificationToken = null; // Token'ı temizle
        user.EmailVerificationTokenExpiry = null;
        user.ModifiedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Hoşgeldin rozeti ver (ileride)
        // TODO: Award welcome badge
        // await _mediator.Send(new AwardBadgeCommand { UserId = user.Id, BadgeType = "Welcome" });

        return new VerifyEmailResult
        {
            Success = true,
            Message = "Email adresiniz başarıyla doğrulandı! Artık tüm özellikleri kullanabilirsiniz."
        };
    }
}

/// <summary>
/// VerifyEmail validation kuralları
/// </summary>
public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Doğrulama bağlantısı geçersiz. Lütfen email'inizdeki bağlantıya tekrar tıklayınız.")
            .Must(BeAValidGuid).WithMessage("Doğrulama bağlantısı hatalı. Lütfen yeni doğrulama emaili talep ediniz.");

        RuleFor(x => x.VerificationToken)
            .NotEmpty().WithMessage("Doğrulama kodu eksik. Lütfen email'inizdeki bağlantıya tıklayınız.")
            .Length(32).WithMessage("Doğrulama kodu geçersiz. Lütfen yeni doğrulama emaili talep ediniz.");
    }

    private bool BeAValidGuid(string id)
    {
        return Guid.TryParse(id, out _);
    }
}
