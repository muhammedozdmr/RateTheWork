namespace RateTheWork.Infrastructure.Configuration;

public class SecurityOptions
{
    public const string SectionName = "Security";

    public BcryptOptions BCrypt { get; set; } = new();
    public EncryptionOptions Encryption { get; set; } = new();
    public JwtOptions Jwt { get; set; } = new();
}

public class BcryptOptions
{
    public int WorkFactor { get; set; } = 12;
}

public class EncryptionOptions
{
    public string Key { get; set; } = string.Empty;
}

public class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "RateTheWork";
    public string Audience { get; set; } = "RateTheWork";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 30;
}
