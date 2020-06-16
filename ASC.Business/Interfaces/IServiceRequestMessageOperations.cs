using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASC.Models.Models;

namespace ASC.Business.Interfaces
{
    public interface IServiceRequestMessageOperations
    {
        Task CreateServiceRequestMesageAsync(ServiceRequestMessage serviceRequestMessage);
        Task<List<ServiceRequestMessage>> GetServiceRequestMessages(string serviceRequestId);
    }
}
