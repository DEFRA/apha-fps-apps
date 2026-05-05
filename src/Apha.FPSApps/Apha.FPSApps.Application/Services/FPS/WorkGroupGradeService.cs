using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupGradeService : IWorkGroupGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(string pcGrade)
        {
            return await _fpsClient.FpsWorkGroupGrade.GetWorkGroupGradeAsync(new QueryParameters<string>(), pcGrade);
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupGrade.DeleteWorkGroupGradeAsync(wgGrade);
        }
    }
}
