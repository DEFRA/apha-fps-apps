using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsYearMasterApiClient
    {
        Task<ApiResponseDto<IEnumerable<YearMasterDto>>> GetAllFpsYearsAsync();
        Task<ApiResponseDto<List<YearMasterDto>>> GetAllFpsYearsPagedAsync(QueryParameters<int> query);
        Task<ApiResponseDto<YearMasterDto>> GetFpsYearByIdAsync(int fpsYear);
    }
}
