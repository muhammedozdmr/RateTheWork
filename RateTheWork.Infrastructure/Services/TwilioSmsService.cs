using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RateTheWork.Application.Common.Interfaces;
using RateTheWork.Infrastructure.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RateTheWork.Infrastructure.Services;

public class TwilioSmsService : ISmsService
{
    private readonly ILogger<TwilioSmsService> _logger;
    private readonly SmsOptions _smsOptions;

    public TwilioSmsService
    (
        IOptions<SmsOptions> smsOptions
        , ILogger<TwilioSmsService> logger
    )
    {
        _logger = logger;
        _smsOptions = smsOptions.Value;

        if (string.IsNullOrEmpty(_smsOptions.Twilio.AccountSid))
            throw new InvalidOperationException("Twilio Account SID not configured");
        if (string.IsNullOrEmpty(_smsOptions.Twilio.AuthToken))
            throw new InvalidOperationException("Twilio Auth Token not configured");
        if (string.IsNullOrEmpty(_smsOptions.Twilio.FromNumber))
            throw new InvalidOperationException("Twilio From Number not configured");

        TwilioClient.Init(_smsOptions.Twilio.AccountSid, _smsOptions.Twilio.AuthToken);
    }

    public async Task<bool> SendSmsAsync
        (string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedNumber = NormalizePhoneNumber(phoneNumber);

            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_smsOptions.Twilio.FromNumber),
                to: new PhoneNumber(normalizedNumber)
            );

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}, MessageSid: {MessageSid}",
                phoneNumber, messageResource.Sid);

            return messageResource.Status != MessageResource.StatusEnum.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var message = $"Your RateTheWork verification code is: {code}. This code will expire in 10 minutes.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    public async Task<Dictionary<string, bool>> SendBulkSmsAsync
    (
        Dictionary<string, string> phoneNumbersAndMessages
        , CancellationToken cancellationToken = default
    )
    {
        var results = new Dictionary<string, bool>();

        foreach (var kvp in phoneNumbersAndMessages)
        {
            try
            {
                var result = await SendSmsAsync(kvp.Key, kvp.Value, cancellationToken);
                results[kvp.Key] = result;
            }
            catch
            {
                results[kvp.Key] = false;
            }
        }

        return results;
    }

    public async Task<SmsStatus> GetSmsStatusAsync(string messageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = await MessageResource.FetchAsync(messageId);

            // Convert status string to enum
            if (message.Status == MessageResource.StatusEnum.Queued)
                return SmsStatus.Pending;
            else if (message.Status == MessageResource.StatusEnum.Sending)
                return SmsStatus.Pending;
            else if (message.Status == MessageResource.StatusEnum.Sent)
                return SmsStatus.Sent;
            else if (message.Status == MessageResource.StatusEnum.Delivered)
                return SmsStatus.Delivered;
            else if (message.Status == MessageResource.StatusEnum.Failed)
                return SmsStatus.Failed;
            else if (message.Status == MessageResource.StatusEnum.Undelivered)
                return SmsStatus.Failed;
            else
                return SmsStatus.Unknown;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SMS status for MessageId: {MessageId}", messageId);
            return SmsStatus.Unknown;
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        // Remove spaces and hyphens
        phoneNumber = phoneNumber.Replace(" ", "").Replace("-", "");

        // If number starts with 0, assume it's Turkish and add +90
        if (phoneNumber.StartsWith("0"))
        {
            phoneNumber = "+90" + phoneNumber.Substring(1);
        }

        // If number doesn't start with +, add +
        if (!phoneNumber.StartsWith("+"))
        {
            phoneNumber = "+" + phoneNumber;
        }

        return phoneNumber;
    }
}
