using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PactViewModelMapper : Profile
    {
        public PactViewModelMapper() 
        {
            CreateMap<PactProjectViewModel, ProjectDto>().ReverseMap();
            CreateMap<ProjectJobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<JobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeViewModel>().ReverseMap();
            CreateMap<ProjectInvoiceItem, ProjectInvoiceDto>()
                .ForMember(dest => dest.InvoiceCounter, opt => opt.MapFrom(src => src.Counter))
                .ReverseMap()
                .ForMember(dest => dest.Counter, opt => opt.MapFrom(src => src.InvoiceCounter));
            CreateMap<ProjectSubContractItem, ProjectSubContractDto>()
                .ForMember(dest => dest.SubContCounter, opt => opt.MapFrom(src => src.Counter))
                .ReverseMap()
                .ForMember(dest => dest.Counter, opt => opt.MapFrom(src => src.SubContCounter));

            CreateMap<TestCapabilityItem, TestCapabilityDto>().ReverseMap();
            CreateMap<ConstituentTestItem, TestCapabilityDto>().ReverseMap();
            CreateMap<PortfolioTimeCodeViewModel, TimeCodeValidDto>().ReverseMap();

            CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap();
            CreateMap<TestPurchaseRequirementItem, TestRequirementDto>().ReverseMap();
        }
    }
}
