namespace RateTheWork.Infrastructure.Configuration;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public bool UseRedis { get; set; } = false;
    public string? RedisConnectionString { get; set; }
    public int DefaultExpirationMinutes { get; set; } = 30;
    public RedisOptions Redis { get; set; } = new();
}

public class RedisOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6379;
    public string? Password { get; set; }
    public int Database { get; set; } = 0;
    public bool Ssl { get; set; } = false;
    public int ConnectTimeout { get; set; } = 5000;
    public int SyncTimeout { get; set; } = 5000;
}
