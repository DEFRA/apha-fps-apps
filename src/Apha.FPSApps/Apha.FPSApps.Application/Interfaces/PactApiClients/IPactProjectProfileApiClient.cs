using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactProjectProfileApiClient
    {
        Task<ApiResponseDto<List<ProjectProfileGraphDto>>> GetProfileGraphDataAsync(string project);
        Task<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>> GetCumulativeGraphDataAsync(string project);
    }
}
