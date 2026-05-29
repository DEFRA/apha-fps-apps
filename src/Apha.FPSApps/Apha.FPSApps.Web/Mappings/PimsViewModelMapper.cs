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
            CreateMap<ProposedProjectViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectCommentItem, CommentDto>().ReverseMap();
            // Plan grid item — maps from plan fields on the shared DTO
            CreateMap<AdditionalCostDto, AdditionalCostPlanItem>().ReverseMap();

            // Actuals grid item — maps from actuals fields on the shared DTO
            CreateMap<AdditionalCostDto, AdditionalCostActualItem>().ReverseMap();

            // Animal Plan grid item
            CreateMap<AnimalCostDto, AnimalCostPlanItem>().ReverseMap();

            // Animal Actuals grid item
            CreateMap<AnimalCostDto, AnimalCostActualItem>().ReverseMap();

            // Test Plan grid item
            CreateMap<TestCostDto, TestCostPlanItem>().ReverseMap();

            // Test Actuals grid item
            CreateMap<TestCostDto, TestCostActualItem>().ReverseMap();

            // Staff Plan grid item
            CreateMap<StaffCostDto, StaffCostPlanItem>().ReverseMap();

            // Staff Actuals grid item
            CreateMap<StaffCostDto, StaffCostActualItem>().ReverseMap();
        }
    }
}
