using ASC.Utilities.CustomValidationAttributes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class NewServiceRequestViewModel
    {
        [Required]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; }
        [Required]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; }
        [Required]
        [Display(Name = "Requested Services")]
        public string RequestedServices { get; set; }
        
        [Required]
        [Display(Name = "Requested Date")]
        [FutureDate(90)]
        [Remote(action: "CheckDenialService", controller: "ServiceRequest", areaName: "ServiceRequests")]
        public DateTime? RequestedDate { get; set; }
    }
}
