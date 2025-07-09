using System.Linq.Expressions;

namespace RateTheWork.Domain.Interfaces.Infrastructure;

/// <summary>
/// Background job service interface'i
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Job'ı hemen çalıştırır
    /// </summary>
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);
    
    /// <summary>
    /// Job'ı zamanlanmış olarak ekler
    /// </summary>
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    
    /// <summary>
    /// Tekrarlı job ekler
    /// </summary>
    Task AddRecurringAsync<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    
    /// <summary>
    /// Job'ı iptal eder
    /// </summary>
    Task<bool> DeleteAsync(string jobId);
}
