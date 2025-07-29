using RateTheWork.Domain.Common;

namespace RateTheWork.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : BaseRepository<T> where T : BaseEntity
{
    public GenericRepository(ApplicationDbContext context) : base(context)
    {
    }
}
