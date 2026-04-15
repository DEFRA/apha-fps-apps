using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsViewModelMapper : Profile
    {
        public PimsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();

            CreateMap<ProjectListItem, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectCommentItem, CommentDto>().ReverseMap();
        }
    }
}
