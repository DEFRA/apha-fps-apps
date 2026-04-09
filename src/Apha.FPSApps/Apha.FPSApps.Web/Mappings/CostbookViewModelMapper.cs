using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class CostbookViewModelMapper: Profile
    {
        public CostbookViewModelMapper()
        {
            
            CreateMap<ProjectDto, ProjectItemViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectDetailViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectCreateEditViewModel>().ReverseMap();
        }
    }
}
