using MediatR;
using RateTheWork.Application.Common.Exceptions;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Interfaces;

namespace RateTheWork.Application.Features.CVApplications.Commands.ViewCVApplication;

/// <summary>
/// CV başvurusunu görüntüleme komutu
/// </summary>
public record ViewCVApplicationCommand : IRequest<ViewCVApplicationResult>
{
    /// <summary>
    /// CV başvuru ID'si
    /// </summary>
    public string ApplicationId { get; init; } = string.Empty;
}

/// <summary>
/// CV görüntüleme sonucu
/// </summary>
public record ViewCVApplicationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime ViewedAt { get; init; }
    public bool IsFirstView { get; init; }
}

/// <summary>
/// CV görüntüleme handler
/// </summary>
public class ViewCVApplicationCommandHandler : IRequestHandler<ViewCVApplicationCommand, ViewCVApplicationResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ViewCVApplicationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ViewCVApplicationResult> Handle(ViewCVApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. CV başvurusunu getir
        var cvApplication = await _unitOfWork.CVApplications.GetByIdAsync(request.ApplicationId);
        if (cvApplication == null)
        {
            throw new NotFoundException("CV başvurusu bulunamadı.");
        }

        // 2. Yetki kontrolü - Sadece şirket yetkilileri görüntüleyebilir
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUserService.UserId!);
        if (user == null)
        {
            throw new UnauthorizedException("Bu işlem için yetkiniz yok.");
        }

        // Şirket yetkilisi mi kontrol et
        var hasCompanyRole = user.UserRoles.Any(role => 
            role == "CompanyAdmin" || 
            role == "CompanyHR" || 
            role == "CompanyManager");

        if (!hasCompanyRole)
        {
            throw new ForbiddenAccessException("CV'leri görüntülemek için şirket yetkilisi olmalısınız.");
        }

        // TODO: Kullanıcının bu şirkette yetkili olup olmadığını kontrol et
        // Bu kontrol için UserCompany gibi bir ilişki tablosu gerekebilir

        // 3. İlk görüntüleme mi kontrol et
        var isFirstView = !cvApplication.ViewedAt.HasValue;

        // 4. Görüntülendi olarak işaretle
        cvApplication.MarkAsViewed(_currentUserService.UserId!);

        // 5. Değişiklikleri kaydet
        await _unitOfWork.CVApplications.UpdateAsync(cvApplication);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Bildirim oluştur (kullanıcıya CV'sinin görüntülendiğini bildir)
        var notification = Notification.Create(
            userId: cvApplication.UserId,
            title: "CV'niz Görüntülendi",
            message: $"CV'niz {cvApplication.Company?.Name ?? "şirket"} tarafından görüntülendi.",
            type: Domain.Enums.Notification.NotificationType.CVViewed,
            relatedEntityId: cvApplication.Id,
            relatedEntityType: "CVApplication"
        );

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ViewCVApplicationResult
        {
            Success = true,
            Message = isFirstView ? "CV ilk kez görüntülendi." : "CV daha önce görüntülenmişti.",
            ViewedAt = cvApplication.ViewedAt!.Value,
            IsFirstView = isFirstView
        };
    }
}