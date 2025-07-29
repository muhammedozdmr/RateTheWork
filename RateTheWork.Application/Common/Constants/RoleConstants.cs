namespace RateTheWork.Application.Common.Constants;

/// <summary>
/// Uygulama genelinde kullanılan rol sabitleri
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// Admin rolleri
    /// </summary>
    public static class AdminRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string ContentManager = "ContentManager";
        public const string CustomerSupport = "CustomerSupport";
    }
    
    /// <summary>
    /// Kullanıcı rolleri
    /// </summary>
    public static class UserRoles
    {
        public const string User = "User";
        public const string VerifiedUser = "VerifiedUser";
        public const string PremiumUser = "PremiumUser";
    }
    
    /// <summary>
    /// Şirket rolleri
    /// </summary>
    public static class CompanyRoles
    {
        public const string CompanyAdmin = "CompanyAdmin";
        public const string CompanyHR = "CompanyHR";
        public const string CompanyManager = "CompanyManager";
    }
}