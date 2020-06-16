using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.BaseTypes;

namespace ASC.Models.Models
{
    public class UserActivity : BaseEntity
    {
        public UserActivity()
        {

        }

        public UserActivity(string email)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = email;
        }

        public string Action { get; set; }
    }
}
