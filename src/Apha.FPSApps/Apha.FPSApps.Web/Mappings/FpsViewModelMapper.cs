using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
namespace Apha.FPSApps.Web.Mappings
{
    public class FpsViewModelMapper : Profile
    {
        public FpsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<StaffJobItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();
            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<AnimalMaintenanceViewModel, AnimalDto>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDto>().ReverseMap();
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectEditViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectItem>()
                .ForMember(d => d.TransferIncome, o => o.MapFrom(s => s.TransferIncome))
                .ReverseMap();
            CreateMap<AnimalPlanItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalRequestDto>().ReverseMap();
            CreateMap<CompareStaff2Item, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ActualProjectCostItem, ProjectSubContractDto>().ReverseMap();
            CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();
            CreateMap<DivisionGradeItem, DivisionGradeDto>().ReverseMap();
            CreateMap<ResourceCentreMaintenanceItem, ProfitCentreDto>().ReverseMap();
            CreateMap<TestPlanItem, Apha.FPSApps.Application.Dtos.PACT.TestRequirementDto>().ReverseMap();
            CreateMap<AdditionalCostItemViewModel, AdditionalCostDto>().ReverseMap();
            CreateMap<TestPlanActualItem, Apha.FPSApps.Application.Dtos.PACT.TestRequirementDto>().ReverseMap();
            CreateMap<ActualTestOutputItem, MonthlyOutputDto>().ReverseMap();

            // ProgrammeNewProject
            CreateMap<ProjectDto, ProgrammeNewProjectViewModel>().ReverseMap();

            // Resource Set-Up
            CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>().ReverseMap();

            // ProfitCentreGradeMaint
            CreateMap<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().ReverseMap();

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityItem>().ReverseMap();

            // Staff Plan view
            CreateMap<StaffPlanViewItem, ProjectStaffPlanViewDto>().ReverseMap();

            // Test Supplier
            CreateMap<TestSupplierItem, Apha.FPSApps.Application.Dtos.FPS.TestSupplierViewDto>()
                .ForMember(d => d.JobCode, o => o.MapFrom(s => s.Buyer))
                .ForMember(d => d.NoTests, o => o.MapFrom(s => s.NoTests))
                .ForMember(d => d.TestPrice, o => o.MapFrom(s => s.TestPrice))
                .ReverseMap()
                .ForMember(d => d.Buyer, o => o.MapFrom(s => s.JobCode));
            CreateMap<TestSupplierItem, Apha.FPSApps.Application.Dtos.FPS.FpsTestRequirementDto>()
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.TestPrice))
                .ForMember(d => d.NoRequired, o => o.MapFrom(s => s.NoTests))
                .ReverseMap()
                .ForMember(d => d.TestPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.NoTests, o => o.MapFrom(s => s.NoRequired));
        }
    }
}
