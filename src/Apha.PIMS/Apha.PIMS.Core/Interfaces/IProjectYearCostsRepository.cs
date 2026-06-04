using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectYearCostsRepository
    {
        Task<PagedData<ProjSubContract>> GetAdditionalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<AdditionalCosts>> GetAdditionalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<ProjSubContract>> GetAnimalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<ProjectAnimalPlan>> GetAnimalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<TestReqmt>> GetTestPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>> GetTestActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<ProjectStaffPlan>> GetStaffPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<TimeCostCalcs>> GetStaffActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<Projects?> GetProjectYearDetailsAsync(string project, short year);
        Task<PagedData<PactPayCalc>> GetPactPayAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<ProjectMonthFinal>> GetMonthlyPactDataAsync(string project, short year, PaginationParameters<string> paging);
        Task<FpsYearTotal?> GetFpsYearTotalsAsync(string project, short year);
    }
}
