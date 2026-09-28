using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Mapster;

namespace Apha.FPSApps.Web.Mappings
{
    public class DepartmentIncomeViewModelMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // All 18 properties align by convention
            config.NewConfig<DepartmentIncomeTimeItem, DepartmentIncomeTimeDto>().TwoWays();

            // All 14 properties align by convention
            config.NewConfig<DepartmentIncomeTestItem, DepartmentIncomeTestDto>().TwoWays();

            // All 13 properties align by convention
            config.NewConfig<DepartmentIncomeAnimalItem, DepartmentIncomeAnimalDto>().TwoWays();

            // All 8 properties align by convention
            config.NewConfig<DepartmentIncomeAdditionalItem, DepartmentIncomeAdditionalDto>().TwoWays();

            // All 7 properties align by convention (nullable decimal? pivot columns preserved)
            config.NewConfig<DepartmentIncomeTotalsItem, DepartmentIncomeTotalsDto>().TwoWays();

            // Snapshot periods — PeriodName, FinalSummariesRun, PeriodLocked aligned from PeriodSnapshotDto
            config.NewConfig<PeriodSnapshotDto, DepartmentIncomeSnapshotItem>()
                .Map(d => d.PeriodName, s => s.PeriodName)
                .Map(d => d.FinalSummariesRun, s => s.FinalSummariesRun)
                .Map(d => d.PeriodLocked, s => s.PeriodLocked)
                .Map(d => d.Month, s => (int)s.EndPeriod)
                .Ignore(d => d.ProjectCode);
        }
    }
}
