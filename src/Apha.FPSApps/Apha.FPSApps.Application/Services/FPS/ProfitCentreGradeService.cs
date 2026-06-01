using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ProfitCentreGradeService : IProfitCentreGradeService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProfitCentreGradeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(string profitCentre)
        {
            return await _fpsClient.FpsProfitCentreGrade.GetProfitCentreGradesAsync(new QueryParameters<string>(), profitCentre);
        }

        public async Task<ApiResponseDto<List<string>>> GetAllPcGradesAsync()
            => await _fpsClient.FpsProfitCentreGrade.GetAllPcGradesAsync();
    }
}
