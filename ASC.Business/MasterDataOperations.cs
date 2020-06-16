using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.DataAccess.Interfaces;
using ASC.Models.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace ASC.Business
{
    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public MasterDataOperations(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public async Task<List<MasterDataKey>> GetAllMasterKeysAsync()
        {
            var masterDataKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();
            return masterDataKeys.ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesAsync()
        {
            var masterDataValues = await _unitOfWork.Repository<MasterDataValue>().FindAllAsync();
            return masterDataValues.ToList();
        }

        public async Task<List<MasterDataValue>> GetAllMasterValuesByKeyAsync(string key)
        {
            var masterDataValues = await _unitOfWork.Repository<MasterDataValue>().FindAllByPartitionAsync(key);
            return masterDataValues.ToList();
        }

        public async Task<List<MasterDataKey>> GetMaserKeyByNameAsync(string name)
        {
            var masterDataKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByPartitionAsync(name);
            return masterDataKeys.ToList();
        }

        public async Task<MasterDataValue> GetMasterValueByNameAsync(string key, string name)
        {
            var masterDataValue = await _unitOfWork.Repository<MasterDataValue>().FindAsync(key, name);
            return masterDataValue;
        }

        public async Task<bool> InsertMasterKeyAsync(MasterDataKey key)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataKey>().AddAsync(key);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> InsertMasterValueAsync(MasterDataValue value)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<MasterDataValue>().AddAsync(value);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterKeyAsync(string orginalPartitionKey, MasterDataKey key)
        {
            using (_unitOfWork)
            {
                var masterKey = await _unitOfWork.Repository<MasterDataKey>().
                FindAsync(orginalPartitionKey, key.RowKey);
                masterKey.IsActive = key.IsActive;
                masterKey.IsDeleted = key.IsDeleted;
                masterKey.Name = key.Name;
                await _unitOfWork.Repository<MasterDataKey>().UpdateAsync(masterKey);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }

        public async Task<bool> UpdateMasterValueAsync(string originalPartitionKey, string originalRowKey, MasterDataValue value)
        {
            using (_unitOfWork)
            {
                var masterValue = await _unitOfWork.Repository<MasterDataValue>().
                FindAsync(originalPartitionKey, originalRowKey);
                masterValue.IsActive = value.IsActive;
                masterValue.IsDeleted = value.IsDeleted;
                masterValue.Name = value.Name;
                await _unitOfWork.Repository<MasterDataValue>().UpdateAsync(masterValue);
                _unitOfWork.CommitTransaction();
                return true;
            }
        }
    }
}
