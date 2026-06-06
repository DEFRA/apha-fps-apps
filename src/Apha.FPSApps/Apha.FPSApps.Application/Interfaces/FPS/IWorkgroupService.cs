using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkgroupService
    {
        Task<ApiResponseDto<List<string>>> GetAllWorkgroupNamesAsync();
    }
}
