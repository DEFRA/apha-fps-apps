using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
namespace Apha.FPSApps.Web.Mappings
{
    public class ViewModelMapper : Profile
    {
        public ViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();            
            CreateMap<StaffJobItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap(); 
            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap(); 
            CreateMap<EmployeeViewModel, EmployeeDto>().ReverseMap();
            CreateMap<PactProjectViewModel, ProjectDto>().ReverseMap();
            CreateMap<ProjectJobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<JobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalRequestDto>()
                .ForMember(d => d.IndCounter, o => o.MapFrom(s => s.IndCounter))
                .ForMember(d => d.JobCode, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.AnimalType, o => o.MapFrom(s => s.AnimalType))
                .ForMember(d => d.NumberOfDays, o => o.MapFrom(s => s.NumberOfDays))
                .ForMember(d => d.NumberOfAnimals, o => o.MapFrom(s => s.NumberOfAnimals))
                .ReverseMap();
        }
    }
}
