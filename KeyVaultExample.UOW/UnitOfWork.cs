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
        //Todo: Kill All Repositories with the reference
        //Todo: Pop the context in the object out of the collection
        //Todo: Create a new Context from the Context Factory, context factory should use a string with a replace function
        //Todo: Replace in SQL Connection String for the default Janus settings
        //Todo: Add delegate in Generic Repositories to capture "Gets" that fail
        
        public void RepositoryFailover
    }
}