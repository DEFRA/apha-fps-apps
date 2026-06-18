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
            CreateMap<TestPlanItem, TestRequirementDto>().ReverseMap();
            CreateMap<AdditionalCostItemViewModel, AdditionalCostDto>().ReverseMap();
            CreateMap<TestPlanActualItem, TestRequirementDto>().ReverseMap();
            CreateMap<ActualTestOutputItem, MonthlyOutputDto>().ReverseMap();

            CreateMap<ProjectDto, ProgrammeNewProjectViewModel>().ReverseMap();

            CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>()
                .ForMember(d => d.MakeAvailable,   o => o.MapFrom(s => s.MakeAvailable ? 1 : 0))
                .ForMember(d => d.TimeRecorder,    o => o.MapFrom(s => s.TimeRecorder ? 1 : 0))
                .ForMember(d => d.Name,            o => o.MapFrom(s => s.StaffName ?? string.Empty))
                .ForMember(d => d.WorkGroupGrade,  o => o.MapFrom(s => s.WgGrade ?? string.Empty))
                .ForMember(d => d.SpNumber,        o => o.MapFrom(s => s.SpNumber ?? string.Empty))
                .ReverseMap()
                .ForMember(d => d.MakeAvailable,   o => o.MapFrom(s => s.MakeAvailable != 0))
                .ForMember(d => d.TimeRecorder,    o => o.MapFrom(s => s.TimeRecorder != 0))
                .ForMember(d => d.StaffName,       o => o.MapFrom(s => s.Name))
                .ForMember(d => d.WgGrade,         o => o.MapFrom(s => s.WorkGroupGrade))
                .ForMember(d => d.SpNumber,        o => o.MapFrom(s => s.SpNumber));

            CreateMap<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().ReverseMap();

            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityItem>().ReverseMap();

            CreateMap<StaffPlanViewItem, ProjectStaffPlanViewDto>().ReverseMap();

            CreateMap<TestSupplierItem, Apha.FPSApps.Application.Dtos.PACT.TestSupplierViewDto>().ReverseMap();
            CreateMap<TestSupplierItem, TestRequirementDto>()
                .ForMember(d => d.TestCode, o => o.MapFrom(s => s.TestCode))
                .ForMember(d => d.Buyer, o => o.MapFrom(s => s.Buyer))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.NoRequired, o => o.MapFrom(s => s.NoRequired))
                .ForMember(d => d.ProjectBuyerCode, o => o.MapFrom(s => s.ProjectBuyerCode))
                .ForMember(d => d.TestBuyerCode, o => o.MapFrom(s => s.TestBuyerCode))
                .ForMember(d => d.Active, o => o.MapFrom(s => s.Active))
                .ForMember(d => d.RecUnitPrice, o => o.MapFrom(s => s.RecUnitPrice))
                .ReverseMap();
            CreateMap<MaintWGGradeItem, WorkgroupGradeDto>().ReverseMap();

            CreateMap<Apha.FPSApps.Web.Areas.FPS.Models.TestCapabilityItem, Apha.FPSApps.Application.Dtos.PACT.TestCapabilityDto>().ReverseMap();

            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobDto>()
                .ForMember(d => d.StaffId, o => o.MapFrom(s => s.StaffID))
                .ReverseMap();
        }
    }
}
