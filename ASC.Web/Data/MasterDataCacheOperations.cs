using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Business;
using ASC.Models.Models;
using ASC.DataAccess.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using ASC.Business.Interfaces;
using Newtonsoft.Json;
using ASC.DataAccess;
using Microsoft.Extensions.Configuration;

namespace ASC.Web.Data
{
    public class MasterDataCacheOperations : IMasterDataCacheOperations
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IMasterDataOperations _masterDataOperations;
        private readonly string _masterDataCacheName = "MasterDataCache";

        public MasterDataCacheOperations(IDistributedCache distributedCache, IConfiguration configuration)
        {
            this._distributedCache = distributedCache;
            this._masterDataOperations = new MasterDataOperations(new UnitOfWork(configuration["IdentityAzureTable:IdentityConfiguration:StorageConnectionString"]));
        }

        public async Task CreateMasterDataCacheAsync()
        {
            var masterDataCache = new MasterDataCache
            {
                Keys = (await _masterDataOperations.GetAllMasterKeysAsync()).Where(p => p.IsActive == true).ToList(),
                Values = (await _masterDataOperations.GetAllMasterValuesAsync()).Where(p => p.IsActive == true).ToList()
            };
            await _distributedCache.SetStringAsync(_masterDataCacheName, JsonConvert.SerializeObject(masterDataCache));
        }
        public async Task<MasterDataCache> GetMasterDataCacheAsync()
        {
            return JsonConvert.DeserializeObject<MasterDataCache>(await _distributedCache.GetStringAsync(_masterDataCacheName));
        }
    }
}
