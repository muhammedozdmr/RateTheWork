using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;
using ValidationException = RateTheWork.Application.Common.Exceptions.ValidationException;

namespace RateTheWork.Application.Features.Users.Commands.ChangePassword;
/// <summary>
/// Şifre değiştirme komutu
/// </summary>
public record ChangePasswordCommand : IRequest<ChangePasswordResult>
{
    /// <summary>
    /// Mevcut şifre
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;
    
    /// <summary>
    /// Yeni şifre
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
    
    /// <summary>
    /// Yeni şifre tekrarı (onay için)
    /// </summary>
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

/// <summary>
/// Şifre değiştirme sonucu
/// </summary>
public record ChangePasswordResult
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
/// ChangePassword command handler
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHashingService _passwordHashingService;

    public ChangePasswordCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPasswordHashingService passwordHashingService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _passwordHashingService = passwordHashingService;
    }

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Current user kontrolü
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Bu işlem için giriş yapmalısınız.");
        }

        // 2. Kullanıcıyı getir
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // 3. Mevcut şifre kontrolü
        if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.HashedPassword))
        {
            throw new ValidationException("Mevcut şifreniz hatalı.");
        }

        // 4. Yeni şifre mevcut şifre ile aynı mı?
        if (_passwordHashingService.VerifyPassword(request.NewPassword, user.HashedPassword))
        {
            throw new ValidationException("Yeni şifreniz mevcut şifrenizle aynı olamaz.");
        }

        // 5. Yeni şifreyi hash'le ve güncelle
        user.HashedPassword = _passwordHashingService.HashPassword(request.NewPassword);
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = _currentUserService.UserId;

        // 6. Değişiklikleri kaydet
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 7. Güvenlik için tüm refresh token'ları iptal et (kullanıcı tüm cihazlardan çıkış yapsın)
        // TODO: Revoke all refresh tokens for this user
        // await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id);

        // 8. Email bildirimi gönder
        // TODO: Send password changed notification email
        // await _emailService.SendPasswordChangedNotificationAsync(user.Email);

        return new ChangePasswordResult
        {
            Success = true,
            Message = "Şifreniz başarıyla değiştirildi. Güvenliğiniz için tüm cihazlardan çıkış yapmanız gerekecek."
        };
    }
}

/// <summary>
/// ChangePassword validation kuralları
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        // Mevcut şifre
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre zorunludur.");

        // Yeni şifre
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre zorunludur.")
            .MinimumLength(8).WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
            .MaximumLength(100).WithMessage("Yeni şifre 100 karakterden uzun olamaz.")
            .Matches(@"[A-Z]").WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
            .Matches(@"[a-z]").WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
            .Matches(@"[0-9]").WithMessage("Yeni şifre en az bir rakam içermelidir.")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Yeni şifre en az bir özel karakter içermelidir.");

        // Şifre onayı
        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Şifre tekrarı zorunludur.")
            .Equal(x => x.NewPassword).WithMessage("Şifreler eşleşmiyor.");

        // Yeni şifre mevcut şifreden farklı olmalı
        RuleFor(x => x)
            .Must(x => x.CurrentPassword != x.NewPassword)
            .WithMessage("Yeni şifre mevcut şifreden farklı olmalıdır.")
            .WithName("NewPassword");
    }
}
