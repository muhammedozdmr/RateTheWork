using FluentValidation;
using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Interfaces;
using RateTheWork.Domain.Services;
using RateTheWork.Domain.Events.User;

namespace RateTheWork.Application.Features.Users.Commands.UpdateUserProfile;
/// <summary>
/// Kullanıcı profil güncelleme komutu
/// </summary>
public record UpdateUserProfileCommand : IRequest<UpdateUserProfileResult>
{
    /// <summary>
    /// Meslek bilgisi
    /// </summary>
    public string? Profession { get; init; }
    
    /// <summary>
    /// Telefon numarası (şifrelenecek)
    /// </summary>
    public string? PhoneNumber { get; init; }
    
    /// <summary>
    /// Adres (şifrelenecek)
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Şehir (şifrelenecek)
    /// </summary>
    public string? City { get; init; }
    
    /// <summary>
    /// İlçe (şifrelenecek)
    /// </summary>
    public string? District { get; init; }
}

/// <summary>
/// Profil güncelleme sonucu
/// </summary>
public record UpdateUserProfileResult
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
    /// Sonuç mesajı
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
/// <summary>
/// UpdateUserProfile command handler
/// </summary>
public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEncryptionService _encryptionService;

    public UpdateUserProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _encryptionService = encryptionService;
    }

    public async Task<UpdateUserProfileResult> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Current user'ı al
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Bu işlem için giriş yapmalısınız.");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId);
        
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        // 2. Hangi alanlar güncellendi sayacı
        var updatedFieldsCount = 0;
        // 3. Telefon numarası güncelleme (şifreli)
        var hasChanges = false;
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var encryptedPhone = _encryptionService.Encrypt(request.PhoneNumber);
            if (user.EncryptedPhoneNumber != encryptedPhone)
            {
                user.UpdatePhoneNumber(encryptedPhone);
                hasChanges = true;
                updatedFieldsCount++;
            }
        }

        // 4. Profil bilgileri güncelleme
        var encryptedAddress = !string.IsNullOrWhiteSpace(request.Address) 
            ? _encryptionService.Encrypt(request.Address) 
            : null;
        var encryptedCity = !string.IsNullOrWhiteSpace(request.City) 
            ? _encryptionService.Encrypt(request.City) 
            : null;
        var encryptedDistrict = !string.IsNullOrWhiteSpace(request.District) 
            ? _encryptionService.Encrypt(request.District) 
            : null;

        // UpdateProfile metodunu çağır
        user.UpdateProfile(
            profession: request.Profession,
            encryptedAddress: encryptedAddress,
            encryptedCity: encryptedCity,
            encryptedDistrict: encryptedDistrict
        );

        // Domain event'lerden değişiklik olup olmadığını kontrol et
        if (user.DomainEvents.Any(e => e is UserProfileUpdatedEvent))
        {
            hasChanges = true;
            // UpdateProfile içinde kaç alan güncellendiğini sayabiliriz
            var profileUpdatedEvent = user.DomainEvents.OfType<UserProfileUpdatedEvent>().FirstOrDefault();
            if (profileUpdatedEvent != null && profileUpdatedEvent.UpdatedFields != null)
            {
                updatedFieldsCount += profileUpdatedEvent.UpdatedFields.Length;
            }
        }

        // 5. Eğer hiç değişiklik yoksa
        if (!hasChanges)
        {
            return new UpdateUserProfileResult
            {
                Success = true,
                UpdatedFieldsCount = 0,
                Message = "Güncellenecek bir değişiklik bulunamadı."
            };
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateUserProfileResult
        {
            Success = true,
            UpdatedFieldsCount = updatedFieldsCount,
            Message = $"Profiliniz başarıyla güncellendi. {updatedFieldsCount} alan değiştirildi."
        };
    }
}

/// <summary>
/// UpdateUserProfile validation kuralları
/// </summary>
public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        // En az bir alan dolu olmalı
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Profession) ||
                      !string.IsNullOrWhiteSpace(x.PhoneNumber) ||
                      !string.IsNullOrWhiteSpace(x.Address) ||
                      !string.IsNullOrWhiteSpace(x.City) ||
                      !string.IsNullOrWhiteSpace(x.District))
            .WithMessage("En az bir alan güncellenmeli.");

        // Meslek validation
        When(x => !string.IsNullOrWhiteSpace(x.Profession), () =>
        {
            RuleFor(x => x.Profession)
                .MaximumLength(100).WithMessage("Meslek bilgisi 100 karakterden uzun olamaz.");
        });

        // Telefon validation
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^(\+90|0)?[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz.");
        });

        // Adres validation
        When(x => !string.IsNullOrWhiteSpace(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .MinimumLength(10).WithMessage("Adres en az 10 karakter olmalıdır.")
                .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");
        });

        // Şehir validation
        When(x => !string.IsNullOrWhiteSpace(x.City), () =>
        {
            RuleFor(x => x.City)
                .MaximumLength(50).WithMessage("Şehir bilgisi 50 karakterden uzun olamaz.");
        });

        // İlçe validation
        When(x => !string.IsNullOrWhiteSpace(x.District), () =>
        {
            RuleFor(x => x.District)
                .MaximumLength(50).WithMessage("İlçe bilgisi 50 karakterden uzun olamaz.");
        });
    }
}
