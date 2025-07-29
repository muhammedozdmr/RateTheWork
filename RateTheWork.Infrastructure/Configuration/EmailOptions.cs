namespace RateTheWork.Infrastructure.Configuration;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "SendGrid";
    public SendGridOptions SendGrid { get; set; } = new();
}

public class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@ratethework.com";
    public string FromName { get; set; } = "RateTheWork";
    public Dictionary<string, string> Templates { get; set; } = new();
}
