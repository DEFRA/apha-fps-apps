using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;

namespace Apha.FPSApps.Application.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecastAsync();
    }
}
