using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASC.Models.Models;

namespace ASC.Business.Interfaces
{
    public interface IServiceRequestOperations
    {
        Task CreateServiceRequestAsync(ServiceRequest serviceRequest);
        Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest serviceRequest);
        Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status);
        Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(DateTime? requestedDateTime, List<string> status = null,
            string customerEmail = "", string serviceEngineerEmail = "");
        Task<List<ServiceRequest>> GetServiceRequestsFormAudit(string serviceEngineerEmail = "");
        Task<List<ServiceRequest>> GetActiveServiceRequests(List<string> status);
        Task<ServiceRequest> GetServiceRequestByRowKey(string id);
        Task<List<ServiceRequest>> GetServiceRequestAuditByPartitionKey(string id);
    }
}
