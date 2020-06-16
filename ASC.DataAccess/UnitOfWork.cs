﻿using ASC.DataAccess.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASC.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed;
        private bool complete;
        private IDictionary<string, object> _repositories;

        public Queue<Task<Action>> RollbackActions { get; set; }
        public string ConnectionString { get; set; }

        public UnitOfWork(string connectionString)
        {
            this.ConnectionString = connectionString;
            RollbackActions = new Queue<Task<Action>>();
        }

        public void CommitTransaction()
        {
            this.complete = true;
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void RollbackTransaction()
        {
            while(RollbackActions.Count > 0)
            {
                var undoAction = RollbackActions.Dequeue();
                undoAction.Result();
            }
        }

        public IRepository<T> Repository<T>() where T : TableEntity
        {
            if (_repositories == null)
                _repositories = new Dictionary<string, object>();
            var type = typeof(T).Name;
            if (_repositories.ContainsKey(type)) return (IRepository<T>)_repositories[type];

            var repositoryType = typeof(Repository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), this);
            _repositories.Add(type, repositoryInstance);

            return (IRepository<T>)_repositories[type];
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (!complete) RollbackTransaction();
                }
                finally
                {
                    RollbackActions.Clear();
                }
            }
            complete = false;
        }
    }
}
