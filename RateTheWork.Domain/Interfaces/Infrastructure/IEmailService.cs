namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Email gönderim service interface'i
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Email gönderir
    /// </summary>
    Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Toplu email gönderimi
    /// </summary>
    Task<Dictionary<string, bool>> SendBulkEmailAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Email template ile gönderim
    /// </summary>
    Task<bool> SendTemplateEmailAsync(string templateId, string to, Dictionary<string, object> data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Email mesajı
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public string? From { get; set; }
    public string? ReplyTo { get; set; }
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public List<EmailAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// Email eki
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}