using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for WorkgroupGrade operations — delegates to FPS API.
    /// </summary>
    public class WorkgroupGradeService : IWorkgroupGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkgroupGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query)
            => await _fpsClient.FpsWorkgroupGrade.GetAllWorkgroupGradesPagedAsync(query);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade)
            => await _fpsClient.FpsWorkgroupGrade.GetByWgGradeAsync(wgGrade);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkgroupGrade.CreateAsync(dto);

        public async Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto)
            => await _fpsClient.FpsWorkgroupGrade.UpdateAsync(wgGrade, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade)
            => await _fpsClient.FpsWorkgroupGrade.DeleteAsync(wgGrade);

        public async Task<ApiResponseDto<List<string>>> GetAllPcGradesAsync()
            => await _fpsClient.FpsWorkgroupGrade.GetAllPcGradesAsync();

        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
            => await _fpsClient.FpsWorkgroupGrade.GetAllGradeCodesAsync();

        public async Task<ApiResponseDto<List<string>>> GetAllWorkgroupNamesAsync()
            => await _fpsClient.FpsWorkgroupGrade.GetAllWorkgroupNamesAsync();
    }
}
