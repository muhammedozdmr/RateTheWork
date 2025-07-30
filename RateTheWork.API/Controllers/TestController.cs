using Microsoft.AspNetCore.Mvc;
using RateTheWork.Application.Common.Interfaces;

namespace RateTheWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TestController> _logger;
    private readonly ISmsService _smsService;

    public TestController
    (
        IEmailService emailService
        , ISmsService smsService
        , ILogger<TestController> logger
    )
    {
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    [HttpPost("email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            await _emailService.SendEmailAsync(
                request.To,
                request.Subject,
                request.Body,
                request.IsHtml);

            return Ok(new { message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing email service");
            return StatusCode(500, new { error = "An error occurred while sending email" });
        }
    }

    [HttpPost("sms")]
    public async Task<IActionResult> TestSms([FromBody] TestSmsRequest request)
    {
        try
        {
            var result = await _smsService.SendSmsAsync(request.To, request.Message);

            if (result)
            {
                return Ok(new { message = "SMS sent successfully" });
            }

            return BadRequest(new { error = "Failed to send SMS" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS service");
            return StatusCode(500, new { error = "An error occurred while sending SMS" });
        }
    }

    [HttpGet("config-status")]
    public IActionResult GetConfigStatus()
    {
        var configStatus = new
        {
            email = new
            {
                provider = "SendGrid"
                , configured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SENDGRID_API_KEY"))
            }
            , sms = new
            {
                provider = "Twilio"
                , configured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID"))
            }
            , cloudflare = new
            {
                configured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CLOUDFLARE_API_TOKEN"))
            }
            , database = new
            {
                configured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))
            }
            , redis = new
            {
                configured = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING"))
            }
        };

        return Ok(configStatus);
    }
}

public class TestEmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = false;
}

public class TestSmsRequest
{
    public string To { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
