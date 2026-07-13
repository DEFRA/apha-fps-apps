using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for TestOrProduct VLA list view operations.
    /// Targets backend route: GET /api/v1/testlistvla and lookup: GET /api/v1/testlistvla/lookup.
    /// </summary>
    public interface IFpsTestListVlaApiClient
    {
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear);

        Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear);
    }
}
