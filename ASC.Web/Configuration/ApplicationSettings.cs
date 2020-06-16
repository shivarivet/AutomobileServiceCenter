using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Configuration
{
    public class ApplicationSettings
    {
        //Admin settings..
        public string ApplicationTitle { get; set; }
        public string Roles { get; set; }
        public string AdminName { get; set; }
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }

        //ServiceEngineer settings..
        public string EngineerEmail { get; set; }
        public string EngineerName { get; set; }
        public string EngineerPassword { get; set; }

        //Email settings..
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string SMTPAccount { get; set; }
        public string SMTPPassword { get; set; }
    }
}
