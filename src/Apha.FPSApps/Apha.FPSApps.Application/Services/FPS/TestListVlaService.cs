using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service delegate for TestOrProduct VLA list management.
    /// Forwards all calls to IFpsApiClient.FpsTestListVla — contains NO business logic.
    /// </summary>
    public class TestListVlaService : ITestListVlaService
    {
        private readonly IFpsApiClient _client;

        public TestListVlaService(IFpsApiClient client)
        {
            _client = client;
        }

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllAsync(QueryParameters<string> query, int fpsYear)
            => await _client.FpsTestListVla.GetAllAsync(query, fpsYear);

        public async Task<ApiResponseDto<List<TestListVlaDto>>> GetAllByYearAsync(int fpsYear)
            => await _client.FpsTestListVla.GetAllByYearAsync(fpsYear);

        public async Task<ApiResponseDto<TestListVlaDto>> GetByIdAsync(string itemCode, int fpsYear)
            => await _client.FpsTestListVla.GetByIdAsync(itemCode, fpsYear);

        public async Task<ApiResponseDto<TestListVlaDto>> CreateAsync(TestListVlaDto dto)
            => await _client.FpsTestListVla.CreateAsync(dto);

        public async Task<ApiResponseDto<TestListVlaDto>> UpdateAsync(string itemCode, int fpsYear, TestListVlaDto dto)
            => await _client.FpsTestListVla.UpdateAsync(itemCode, fpsYear, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(string itemCode, int fpsYear)
            => await _client.FpsTestListVla.DeleteAsync(itemCode, fpsYear);
    }
}
