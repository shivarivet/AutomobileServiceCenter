﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterDataKeyViewModel
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool IsActive { get; set; }
        
        [Required]
        public string Name { get; set; }
    }
}
