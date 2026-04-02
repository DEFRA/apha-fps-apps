using Apha.FPSApps.Application.Dtos.PACT;
using Apha.Common.Contracts.PACT;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PactApiDtoMapper : Profile
    {
        public PactApiDtoMapper()
        {
            // PACT
            CreateMap<JobCodeDto, JobCodeReq>().ReverseMap();
            CreateMap<JobCodeDto, JobCodeRes>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidReq>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidRes>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroupRes>().ReverseMap();
        }
    }
}
