using RateTheWork.Domain.Entities;
using RateTheWork.Domain.Enums.Report;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Şikayet repository interface'i
/// </summary>
public interface IReportRepository : IRepository<Report>
{
    /// <summary>
    /// Bekleyen şikayetleri getirir
    /// </summary>
    Task<IReadOnlyList<Report>> GetPendingReportsAsync();

    /// <summary>
    /// Belirli bir hedef için şikayetleri getirir
    /// </summary>
    Task<IReadOnlyList<Report>> GetReportsByTargetAsync(string targetType, string targetId);

    /// <summary>
    /// Kullanıcının yaptığı şikayetleri getirir
    /// </summary>
    Task<IReadOnlyList<Report>> GetReportsByReporterAsync(string reporterId);

    /// <summary>
    /// Öncelikli şikayetleri getirir
    /// </summary>
    Task<IReadOnlyList<Report>> GetHighPriorityReportsAsync();

    /// <summary>
    /// Belirli durumdaki şikayetleri sayfalı getirir
    /// </summary>
    Task<(IReadOnlyList<Report> items, int totalCount)> GetReportsByStatusPagedAsync
    (
        ReportStatus status
        , int pageNumber
        , int pageSize
    );

    /// <summary>
    /// Kullanıcının aynı hedef için daha önce şikayet yapıp yapmadığını kontrol eder
    /// </summary>
    Task<bool> HasUserReportedTargetAsync(string userId, string targetType, string targetId);

    /// <summary>
    /// Belirli bir süre içinde yapılan şikayet sayısını getirir
    /// </summary>
    Task<int> GetReportCountInPeriodAsync(string reporterId, DateTime startDate, DateTime endDate);
}
