using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
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
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>()
                .ForMember(d => d.JobCode, o => o.MapFrom(s => s.ParentProject))
                .ForMember(d => d.JobDescription, o => o.MapFrom(s => s.ProjectTitle))
                .ForMember(d => d.Programme, o => o.MapFrom(s => s.Program))
                .ForMember(d => d.Budget_cvl, o => o.MapFrom(s => s.BudgetCvl))
                .ReverseMap()
                .ForMember(d => d.ParentProject, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.ProjectTitle, o => o.MapFrom(s => s.JobDescription))
                .ForMember(d => d.Program, o => o.MapFrom(s => s.Programme))
                .ForMember(d => d.BudgetCvl, o => o.MapFrom(s => s.Budget_cvl));
        }
    }
}
