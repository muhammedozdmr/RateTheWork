namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// SMS gönderim service interface'i
/// Infrastructure katmanında SMS provider'ları ile implemente edilir.
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

    /// <summary>
    /// Toplu SMS gönderimi
    /// </summary>
    Task<Dictionary<string, bool>> SendBulkSmsAsync
        (Dictionary<string, string> phoneNumbersAndMessages, CancellationToken cancellationToken = default);

    /// <summary>
    /// SMS durumunu sorgular
    /// </summary>
    Task<SmsStatus> GetSmsStatusAsync(string messageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// SMS durumu
/// </summary>
public enum SmsStatus
{
    Pending
    , Sent
    , Delivered
    , Failed
    , Unknown
}
