using KeyVaultExample.Repository;
using KeyVaultExample.UOW.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyVaultExample.UOW
{
    public class GenericUoW : IGenericUoW
    {
        private readonly Dictionary<string, IDesignTimeDbContextFactory<DbContext>> _exampleContextFactories = null;
        private Dictionary<string, DbContext> _contexts = new Dictionary<string, DbContext>(); 
        public Dictionary<Type, object> repositories = new Dictionary<Type, object>();

        public GenericUoW(IDesignTimeDbContextFactory<DbContext>[] exampleContextFactories)
        {
            _exampleContextFactories = new Dictionary<string, IDesignTimeDbContextFactory<DbContext>>();
            foreach(var context in exampleContextFactories)
            {
                var realContext = context.CreateDbContext(new string[0]);
                var name = realContext.GetType().Name;
                _exampleContextFactories.Add(name, context);
                _contexts.Add(name, realContext);
            }
        }

        public IResiliantRepository<T> Repository<T>() where T : class
        {
            if (repositories.Keys.Contains(typeof(T)) == true)
            {
                return repositories[typeof(T)] as IResiliantRepository<T>;
            }

            IResiliantRepository<T> repo = new ResiliantRepository<T>(_contexts["ExampleContext"]);
            repositories.Add(typeof(T), repo);
            return repo;
        }

    }
}