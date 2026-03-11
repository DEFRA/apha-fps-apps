using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IFpsApiClient _fpsClient;

        public WeatherForecastService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecastAsync()
        {
            var response = _fpsClient.FpsWeatherForecast.GetWeatherForecast();
            return response;
        }

        public Task<byte[]> ExportWeatherForecast()
        {
            var response = _fpsClient.FpsWeatherForecast.ExportWeatherForecast();
            return response;
        }
    }
}
