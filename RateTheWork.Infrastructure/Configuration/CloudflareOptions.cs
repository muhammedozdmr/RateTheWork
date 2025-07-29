namespace RateTheWork.Infrastructure.Configuration;

public class CloudflareOptions
{
    public const string SectionName = "Cloudflare";

    public string AccountId { get; set; } = string.Empty;
    public string KvNamespaceId { get; set; } = string.Empty;
    public string KvApiToken { get; set; } = string.Empty;
}
