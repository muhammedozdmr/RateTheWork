using RateTheWork.Domain.Common;

namespace RateTheWork.Domain.Interfaces.Repositories;

/// <summary>
/// Tüm repository işlemleri için birleşik interface
/// </summary>
/// <typeparam name="T">Entity tipi (BaseEntity'den türemeli)</typeparam>
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : BaseEntity
{
}
