using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ASC.Web.Controllers;
using ASC.Business.Interfaces;
using ASC.Web.Data;
using ASC.Models.BaseTypes;
using ASC.Models.Models;
using ASC.Utilities;
using ASC.Web.Areas.ServiceRequests.Models;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class DashboardController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterDataCacheOperations;
        public DashboardController(IServiceRequestOperations serviceRequestOperations, IMasterDataCacheOperations masterDataCacheOperations)
        {
            this._serviceRequestOperations = serviceRequestOperations;
            this._masterDataCacheOperations = masterDataCacheOperations;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var status = new List<string>() {
                Status.New.ToString(),
                Status.Initiated.ToString(),
                Status.InProgress.ToString(),
                Status.RequestForInformation.ToString()
            };

            List<ServiceRequest> serviceRequests = new List<ServiceRequest>();
            List<ServiceRequest> auditServiceRequests = new List<ServiceRequest>();
            Dictionary<string, int> activeServiceRequests = new Dictionary<string, int>();

            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                serviceRequests = await this._serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    DateTime.UtcNow.AddDays(-7), status);

                auditServiceRequests = await this._serviceRequestOperations.GetServiceRequestsFormAudit();

                var serviceEngineerServiceRequests = await _serviceRequestOperations.GetActiveServiceRequests(new List<string>{ Status.InProgress.ToString(),
                                                                                                                                Status.Initiated.ToString(),
                                                                                                                                });
                if(serviceEngineerServiceRequests.Any())
                {
                    activeServiceRequests = serviceEngineerServiceRequests.GroupBy(s => s.ServiceEngineer).ToDictionary(p => p.Key, p => p.Count());
                }
            }
            else if (HttpContext.User.IsInRole(Roles.Engineer.ToString()))
            {
                serviceRequests = await this._serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddDays(-7), status, serviceEngineerEmail: HttpContext.User.GetCurrentUser().Email);

                auditServiceRequests = await this._serviceRequestOperations.GetServiceRequestsFormAudit(serviceEngineerEmail: HttpContext.User.GetCurrentUser().Email);
            }
            else
            {
                serviceRequests = await this._serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(
                    DateTime.UtcNow.AddDays(-7), status, customerEmail: HttpContext.User.GetCurrentUser().Email);
            }

            return View(new DashboardViewModel
            {
                ServiceRequests = serviceRequests.OrderByDescending(p => p.RequestedDate).ToList(),
                AuditServiceRequests = auditServiceRequests.OrderByDescending(a => a.Timestamp).ToList(),
                ActiveServiceRequests = activeServiceRequests
            });
        }
    }
}