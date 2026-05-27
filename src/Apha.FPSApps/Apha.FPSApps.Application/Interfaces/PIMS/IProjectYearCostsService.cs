using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IProjectYearCostsService
    {
        Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalActualsAsync(string project, short year, QueryParameters<string> query);
        Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalPlansAsync(string project, short year, QueryParameters<string> query);
    }
}
