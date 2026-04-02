using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsSettingApiClient
    {
        Task<ApiResponseDto<decimal>> GetHoursPerDayAsync();
    }
}
