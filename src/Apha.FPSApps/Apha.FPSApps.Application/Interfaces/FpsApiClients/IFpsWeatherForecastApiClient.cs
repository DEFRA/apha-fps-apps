using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWeatherForecastApiClient
    {
        Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecast();
    }
}
