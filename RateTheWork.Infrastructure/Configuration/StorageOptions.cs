namespace RateTheWork.Infrastructure.Configuration;

public class StorageOptions
{
    public const string SectionName = "Storage";

    public string LocalPath { get; set; } = "wwwroot/uploads";
    public string BaseUrl { get; set; } = "/uploads";
}
