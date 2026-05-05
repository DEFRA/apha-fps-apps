using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WgGradeService : IWgGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public WgGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWgGradesAsync(string pcGrade)
        {
            return await _fpsClient.FpsWgGrade.GetWgGradesAsync(new QueryParameters<string>(), pcGrade);
        }

        public async Task<ApiResponseDto<bool>> DeleteWgGradeAsync(string wgGrade)
        {
            return await _fpsClient.FpsWgGrade.DeleteWgGradeAsync(wgGrade);
        }
    }
}
