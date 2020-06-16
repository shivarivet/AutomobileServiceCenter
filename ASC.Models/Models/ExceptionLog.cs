using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.BaseTypes;

namespace ASC.Models.Models
{
    public class ExceptionLog : BaseEntity
    {
        public ExceptionLog()
        {

        }

        public ExceptionLog(string key)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = DateTime.UtcNow.ToString();
        }

        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
