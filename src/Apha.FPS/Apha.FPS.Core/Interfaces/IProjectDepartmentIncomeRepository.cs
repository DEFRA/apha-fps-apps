using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IProjectDepartmentIncomeRepository
    {
        // Snapshot variants — use fPeriod* period-diff functions (aggregated)
        Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeTime>> GetPagedTimeIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

        Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeTest>> GetPagedTestIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

        // Snapshot test income using period_monthlyoutput delta (fPeriodTests equivalent):
        // end-period snapshot minus start-period snapshot, HAVING abs(sum(volume)) > 0
        Task<List<DepartmentIncomeTest>> GetTestSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot time income using period_timecostcalcs delta (fPeriodTime equivalent):
        // end-period snapshot minus start-period snapshot, HAVING abs(sum(time)) > 0.001 AND project NOT LIKE 'ZT%'
        Task<List<DepartmentIncomeTime>> GetTimeSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot animal income using period_proj_subcontract delta (fPeriodAnimals equivalent):
        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice"), HAVING abs(sum(amount)) > 0.001
        Task<List<DepartmentIncomeAnimal>> GetAnimalSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot exceptional income using period_proj_subcontract delta (fPeriodExceptional equivalent):
        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice"), HAVING abs(sum(amount)) > 0.001 (negatives retained)
        Task<List<DepartmentIncomeAdditional>> GetExceptionalSnapshotIncomeAsync(string? project, int startPeriod, int endPeriod);

        // Snapshot totals — union of the four fPeriod* snapshot diffs (fPeriodTotals equivalent)
        Task<List<DepartmentIncomeTotals>> GetTotalsSnapshotAsync(string? project, int startPeriod, int endPeriod);

        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeAnimal>> GetPagedAnimalIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in implementation
        Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeAdditional>> GetPagedAdditionalIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

        Task<List<DepartmentIncomeTotals>> GetTotalsAsync(string? project, int monthFrom, int monthTo);

        // Current (old style) variants — use raw qryDeptIncome* live-table queries (no period aggregation)
        Task<List<DepartmentIncomeTime>> GetTimeIncomeCurrentAsync(string? project, int monthFrom, int monthTo);
        Task<List<DepartmentIncomeTest>> GetTestIncomeCurrentAsync(string? project, int monthFrom, int monthTo);
        Task<List<DepartmentIncomeAnimal>> GetAnimalIncomeCurrentAsync(string? project, int monthFrom, int monthTo);
        Task<List<DepartmentIncomeAdditional>> GetAdditionalIncomeCurrentAsync(string? project, int monthFrom, int monthTo);
        Task<List<DepartmentIncomeTotals>> GetTotalsCurrentAsync(string? project, int monthFrom, int monthTo);

        Task<List<PeriodLookup>> GetPeriodsAsync(double? accntsPeriod = null);

        Task<List<Period>> GetSnapshotPeriodsAsync();

        Task<int> UpdatePeriodLockedAsync(string periodName, bool periodLocked);
    }
}
