using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectYearCostsService
    {
        Task<PaginatedResult<AdditionalCostDto>> GetAdditionalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<AdditionalCostDto>> GetAdditionalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<AnimalCostDto>> GetAnimalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<AnimalCostDto>> GetAnimalPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<TestCostDto>> GetTestPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<TestCostDto>> GetTestActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<StaffCostDto>> GetStaffPlansAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<StaffCostDto>> GetStaffActualsAsync(string project, short year, PaginationParameters<string> paging);
    }
}
