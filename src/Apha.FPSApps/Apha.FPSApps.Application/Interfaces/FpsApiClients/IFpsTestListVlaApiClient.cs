using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for TestOrProduct VLA list management.
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testlistvla
    /// and lookup: GET /api/v1/testlistvla/lookup
    /// Composite PK: ItemCode + FpsYear.
    /// </summary>
    public interface IFpsTestListVlaApiClient
    {
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear);

        Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear);

        Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto);

        Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto);

        Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear);
    }
}
