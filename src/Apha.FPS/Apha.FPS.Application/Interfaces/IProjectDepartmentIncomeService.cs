using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IProjectDepartmentIncomeService
    {
        Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeTimeDto>> GetPagedTimeIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

        Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo);
        Task<PaginatedResult<DepartmentIncomeTestDto>> GetPagedTestIncomeAsync(QueryParameters<string> query, string? project, int? monthFrom, int? monthTo);

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
