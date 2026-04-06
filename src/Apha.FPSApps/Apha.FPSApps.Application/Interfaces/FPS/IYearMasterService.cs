using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IYearMasterService
    {
        Task<ApiResponseDto<IEnumerable<YearMasterDto>>> GetAllYearMastersAsync();
        Task<ApiResponseDto<List<YearMasterDto>>> GetAllYearMastersPagedAsync(QueryParameters<int> query);
        Task<ApiResponseDto<YearMasterDto>> GetYearMasterByIdAsync(int fpsYear);
    }
}
