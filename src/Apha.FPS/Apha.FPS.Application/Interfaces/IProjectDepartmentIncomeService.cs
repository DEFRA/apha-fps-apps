using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectDepartmentIncomeService
    {
        Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeTimeDto>> GetPagedTimeIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

        // Snapshot time income — uses period_timecostcalcs delta (equivalent to SQL Server fPeriodTime)
        Task<List<DepartmentIncomeTimeDto>> GetTimeSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot animal income — uses period_proj_subcontract delta (equivalent to SQL Server fPeriodAnimals)
        Task<List<DepartmentIncomeAnimalDto>> GetAnimalSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot exceptional income — uses period_proj_subcontract delta (equivalent to SQL Server fPeriodExceptional)
        Task<List<DepartmentIncomeAdditionalDto>> GetExceptionalSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot totals — union of the four fPeriod* snapshot diffs (equivalent to SQL Server fPeriodTotals)
        Task<List<DepartmentIncomeTotalsDto>> GetTotalsSnapshotAsync(string? project, int startPeriod, int endPeriod);

        Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeTestDto>> GetPagedTestIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

        // Snapshot test income — uses period_monthlyoutput delta (equivalent to SQL Server fPeriodTests)
        Task<List<DepartmentIncomeTestDto>> GetTestSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeAnimalDto>> GetPagedAnimalIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeAdditionalDto>> GetPagedAdditionalIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo);

        // Current (old style) variants — raw qryDeptIncome* live-table queries
        Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo);
        Task<List<DepartmentIncomeTestDto>> GetTestIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo);
        Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo);
        Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo);
        Task<List<DepartmentIncomeTotalsDto>> GetTotalsCurrentAsync(string? project, int? monthFrom, int? monthTo);

        Task<List<PeriodLookupDto>> GetPeriodsAsync(double? accntsPeriod = null);

        Task<List<PeriodSnapshotDto>> GetSnapshotPeriodsAsync();

        Task<int> UpdatePeriodLockedAsync(string periodName, bool periodLocked);
    }
}
