using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ASC.DataAccess.Interfaces;
using System.Threading.Tasks;
using ASC.Models.BaseTypes;
using ASC.Utilities;

namespace ASC.DataAccess
{
    public class Repository<T> : IRepository<T> where T : TableEntity, new()
    {
        private readonly CloudStorageAccount storageAccount;
        private readonly CloudTableClient tableClient;
        private CloudTable storageTable;

        public IUnitOfWork Scope { get; set; }

        public Repository(IUnitOfWork scope)
        {
            storageAccount = CloudStorageAccount.Parse(scope.ConnectionString);
            tableClient = storageAccount.CreateCloudTableClient();
            storageTable = tableClient.GetTableReference(typeof(T).Name);
            this.Scope = scope;
        }
        public async Task<T> AddAsync(T entity)
        {
            var entityToInsert = entity as BaseEntity;
            entityToInsert.CreatedDate = DateTime.UtcNow;
            entityToInsert.UpdatedDate = DateTime.UtcNow;
            TableOperation insertOperation = TableOperation.Insert(entity);
            var result = await ExecuteAsync(insertOperation);
            return result.Result as T;
        }

        public async Task CreateTableAsync()
        {
            CloudTable table = tableClient.GetTableReference(typeof(T).Name);
            await table.CreateIfNotExistsAsync();

            if (typeof(IAuditTracker).IsAssignableFrom(typeof(T)))
            {
                CloudTable auditTable = tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.CreateIfNotExistsAsync();
            }
        }

        public async Task DeleteAsync(T entity)
        {
            var entityToDelete = entity as BaseEntity;
            entityToDelete.UpdatedDate = DateTime.UtcNow;
            entityToDelete.IsDeleted = true;
            TableOperation deleteOperation = TableOperation.Replace(entityToDelete);
            await ExecuteAsync(deleteOperation);
        }

        public async Task<IEnumerable<T>> FindAllAsync()
        {
            TableQuery<T> query = new TableQuery<T>();
            TableContinuationToken tableContinuationToken = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
            return result.Results as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> FindAllByPartitionAsync(string partitionKey)
        {
            TableQuery<T> query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
            TableContinuationToken tableContinuationToken = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
            return result.Results as IEnumerable<T>;
        }

        public async Task<T> FindAsync(string partitionKey, string rowKey)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await storageTable.ExecuteAsync(retrieveOperation);
            return result.Result as T;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var entityToUpdate = entity as BaseEntity;
            entityToUpdate.UpdatedDate = DateTime.UtcNow;
            TableOperation updateOperation = TableOperation.Replace(entity);
            var result = await ExecuteAsync(updateOperation);
            return result.Result as T;
        }

        public async Task<IEnumerable<T>> FindAllByQueryAsync(string query)
        {
            TableQuery<T> tableQuery = new TableQuery<T>().Where(query);
            TableContinuationToken tableContinuationToken = null;
            var result = await storageTable.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);
            return result.Results as IEnumerable<T>;
        }

        public async Task<IEnumerable<T>> FindAllInAuditByQuery(string query)
        {
            var auditTable = tableClient.GetTableReference($"{typeof(T).Name}Audit");
            TableQuery<T> tableQuery = new TableQuery<T>().Take(20).Where(query);
            TableContinuationToken tableContinuationToken = null;
            var result = await auditTable.ExecuteQuerySegmentedAsync(tableQuery, tableContinuationToken);
            return result.Results as IEnumerable<T>;
        }


        private async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            var rollbackAction = CreateRollbackAction(operation);
            var result = await storageTable.ExecuteAsync(operation);
            Scope.RollbackActions.Enqueue(rollbackAction);

            // Audit Implementation
            if (operation.Entity is IAuditTracker)
            {
                // Make sure we do not use same RowKey and PartitionKey
                var auditEntity = ObjectExtensions.CopyObject<T>(operation.Entity);
                auditEntity.PartitionKey = $"{auditEntity.PartitionKey}-{auditEntity.RowKey}";
                auditEntity.RowKey = $"{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff")}";
                var auditOperation = TableOperation.Insert(auditEntity);
                var auditRollbackAction = CreateRollbackAction(auditOperation, true);
                var auditTable = tableClient.GetTableReference($"{typeof(T).Name}Audit");
                await auditTable.ExecuteAsync(auditOperation);
                Scope.RollbackActions.Enqueue(auditRollbackAction);
            }

            return result;
        }

        private async Task<Action> CreateRollbackAction(TableOperation tableOperation, bool IsAuditOperation = false)
        {
            if (tableOperation.OperationType == TableOperationType.Retrieve)
                return null;

            var tableEntity = tableOperation.Entity;
            var cloudTable = !IsAuditOperation ? storageTable : tableClient.GetTableReference($"{typeof(T).Name}Audit"); ;
            switch (tableOperation.OperationType)
            {
                case TableOperationType.Insert:
                    return async () => await UndoInsertOperation(cloudTable, tableEntity);
                case TableOperationType.Delete:
                    return async () => await UndoDeleteOperation(cloudTable, tableEntity);
                case TableOperationType.Replace:
                    var retrieveResult = await cloudTable.ExecuteAsync(TableOperation.Retrieve(tableEntity.PartitionKey, tableEntity.RowKey));
                    return async () => await UndoReplaceOperation(cloudTable, retrieveResult.Result as DynamicTableEntity, tableEntity);
                default:
                    throw new InvalidOperationException("The storage operation cannot be identified.");
            }
        }

        private async Task UndoInsertOperation(CloudTable table, ITableEntity tableEntity)
        {
            TableOperation operation = TableOperation.Delete(tableEntity);
            await table.ExecuteAsync(operation);
        }

        private async Task UndoDeleteOperation(CloudTable table, ITableEntity tableEntity)
        {
            var entityToRestore = tableEntity as BaseEntity;
            entityToRestore.IsDeleted = false;

            TableOperation insertOperation = TableOperation.Replace(entityToRestore);
            await table.ExecuteAsync(insertOperation);
        }

        private async Task UndoReplaceOperation(CloudTable table, ITableEntity originalEntity, ITableEntity newEntity)
        {
            if (originalEntity != null)
            {
                if (!String.IsNullOrEmpty(newEntity.ETag)) originalEntity.ETag = newEntity.ETag;
                var replaceOperation = TableOperation.Replace(originalEntity);
                await table.ExecuteAsync(replaceOperation);
            }
        }
    }
}