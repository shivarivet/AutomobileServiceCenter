using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Utilities.Navigation;

namespace ASC.Web.Data
{
    public interface INavigationCacheOperations
    {
        Task<NavigationMenu> GetNavigationMenuCacheAsync();
        Task CreateNavigationMenuCacheAsync();
    }
}
