using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace JanusKeyManagement
{
    public class JanusResiliantDbContext<T> where T: DbContext, new()
    {
        private IDesignTimeDbContextFactory<T> _contextFactory;
        private IJanusKeyEngine _keyEngine;
        private T _activeContext;
        public DbContext ActiveContext { get
            {
                if(_activeContext != null)
                    return _activeContext;

                string[] args = new string[] { _keyEngine.ActiveCredential.Identifier, _keyEngine.ActiveCredential.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                return _activeContext;
            } }
        
        public JanusResiliantDbContext(IDesignTimeDbContextFactory<T> DbContextFactory, IJanusKeyEngine keyEngine)
        {
            _contextFactory = DbContextFactory;
            _keyEngine = keyEngine;
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
                _keyEngine.RotateCredential();
                string[] args = new string[] { _keyEngine.ActiveCredential.Identifier, _keyEngine.ActiveCredential.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                result = GenericRetry(retryMethod);
                _keyEngine.RefreshDeadCredentials();
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
                _keyEngine.RotateCredential();
                string[] args = new string[] { _keyEngine.ActiveCredential.Identifier, _keyEngine.ActiveCredential.Token };
                _activeContext = _contextFactory.CreateDbContext(args);
                result = GenericRetry(retryMethod, parameter);
                _keyEngine.RefreshDeadCredentials();
            }
            return result;
        }

    }

   
}
