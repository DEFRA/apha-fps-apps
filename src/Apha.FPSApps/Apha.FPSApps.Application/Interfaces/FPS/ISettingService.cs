using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ISettingService
    {
        Task<ApiResponseDto<decimal>> GetHoursPerDayAsync();
    }
}
