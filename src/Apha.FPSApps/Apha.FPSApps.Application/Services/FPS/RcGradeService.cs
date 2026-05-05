using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class RcGradeService : IRcGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public RcGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetRcGradesAsync(string profitCentre)
        {
            return await _fpsClient.FpsRcGrade.GetRcGradesAsync(new QueryParameters<string>(), profitCentre);
        }
    }
}
