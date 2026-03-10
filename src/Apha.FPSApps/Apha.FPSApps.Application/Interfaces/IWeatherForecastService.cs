using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecastAsync();
        Task<byte[]> ExportWeatherForecast();
    }
}
