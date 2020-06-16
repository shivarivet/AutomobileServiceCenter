using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.Models;
using ASC.Business.Interfaces;
using System.Threading.Tasks;
using ASC.DataAccess.Interfaces;
using System.Linq;

namespace ASC.Business
{
    public class ServiceRequestMessageOperations : IServiceRequestMessageOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceRequestMessageOperations(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateServiceRequestMesageAsync(ServiceRequestMessage serviceRequestMessage)
        {
            using (_unitOfWork)
            {
                await _unitOfWork.Repository<ServiceRequestMessage>().AddAsync(serviceRequestMessage);
                _unitOfWork.CommitTransaction();
            }
        }

        public async Task<List<ServiceRequestMessage>> GetServiceRequestMessages(string serviceRequestId)
        {
            var serviceRequestMessages = await _unitOfWork.Repository<ServiceRequestMessage>().FindAllByPartitionAsync(serviceRequestId);
            return serviceRequestMessages.ToList();
        }
    }
}
