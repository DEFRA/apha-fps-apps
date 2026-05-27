using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProjectYearCostsService
    {
        Task<PaginatedResult<AdditionalCostDto>> GetAdditionalActualsAsync(string project, short year, PaginationParameters<string> paging);
        Task<PaginatedResult<AdditionalCostDto>> GetAdditionalPlansAsync(string project, short year, PaginationParameters<string> paging);
    }
}
