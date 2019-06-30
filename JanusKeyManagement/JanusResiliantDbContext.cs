using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace JanusKeyManagement
{
    public class JanusResiliantDbContext<T> : IJanusResiliantDbContext<T> where T: DbContext
    {
        private IDesignTimeDbContextFactory<T> _contextFactory;
        private IJanusKeySet _keySet;
        private T _activeContext;
        public DbContext ActiveContext { get
            {
                if(_activeContext != null)
                    return _activeContext;

                string[] args = new string[] { _keySet.Active.Identifier, _keySet.Active.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                return _activeContext;
            } }
        
        public JanusResiliantDbContext(IDesignTimeDbContextFactory<T> DbContextFactory, IJanusKeySet keyEngine)
        {
            _contextFactory = DbContextFactory;
            _keySet = keyEngine;
        }

        public EntityEntry Add(object entity)
        {
            return GenericRetry(AddInternal, entity);
        }

        private EntityEntry AddInternal(object entity)
        {
            return ActiveContext.Add(entity);
        }

        public EntityEntry Update(object entity)
        {
            return GenericRetry(UpdateInternal, entity);
        }

        private EntityEntry UpdateInternal(object entity)
        {
            return ActiveContext.Update(entity);
        }

        public EntityEntry Remove(object entity)
        {
            return GenericRetry(RemoveInternal, entity);
        }

        private EntityEntry RemoveInternal(object entity)
        {
            return ActiveContext.Remove(entity);
        }

        public int SaveChanges(object entity)
        {
            return GenericRetry(SaveChangesInternal);
        }

        private int SaveChangesInternal()
        {
            return ActiveContext.SaveChanges();
        }

        private G GenericRetry<G>(Func<G> retryMethod)
        {
            G result = default(G);
            try
            {
                result = retryMethod();
            }
            catch (DbUpdateException exp)
            {
                _keySet.Rotate();
                string[] args = new string[] { _keySet.Active.Identifier, _keySet.Active.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                result = GenericRetry(retryMethod);
                _keySet.Refresh();
            }
            return result;
        }

        private G GenericRetry<L,G>(Func<L, G> retryMethod, L parameter)
        {
            G result = default(G);
            try
            {
                result = retryMethod(parameter);
            }
            catch(DbUpdateException exp)
            {
                _keySet.Rotate();
                string[] args = new string[] { _keySet.Active.Identifier, _keySet.Active.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                result = GenericRetry(retryMethod, parameter);
                _keySet.Refresh();
            }
            return result;
        }

    }

   
}
