using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProjectYearCostsRepository
    {
        Task<PagedData<MyProjSubContract>> GetAdditionalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<MyTblAdditionalCosts>> GetAdditionalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<MyProjSubContract>> GetAnimalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<MyProjectAnimalPlan>> GetAnimalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<MyTlkpTestReqmt>> GetTestPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PagedData<(MyMonthlyOutput Output, MyTlkpTestReqmt Reqmt)>> GetTestActualsAsync(string project, short year, PaginationParameters<string> paging);
    }
}
