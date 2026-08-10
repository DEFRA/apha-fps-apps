using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IDepartmentIncomeRepository
    {
        // Snapshot variants — use fPeriod* period-diff functions (aggregated)
        Task<List<DepartmentIncomeTime>> GetTimeIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeTime>> GetPagedTimeIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

        Task<List<DepartmentIncomeTest>> GetTestIncomeAsync(string? project, int monthFrom, int monthTo);
        Task<PagedData<DepartmentIncomeTest>> GetPagedTestIncomeAsync(PaginationParameters<string> query, string? project, int monthFrom, int monthTo);

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
