using System;
using System.Collections.Generic;
using System.Text;

namespace KeyVaultExample.UOW.GenericRepository
{
    public interface IResiliantRepository<T> where T : class
    {
        IEnumerable<T> GetAll(Func<T, bool> predicate = null);
        T Get(Func<T, bool> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);

        void SaveChanges();
    }
}
