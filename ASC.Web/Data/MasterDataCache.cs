using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Models.Models;

namespace ASC.Web.Data
{
    public class MasterDataCache
    {
        public List<MasterDataKey> Keys { get; set; }
        public List<MasterDataValue> Values { get; set; }
    }
}
