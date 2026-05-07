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
            CreateMap<ProjectInvoiceDto, ProjectInvoiceReq>().ReverseMap();
            CreateMap<ProjectInvoiceDto, ProjectInvoiceRes>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractReq>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractRes>().ReverseMap();
            CreateMap<TestCapabilityDto, TestCapabilityReq>().ReverseMap();
            CreateMap<TestCapabilityDto, TestCapabilityRes>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementtRes>().ReverseMap();
            CreateMap<TestorProductDto, TestorProductReq>().ReverseMap();
            CreateMap<TestorProductDto, TestorProductRes>().ReverseMap();

            CreateMap<ProjectMonthDto, ProjectMonthReq>().ReverseMap();
            CreateMap<ProjectMonthDto, ProjectMonthRes>().ReverseMap();
            CreateMap<MonthDto, MonthRes>().ReverseMap();
            CreateMap<ProjectProfileGraphDto, ProjectProfileGraphRes>().ReverseMap();
            CreateMap<ProjectProfileCumulativeGraphDto, ProjectProfileCumulativeGraphRes>().ReverseMap();
        }
    }
}
