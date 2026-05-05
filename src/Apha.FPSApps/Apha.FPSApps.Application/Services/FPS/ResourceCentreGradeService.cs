using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ResourceCentreGradeService : IResourceCentreGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public ResourceCentreGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetResourceCentreGradesAsync(string profitCentre)
        {
            return await _fpsClient.FpsResourceCentreGrade.GetResourceCentreGradesAsync(new QueryParameters<string>(), profitCentre);
        }
    }
}
