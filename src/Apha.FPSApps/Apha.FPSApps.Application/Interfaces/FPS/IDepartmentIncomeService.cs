using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IDepartmentIncomeService
    {
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        // Snapshot test income — uses period_monthlyoutput delta (fPeriodTests equivalent)
        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestSnapshotIncomeAsync(
            string? project = null,
            int? startPeriod = null,
            int? endPeriod = null);

        // Snapshot time income — uses period_timecostcalcs delta (fPeriodTime equivalent)
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeSnapshotIncomeAsync(
            string? project = null,
            int? startPeriod = null,
            int? endPeriod = null);

        // Snapshot animal income — uses period_proj_subcontract delta (fPeriodAnimals equivalent)
        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalSnapshotIncomeAsync(
            string? project = null,
            int? startPeriod = null,
            int? endPeriod = null);

        // Snapshot exceptional income — uses period_proj_subcontract delta (fPeriodExceptional equivalent)
        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetExceptionalSnapshotIncomeAsync(
            string? project = null,
            int? startPeriod = null,
            int? endPeriod = null);

        // Snapshot totals — union of the four fPeriod* snapshot diffs (fPeriodTotals equivalent)
        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsSnapshotAsync(
            string? project = null,
            int? startPeriod = null,
            int? endPeriod = null);

        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsAsync(
            string? project = null,
            int? monthFrom = null,
            int? monthTo = null);

        Task<ApiResponseDto<List<PeriodLookupDto>>> GetPeriodsAsync(double? accntsPeriod = null);

        Task<ApiResponseDto<List<PeriodSnapshotDto>>> GetSnapshotPeriodsAsync();

        Task<ApiResponseDto<bool>> UpdatePeriodLockedAsync(string periodName, bool periodLocked);

        // Current (old style) variants
        Task<ApiResponseDto<List<DepartmentIncomeTimeDto>>> GetTimeIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeTestDto>>> GetTestIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeAnimalDto>>> GetAnimalIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeAdditionalDto>>> GetAdditionalIncomeCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
        Task<ApiResponseDto<List<DepartmentIncomeTotalsDto>>> GetTotalsCurrentAsync(string? project = null, int? monthFrom = null, int? monthTo = null);
    }
}
