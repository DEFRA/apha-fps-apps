using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// API client interface for Agency operations.
    /// </summary>
    public interface IFpsAgencyApiClient
    {
        /// <summary>
        /// Retrieves all agencies from the API.
        /// </summary>
        /// <returns>API response containing agency collection.</returns>
        Task<ApiResponseDto<IEnumerable<AgencyDto>>> GetAllAgenciesAsync();
    }
}
