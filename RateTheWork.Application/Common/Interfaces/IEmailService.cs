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
    /// Email şablonu ile email gönderir
    /// </summary>
    /// <param name="to">Alıcı email adresi</param>
    /// <param name="templateName">Şablon adı</param>
    /// <param name="templateData">Şablon değişkenleri</param>
    Task SendTemplatedEmailAsync(string to, string templateName, object templateData);
}
