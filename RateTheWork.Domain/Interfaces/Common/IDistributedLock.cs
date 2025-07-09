namespace RateTheWork.Domain.Interfaces.Common;

/// <summary>
/// Distributed lock interface'i
/// </summary>
public interface IDistributedLock : IDisposable
{
    string Resource { get; }
    string LockId { get; }
    DateTime AcquiredAt { get; }
    TimeSpan Duration { get; }
}
