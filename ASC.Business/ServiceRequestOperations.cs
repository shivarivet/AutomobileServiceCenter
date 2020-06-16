using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASC.Business.Interfaces;
using ASC.Models.Models;
using ASC.DataAccess.Interfaces;
using ASC.Models.Queries;
using System.Linq;

namespace ASC.Business
{
    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceRequestOperations(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public async Task CreateServiceRequestAsync(ServiceRequest serviceRequest)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().AddAsync(serviceRequest);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsByRequestedDateAndStatus(DateTime? requestedDateTime, List<string> status = null, 
            string customerEmail = "", string serviceEngineerEmail = "")
        {
            string query = Queries.GetDashboardQuery(requestedDateTime, status, customerEmail, serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQueryAsync(query);
            return serviceRequests.ToList();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsFormAudit(string serviceEngineerEmail = "")
        {
            var query = Queries.GetDashboardAuditQuery(serviceEngineerEmail);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllInAuditByQuery(query);
            return serviceRequests.ToList();
        }

        public async Task<List<ServiceRequest>> GetActiveServiceRequests(List<string> status)
        {
            var query = Queries.GetDashboardServiceEngineersQuery(status);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQueryAsync(query);
            return serviceRequests.ToList();
        }

        public async Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest request)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(request);
                _unitOfWork.CommitTransaction();
                return request;
            }
        }
        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string rowKey, string partitionKey, string status)
        {
            using (_unitOfWork)
            {
                var serviceRequest = await _unitOfWork.Repository<ServiceRequest>().FindAsync(partitionKey, rowKey);

                if (serviceRequest == null)
                    throw new NullReferenceException();

                serviceRequest.Status = status;
                await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(serviceRequest);
                _unitOfWork.CommitTransaction();
                return serviceRequest;
            }
        }

        public async Task<ServiceRequest> GetServiceRequestByRowKey(string id)
        {
            var query = Queries.GetServiceRequestDetailsQuery(id);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllByQueryAsync(query);
            return serviceRequests.FirstOrDefault();
        }

        public async Task<List<ServiceRequest>> GetServiceRequestAuditByPartitionKey(string id)
        {
            var query = Queries.GetServiceRequestAuditDetailsQuery(id);
            var serviceRequests = await _unitOfWork.Repository<ServiceRequest>().FindAllInAuditByQuery(query);
            return serviceRequests.ToList();
        }
    }
}
