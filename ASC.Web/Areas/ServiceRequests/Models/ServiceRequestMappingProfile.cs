using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ASC.Models.Models;

namespace ASC.Web.Areas.ServiceRequests.Models
{
    public class ServiceRequestMappingProfile : Profile
    {
        public ServiceRequestMappingProfile()
        {
            CreateMap<NewServiceRequestViewModel, ServiceRequest>();
            CreateMap<ServiceRequest, NewServiceRequestViewModel>();

            CreateMap<ServiceRequest, UpdateServiceRequestViewModel>();
            CreateMap<UpdateServiceRequestViewModel, ServiceRequest>();
        }
    }
}
