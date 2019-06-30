using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace JanusKeyManagement
{
    public interface IJanusResiliantDbContext<T> where T: DbContext
    {
        DbContext ActiveContext { get; }

        EntityEntry Add(object entity);
        EntityEntry Remove(object entity);
        int SaveChanges(object entity);
        EntityEntry Update(object entity);
    }
}