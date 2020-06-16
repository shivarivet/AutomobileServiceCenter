using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ASC.DataAccess.Interfaces
{
    public interface IRepository<T>  where T : TableEntity
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T Entity);
        Task<T> FindAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> FindAllByPartitionAsync(string partitionKey);
        Task<IEnumerable<T>> FindAllAsync();
        Task CreateTableAsync();
        Task<IEnumerable<T>> FindAllByQueryAsync(string query);
        Task<IEnumerable<T>> FindAllInAuditByQuery(string query);
    }
}
