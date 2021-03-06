﻿using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.BaseTypes;

namespace ASC.Models.Models
{
    public class MasterDataValue : BaseEntity
    {
        public MasterDataValue()
        {

        }

        public MasterDataValue(string masterPartitionKey)
        {
            this.PartitionKey = masterPartitionKey;
            this.RowKey = Guid.NewGuid().ToString();
        }

        public bool IsActive { get; set; }
        public string Name { get; set; }
    }
}
