namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// SMS gönderim service interface'i
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// SMS gönderir
    /// </summary>
    Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// OTP SMS gönderir
    /// </summary>
    Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
