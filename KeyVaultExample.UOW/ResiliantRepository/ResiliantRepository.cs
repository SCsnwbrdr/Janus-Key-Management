﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyVaultExample.UOW.GenericRepository
{
    class ResiliantRepository<T> : IResiliantRepository<T> where T : class
    {
        private readonly DbContext entities = null;
        private DbSet<T> _objectSet;

        public ResiliantRepository(DbContext _entities)
        {
            entities = _entities;
            _objectSet = entities.Set<T>();
        }
        /// <summary>
        /// Get all the list
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<T> GetAll(Func<T, bool> predicate = null)
        {
            if (predicate != null)
            {
                return _objectSet.Where(predicate);
            }

            return _objectSet.AsEnumerable();
        }
        /// <summary>
        /// Get a certain element of the list
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public T Get(Func<T, bool> predicate)
        {
            return _objectSet.First(predicate);
        }
        /// <summary>
        /// Add a new element to the list
        /// </summary>
        /// <param name="entity"></param>
        public void Add(T entity)
        {
            _objectSet.Add(entity);
        }
        /// <summary>
        /// Update a certain element
        /// </summary>
        /// <param name="entity"></param>
        public void Update(T entity)
        {
            _objectSet.Update(entity);
        }
        /// <summary>
        /// Delete an element from the list
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(T entity)
        {
            _objectSet.Remove(entity);
        }

        public void SaveChanges()
        {
            
        }
    }
}