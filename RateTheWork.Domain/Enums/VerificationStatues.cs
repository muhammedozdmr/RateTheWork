namespace RateTheWork.Domain.Enums;

/// <summary>
/// Doğrulama talebi durumları
/// </summary>
public static class VerificationStatuses
{
    public const string Pending = "Beklemede";
    public const string Approved = "Onaylandı";
    public const string Rejected = "Reddedildi";
}
