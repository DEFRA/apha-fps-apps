using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class ContributionSummaryService : IContributionSummaryService
    {
        private readonly IFpsApiClient _fpsClient;

        public ContributionSummaryService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public Task<ApiResponseDto<List<TimeSellerPcRowDto>>> GetRowsAsync(string sellingPc)
            => _fpsClient.FpsTimeSellerPc.GetRowsAsync(sellingPc);

        public Task<ApiResponseDto<TimeSellerPcTotalsDto>> GetTotalsAsync(string sellingPc)
            => _fpsClient.FpsTimeSellerPc.GetTotalsAsync(sellingPc);
    }
}
