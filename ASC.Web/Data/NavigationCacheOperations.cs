using ASC.Utilities.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using System.IO;
using Newtonsoft.Json;

namespace ASC.Web.Data
{
    public class NavigationCacheOperations : INavigationCacheOperations
    {
        private readonly IDistributedCache _cache;
        private readonly string NavigationCacheName = "NavigationCache";
        public NavigationCacheOperations(IDistributedCache cache)
        {
            this._cache = cache;
        }

        public async Task CreateNavigationMenuCacheAsync()
        {
            await _cache.SetStringAsync(NavigationCacheName, File.ReadAllText("Navigation/Navigation.json"));
        }

        public async Task<NavigationMenu> GetNavigationMenuCacheAsync()
        {
            return JsonConvert.DeserializeObject<NavigationMenu>(await _cache.GetStringAsync(NavigationCacheName));
        }
    }
}
