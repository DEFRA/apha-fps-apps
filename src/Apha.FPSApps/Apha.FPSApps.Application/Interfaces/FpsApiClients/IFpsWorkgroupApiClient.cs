using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWorkgroupApiClient
    {
        Task<ApiResponseDto<List<string>>> GetAllWorkgroupNamesAsync();
    }
}
