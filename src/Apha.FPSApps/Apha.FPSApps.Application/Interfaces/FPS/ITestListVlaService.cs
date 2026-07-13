using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for TestOrProduct VLA list view operations.
    /// Thin delegate surface — all methods forward to IFpsApiClient.FpsTestListVla.
    /// Backend routes: GET /api/v1/testlistvla and GET /api/v1/testlistvla/lookup.
    /// </summary>
    public interface ITestListVlaService
    {
        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear);

        Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear);

        Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear);
    }
}
