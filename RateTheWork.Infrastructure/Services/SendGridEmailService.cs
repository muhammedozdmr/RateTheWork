using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Infrastructure.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace RateTheWork.Infrastructure.Services;

public class SendGridEmailService : IEmailService
{
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<SendGridEmailService> _logger;
    private readonly ISendGridClient _sendGridClient;

    public SendGridEmailService
    (
        IOptions<EmailOptions> emailOptions
        , ILogger<SendGridEmailService> logger
    )
    {
        _logger = logger;
        _emailOptions = emailOptions.Value;

        if (string.IsNullOrEmpty(_emailOptions.SendGrid.ApiKey))
            throw new InvalidOperationException("SendGrid API Key not configured");

        _sendGridClient = new SendGridClient(_emailOptions.SendGrid.ApiKey);
    }

    public async Task SendEmailAsync
        (string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var message = new EmailMessage
        {
            To = to, Subject = subject, Body = body, IsHtml = isHtml
        };

        await SendEmailAsync(message, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(message.From ?? _emailOptions.SendGrid.FromEmail
                    , _emailOptions.SendGrid.FromName)
                , Subject = message.Subject
            };

            msg.AddTo(new EmailAddress(message.To));

            if (!string.IsNullOrEmpty(message.ReplyTo))
            {
                msg.ReplyTo = new EmailAddress(message.ReplyTo);
            }

            foreach (var cc in message.Cc)
            {
                msg.AddCc(new EmailAddress(cc));
            }

            foreach (var bcc in message.Bcc)
            {
                msg.AddBcc(new EmailAddress(bcc));
            }

            if (message.IsHtml)
            {
                msg.HtmlContent = message.Body;
            }
            else
            {
                msg.PlainTextContent = message.Body;
            }

            foreach (var attachment in message.Attachments)
            {
                msg.AddAttachment(new Attachment
                {
                    Content = Convert.ToBase64String(attachment.Content), Filename = attachment.FileName
                    , Type = attachment.ContentType
                });
            }

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully to {To}", message.To);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send email. Status: {Status}, Body: {Body}", response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", message.To);
            throw;
        }
    }

    public async Task<Dictionary<string, bool>> SendBulkEmailAsync
        (IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>();

        foreach (var message in messages)
        {
            try
            {
                var result = await SendEmailAsync(message, cancellationToken);
                results[message.To] = result;
            }
            catch
            {
                results[message.To] = false;
            }
        }

        return results;
    }

    public async Task SendTemplatedEmailAsync(string to, string templateName, object templateData)
    {
        var templateId = _emailOptions.SendGrid.Templates?.GetValueOrDefault(templateName) ??
                         throw new InvalidOperationException($"Template {templateName} not configured");

        var data = templateData.GetType()
            .GetProperties()
            .ToDictionary(p => p.Name, p => p.GetValue(templateData) ?? new object());

        await SendTemplateEmailAsync(templateId, to, data);
    }

    public async Task<bool> SendTemplateEmailAsync
        (string templateId, string to, Dictionary<string, object> data, CancellationToken cancellationToken = default)
    {
        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_emailOptions.SendGrid.FromEmail, _emailOptions.SendGrid.FromName)
                , TemplateId = templateId
            };

            msg.AddTo(new EmailAddress(to));
            msg.SetTemplateData(data);

            var response = await _sendGridClient.SendEmailAsync(msg, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Template email sent successfully to {To} with template {TemplateId}", to
                    , templateId);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send template email. Status: {Status}, Body: {Body}", response.StatusCode
                    , body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template email to {To} with template {TemplateId}", to, templateId);
            throw;
        }
    }
}
