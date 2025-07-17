namespace RateTheWork.Application.Common.Interfaces;

/// <summary>
/// Email gönderimi için abstraction.
/// Infrastructure katmanında SMTP veya SendGrid gibi servislerle implemente edilir.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Email gönderir
    /// </summary>
    /// <param name="to">Alıcı email adresi</param>
    /// <param name="subject">Email konusu</param>
    /// <param name="body">Email içeriği (HTML olabilir)</param>
    /// <param name="isHtml">Email HTML formatında mı?</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Email mesajı ile gönderim
    /// </summary>
    Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toplu email gönderimi
    /// </summary>
    Task<Dictionary<string, bool>> SendBulkEmailAsync
        (IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Email şablonu ile email gönderir
    /// </summary>
    /// <param name="to">Alıcı email adresi</param>
    /// <param name="templateName">Şablon adı</param>
    /// <param name="templateData">Şablon değişkenleri</param>
    Task SendTemplatedEmailAsync(string to, string templateName, object templateData);

    /// <summary>
    /// Email template ile gönderim
    /// </summary>
    Task<bool> SendTemplateEmailAsync
        (string templateId, string to, Dictionary<string, object> data, CancellationToken cancellationToken = default);
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
