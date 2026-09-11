using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Mapster;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsDepartmentIncomeApiDtoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // All 18 properties align by convention between DepartmentIncomeTimeRes and DepartmentIncomeTimeDto
            config.NewConfig<DepartmentIncomeTimeRes, DepartmentIncomeTimeDto>().TwoWays();

            // All 14 properties align by convention between DepartmentIncomeTestRes and DepartmentIncomeTestDto
            config.NewConfig<DepartmentIncomeTestRes, DepartmentIncomeTestDto>().TwoWays();

            // All 13 properties align by convention between DepartmentIncomeAnimalRes and DepartmentIncomeAnimalDto
            config.NewConfig<DepartmentIncomeAnimalRes, DepartmentIncomeAnimalDto>().TwoWays();

            // All 8 properties align by convention between DepartmentIncomeAdditionalRes and DepartmentIncomeAdditionalDto
            config.NewConfig<DepartmentIncomeAdditionalRes, DepartmentIncomeAdditionalDto>().TwoWays();

            // All 7 properties align by convention between DepartmentIncomeTotalsRes and DepartmentIncomeTotalsDto
            config.NewConfig<DepartmentIncomeTotalsRes, DepartmentIncomeTotalsDto>().TwoWays();

            // Dedicated lookup DTO — must NOT be handled by CRUD entity mappers above
            // AccntsPeriod, MonthName, MonthNumber align by convention
            config.NewConfig<PeriodLookupRes, PeriodLookupDto>().TwoWays();

            // Snapshot periods — EndPeriod, PeriodName, FinalSummariesRun, PeriodLocked align by convention
            config.NewConfig<PeriodSnapshotRes, PeriodSnapshotDto>().TwoWays();
        }
    }
}
