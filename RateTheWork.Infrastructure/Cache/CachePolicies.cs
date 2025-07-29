namespace RateTheWork.Infrastructure.Cache;

/// <summary>
/// Önbellek politikaları ve sabitleri
/// </summary>
public static class CachePolicies
{
    /// <summary>
    /// Önbellek anahtarı oluşturur
    /// </summary>
    public static string GetKey(string prefix, params object[] values)
    {
        return prefix + string.Join(":", values);
    }

    /// <summary>
    /// Kullanıcı önbellek anahtarı oluşturur
    /// </summary>
    public static string GetUserKey(Guid userId) => GetKey(Keys.User, userId);

    /// <summary>
    /// Şirket önbellek anahtarı oluşturur
    /// </summary>
    public static string GetCompanyKey(Guid companyId) => GetKey(Keys.Company, companyId);

    /// <summary>
    /// İş ilanı önbellek anahtarı oluşturur
    /// </summary>
    public static string GetJobPostingKey(Guid jobId) => GetKey(Keys.JobPosting, jobId);

    /// <summary>
    /// İnceleme önbellek anahtarı oluşturur
    /// </summary>
    public static string GetReviewKey(Guid reviewId) => GetKey(Keys.Review, reviewId);

    /// <summary>
    /// Şube önbellek anahtarı oluşturur
    /// </summary>
    public static string GetBranchKey(Guid branchId) => GetKey(Keys.CompanyBranch, branchId);

    /// <summary>
    /// Liste önbellek anahtarı oluşturur
    /// </summary>
    public static string GetListKey(string entityType, params object[] filters)
    {
        var key = $"list:{entityType}";
        if (filters.Length > 0)
        {
            key += ":" + string.Join(":", filters);
        }

        return key;
    }

    /// <summary>
    /// Sayfalama önbellek anahtarı oluşturur
    /// </summary>
    public static string GetPagedKey(string entityType, int page, int pageSize, params object[] filters)
    {
        return GetListKey(entityType, filters) + $":page{page}:size{pageSize}";
    }

    /// <summary>
    /// Önbellek anahtarı önekleri
    /// </summary>
    public static class Keys
    {
        public const string User = "user:";
        public const string Company = "company:";
        public const string JobPosting = "jobposting:";
        public const string Review = "review:";
        public const string CompanyBranch = "branch:";
        public const string Department = "department:";
        public const string Badge = "badge:";
        public const string Notification = "notification:";
        public const string FeatureFlag = "featureflag:";
        public const string Configuration = "config:";
        public const string Report = "report:";
        public const string Metrics = "metrics:";
    }

    /// <summary>
    /// Önbellek süreleri (dakika cinsinden)
    /// </summary>
    public static class Durations
    {
        // Kısa süreli önbellek (5 dakika)
        public const int VeryShort = 5;

        // Kısa süreli önbellek (15 dakika)
        public const int Short = 15;

        // Orta süreli önbellek (30 dakika)
        public const int Medium = 30;

        // Uzun süreli önbellek (1 saat)
        public const int Long = 60;

        // Çok uzun süreli önbellek (4 saat)
        public const int VeryLong = 240;

        // Günlük önbellek (24 saat)
        public const int Daily = 1440;

        // Haftalık önbellek (7 gün)
        public const int Weekly = 10080;
    }

    /// <summary>
    /// Entity bazlı önbellek süreleri
    /// </summary>
    public static class EntityDurations
    {
        public const int User = Durations.Medium;
        public const int Company = Durations.Long;
        public const int JobPosting = Durations.Medium;
        public const int Review = Durations.Short;
        public const int CompanyBranch = Durations.Long;
        public const int Department = Durations.VeryLong;
        public const int Badge = Durations.Daily;
        public const int Configuration = Durations.VeryLong;
        public const int FeatureFlag = Durations.VeryShort;
        public const int Report = Durations.Long;
    }

    /// <summary>
    /// Cache tag'leri (grup bazlı temizleme için)
    /// </summary>
    public static class Tags
    {
        public const string UserData = "user-data";
        public const string CompanyData = "company-data";
        public const string JobData = "job-data";
        public const string ReviewData = "review-data";
        public const string ConfigurationData = "config-data";
        public const string ReportData = "report-data";
        public const string TransientData = "transient-data";
    }
}
