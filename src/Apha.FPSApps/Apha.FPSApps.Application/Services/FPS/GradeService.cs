using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for the Grade maintenance resource.
    /// Thin delegate — all calls forwarded to <see cref="IFpsApiClient.FpsGrade"/> with no business logic.
    /// </summary>
    public class GradeService : IGradeService
    {
        // TRANSFORMENGINE: private readonly _fpsClient — Sonar S2933 compliance; aggregate API client injected via DI
        private readonly IFpsApiClient _fpsClient;

        public GradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsGrade.GetAllPagedAsync (no logic added)
        public async Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            return await _fpsClient.FpsGrade.GetAllPagedAsync(query);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsGrade.GetByIdAsync
        public async Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode)
        {
            return await _fpsClient.FpsGrade.GetByIdAsync(gradeCode);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsGrade.CreateAsync
        public async Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto)
        {
            return await _fpsClient.FpsGrade.CreateAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate — originalCode forwarded for GradeCode rename support
        public async Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto)
        {
            return await _fpsClient.FpsGrade.UpdateAsync(originalCode, dto);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsGrade.DeleteAsync
        public async Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode)
        {
            return await _fpsClient.FpsGrade.DeleteAsync(gradeCode);
        }
    }
}
