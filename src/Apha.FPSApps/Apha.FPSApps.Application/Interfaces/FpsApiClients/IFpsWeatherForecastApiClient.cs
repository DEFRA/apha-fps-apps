using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWeatherForecastApiClient
    {
        Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecast();
        Task<byte[]> ExportWeatherForecast();
    }
}
