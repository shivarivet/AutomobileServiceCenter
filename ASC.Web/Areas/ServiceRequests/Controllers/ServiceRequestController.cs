using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.Web.Controllers;
using ASC.Web.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ASC.Models.BaseTypes;
using ASC.Web.Areas.ServiceRequests.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ASC.Models.Models;
using ASC.Utilities;
using Microsoft.AspNetCore.Identity;
using ASC.Web.Models;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using ASC.Web.ServiceHub;
using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOperations;
        private readonly IMasterDataCacheOperations _masterDataCacheOperations;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceRequestMessageOperations _serviceRequestMessageOperations;
        private readonly IOptions<ApplicationSettings> _options;
        private readonly IHubContext<ServiceMessagesHub> _serviceMessagesHubContext;

        public ServiceRequestController(IServiceRequestOperations serviceRequestOperations, IMasterDataCacheOperations masterDataCacheOperations,
            IMapper mapper, UserManager<ApplicationUser> userManager, IServiceRequestMessageOperations serviceRequestMessageOperations,
            IOptions<ApplicationSettings> options, IHubContext<ServiceMessagesHub> serviceMessagesHubContext)
        {
            this._serviceRequestOperations = serviceRequestOperations;
            this._masterDataCacheOperations = masterDataCacheOperations;
            this._mapper = mapper;
            this._userManager = userManager;
            this._serviceRequestMessageOperations = serviceRequestMessageOperations;
            this._options = options;
            this._serviceMessagesHubContext = serviceMessagesHubContext;
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterData = await _masterDataCacheOperations.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
            return View(new NewServiceRequestViewModel());
        }

        public async Task<IActionResult> ServiceRequest(NewServiceRequestViewModel serviceRequestViewModel)
        {
            if (!ModelState.IsValid)
            {
                var masterData = await _masterDataCacheOperations.GetMasterDataCacheAsync();
                ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleType.ToString()).ToList();
                ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey == MasterKeys.VehicleName.ToString()).ToList();
                return View(serviceRequestViewModel);
            }
            ServiceRequest serviceRequest = _mapper.Map<NewServiceRequestViewModel, ServiceRequest>(serviceRequestViewModel);
            serviceRequest.PartitionKey = this.HttpContext.User.GetCurrentUser()?.Email;
            serviceRequest.RowKey = Guid.NewGuid().ToString();
            serviceRequest.Status = Status.New.ToString();

            await _serviceRequestOperations.CreateServiceRequestAsync(serviceRequest);

            return RedirectToAction("Dashboard", "Dashboard", new { Area = "ServiceRequests" });
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestDetails(string id)
        {
            var serviceRequestDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(id);

            // Access Check
            if (HttpContext.User.IsInRole(Roles.Engineer.ToString())
                && serviceRequestDetails.ServiceEngineer != HttpContext.User.GetCurrentUser().Email)
            {
                throw new UnauthorizedAccessException();
            }
            if (HttpContext.User.IsInRole(Roles.User.ToString())
            && serviceRequestDetails.PartitionKey != HttpContext.User.GetCurrentUser().Email)
            {
                throw new UnauthorizedAccessException();
            }

            var serviceRequestAuditDetails = await _serviceRequestOperations.GetServiceRequestAuditByPartitionKey(serviceRequestDetails.PartitionKey + "-" + id);

            // Select List Data
            var masterData = await _masterDataCacheOperations.GetMasterDataCacheAsync();
            ViewBag.VehicleTypes = masterData.Values.Where(p => p.PartitionKey ==
            MasterKeys.VehicleType.ToString()).ToList();
            ViewBag.VehicleNames = masterData.Values.Where(p => p.PartitionKey ==
            MasterKeys.VehicleName.ToString()).ToList();
            ViewBag.Status = Enum.GetValues(typeof(Status)).Cast<Status>().Select(v => v.ToString()).ToList();
            ViewBag.ServiceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());
            return View(new ServiceRequestDetailViewModel
            {
                UpdateServiceRequestViewModel = _mapper.Map<ServiceRequest, UpdateServiceRequestViewModel>(serviceRequestDetails),
                ServiceRequestAudit = serviceRequestAuditDetails.OrderByDescending(p => p.Timestamp).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServiceRequestDetails(ServiceRequestDetailViewModel serviceRequestDetail)
        {
            var originalServiceRequest = await _serviceRequestOperations.GetServiceRequestByRowKey(serviceRequestDetail.UpdateServiceRequestViewModel.RowKey);
            originalServiceRequest.RequestedServices = serviceRequestDetail.UpdateServiceRequestViewModel.RequestedServices;

            // Update Status only if user role is either Admin or Engineer
            // Or Customer can update the status if it is only in Pending Customer Approval.
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()) ||
            HttpContext.User.IsInRole(Roles.Engineer.ToString()) ||
            (HttpContext.User.IsInRole(Roles.User.ToString()) && originalServiceRequest.Status == Status.PendingCustomerApproval.ToString()))
            {
                originalServiceRequest.Status = serviceRequestDetail.UpdateServiceRequestViewModel.Status;
            }

            // Update Service Engineer field only if user role is Admin
            if (HttpContext.User.IsInRole(Roles.Admin.ToString()))
            {
                originalServiceRequest.ServiceEngineer = serviceRequestDetail.UpdateServiceRequestViewModel.ServiceEngineer;
            }
            await _serviceRequestOperations.UpdateServiceRequestAsync(originalServiceRequest);
            return RedirectToAction("ServiceRequestDetails", "ServiceRequest", new { Area = "ServiceRequests", Id = serviceRequestDetail.UpdateServiceRequestViewModel.RowKey });
        }

        public async Task<IActionResult> CheckDenialService(DateTime requestedDate)
        {
            var serviceRequests = await _serviceRequestOperations.GetServiceRequestsByRequestedDateAndStatus(DateTime.UtcNow.AddDays(-90), new List<string>() { Status.Denied.ToString() },
            HttpContext.User.GetCurrentUser().Email);

            if (serviceRequests.Any())
                return Json(data: $"There is a denied service request for you in last 90 days. Please contact ASC Admin.");

            return Json(data: true);
        }

        [HttpGet]
        public async Task<IActionResult> ServiceRequestMessages(string serviceRequestId)
        {
            return Json((await _serviceRequestMessageOperations.GetServiceRequestMessages(serviceRequestId)).OrderByDescending(p => p.MessageDate));
        }

        [HttpPost]
        public async Task<IActionResult> CreateServiceRequestMessage(ServiceRequestMessage message)
        {
            // Message and Service Request Id (Service request Id is the partition key for a message)
            if (string.IsNullOrWhiteSpace(message.Message) || string.IsNullOrWhiteSpace(message.PartitionKey))
                return Json(false);

            // Get Service Request details
            var serviceRequesrDetails = await _serviceRequestOperations.GetServiceRequestByRowKey(message.PartitionKey);
            // Populate message details
            message.FromEmail = HttpContext.User.GetCurrentUser().Email;
            message.FromDisplayName = HttpContext.User.GetCurrentUser().Name;
            message.MessageDate = DateTime.UtcNow;
            message.RowKey = Guid.NewGuid().ToString();

            // Get Customer and Service Engineer names
            var customerName = (await _userManager.FindByEmailAsync(serviceRequesrDetails.PartitionKey)).UserName;
            var serviceEngineerName = string.Empty;
            if (!string.IsNullOrWhiteSpace(serviceRequesrDetails.ServiceEngineer))
            {
                serviceEngineerName = (await _userManager.FindByEmailAsync(serviceRequesrDetails.ServiceEngineer)).UserName;
            }
            var adminName = (await _userManager.FindByEmailAsync(_options.Value.AdminEmail)).UserName;

            // Save the message to Azure Storage
            await _serviceRequestMessageOperations.CreateServiceRequestMesageAsync(message);
            var users = new List<string> { customerName, adminName };
            if (!string.IsNullOrWhiteSpace(serviceEngineerName))
            {
                users.Add(serviceEngineerName);
            }

            //// Broadcast the message to all clients asscoaited with Service Request
            //await _serviceMessagesHubContext
            //.Clients
            //.Users(users)
            //.SendAsync("SendMessage", message);

            // Return true
            return Json(message);
        }
    }
}