using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;using Apha.FPSApps.Web.Areas.PACT.Models;
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
            CreateMap<PortfolioJobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeViewModel>().ReverseMap();
            CreateMap<TimeCodeValidDto, ValidTimeCodeViewModel>()
                .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.ParentProject))
                .ForMember(dest => dest.OriginalWorkGroup, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ParentProject, opt => opt.MapFrom(src => src.ParentProject));
            CreateMap<ProjectInvoiceItem, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectSubContractItem, ProjectSubContractDto>()
                .ForMember(dest => dest.SubContCounter, opt => opt.MapFrom(src => src.Counter))
                .ReverseMap()
                .ForMember(dest => dest.Counter, opt => opt.MapFrom(src => src.SubContCounter));

            // Mapping for standalone SubContract page
            CreateMap<SubContractItem, ProjectSubContractDto>()
                .ForMember(dest => dest.DailyRate, opt => opt.Ignore())
                .ForMember(dest => dest.AnimalDays, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Counter, opt => opt.MapFrom(src => src.SubContCounter));

            CreateMap<TestCapabilityItem, TestCapabilityDto>().ReverseMap();
            CreateMap<ConstituentTestItem, TestCapabilityDto>().ReverseMap();

            // Mapping for WorkGroup-focused Test Capability view
            CreateMap<WorkGroupTestCapabilityItem, TestCapabilityDto>().ReverseMap();

            CreateMap<PortfolioTimeCodeViewModel, TimeCodeValidDto>().ReverseMap();

            CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap();
            CreateMap<TestPurchaseRequirementItem, TestRequirementDto>().ReverseMap();

            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<ProgramProjectItem, ProjectDto>().ReverseMap();
            CreateMap<TestorProductDto, TestOrProductViewModel>().ReverseMap();

            CreateMap<ProjectMonthItem, ProjectMonthDto>().ReverseMap();
            CreateMap<WorkGroupStaffDto, WorkGroupPeopleItem>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroup>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPerson>().ReverseMap();
            CreateMap<MonthlyOutputLogDto, MonthlyOutputLogItem>().ReverseMap();
            CreateMap<CalenderMonthDto, CalenderMonth>().ReverseMap();
            CreateMap<WorkGroupTimeCodeDto, WorkGroupTimeCodeItem>().ReverseMap();
            CreateMap<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeItem>().ReverseMap();

            // Summarised Workgroup Time mappings
            CreateMap<SummarisedWgTimeDto, SummarisedWgTimePivotRow>()
                .ForMember(dest => dest.M1, opt => opt.MapFrom(src => src.April))
                .ForMember(dest => dest.M2, opt => opt.MapFrom(src => src.May))
                .ForMember(dest => dest.M3, opt => opt.MapFrom(src => src.June))
                .ForMember(dest => dest.M4, opt => opt.MapFrom(src => src.July))
                .ForMember(dest => dest.M5, opt => opt.MapFrom(src => src.August))
                .ForMember(dest => dest.M6, opt => opt.MapFrom(src => src.September))
                .ForMember(dest => dest.M7, opt => opt.MapFrom(src => src.October))
                .ForMember(dest => dest.M8, opt => opt.MapFrom(src => src.November))
                .ForMember(dest => dest.M9, opt => opt.MapFrom(src => src.December))
                .ForMember(dest => dest.M10, opt => opt.MapFrom(src => src.January))
                .ForMember(dest => dest.M11, opt => opt.MapFrom(src => src.February))
                .ForMember(dest => dest.M12, opt => opt.MapFrom(src => src.March));
        }
    }
}
