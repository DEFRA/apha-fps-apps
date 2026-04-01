using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface ISettingService
    {
        Task<ApiResponseDto<decimal>> GetHoursPerDayAsync();
    }
}
