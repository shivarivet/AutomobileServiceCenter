﻿using ASC.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class ServiceRequestDetailViewModel
    {
        public UpdateServiceRequestViewModel UpdateServiceRequestViewModel { get; set; }
        public List<ServiceRequest> ServiceRequestAudit { get; set; }
    }
}
