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

        public Task<ApiResponseDto<List<ContributionSummaryRowDto>>> GetRowsAsync(string sellingPc, string? sortBy = null, bool descending = false)
            => _fpsClient.FpsContributionSummary.GetRowsAsync(sellingPc, sortBy, descending);

        public Task<ApiResponseDto<ContributionSummaryTotalsDto>> GetTotalsAsync(string sellingPc)
            => _fpsClient.FpsContributionSummary.GetTotalsAsync(sellingPc);
    }
}
