using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class DepartmentIncomeViewModelMapper : Profile
    {
        public DepartmentIncomeViewModelMapper()
        {
            // All 18 properties align by convention
            CreateMap<DepartmentIncomeTimeItem, DepartmentIncomeTimeDto>().ReverseMap();

            // All 14 properties align by convention
            CreateMap<DepartmentIncomeTestItem, DepartmentIncomeTestDto>().ReverseMap();

            // All 13 properties align by convention
            CreateMap<DepartmentIncomeAnimalItem, DepartmentIncomeAnimalDto>().ReverseMap();

            // All 8 properties align by convention
            CreateMap<DepartmentIncomeAdditionalItem, DepartmentIncomeAdditionalDto>().ReverseMap();

            // All 7 properties align by convention (nullable decimal? pivot columns preserved)
            CreateMap<DepartmentIncomeTotalsItem, DepartmentIncomeTotalsDto>().ReverseMap();

            // Snapshot periods — PeriodName, FinalSummariesRun, PeriodLocked aligned from PeriodSnapshotDto
            CreateMap<PeriodSnapshotDto, DepartmentIncomeSnapshotItem>()
                .ForMember(d => d.PeriodName,         o => o.MapFrom(s => s.PeriodName))
                .ForMember(d => d.FinalSummariesRun,  o => o.MapFrom(s => s.FinalSummariesRun))
                .ForMember(d => d.PeriodLocked,       o => o.MapFrom(s => s.PeriodLocked))
                .ForMember(d => d.Month,              o => o.MapFrom(s => (int)s.EndPeriod))
                .ForMember(d => d.ProjectCode,        o => o.Ignore());
        }
    }
}
