using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactProjectProfileApiClient
    {
        Task<ApiResponseDto<List<ProjectProfileDto>>> GetProfileDataAsync(string project);
        Task<ApiResponseDto<List<ProjectProfileCumulativeDto>>> GetCumulativeDataAsync(string project);
    }
}
