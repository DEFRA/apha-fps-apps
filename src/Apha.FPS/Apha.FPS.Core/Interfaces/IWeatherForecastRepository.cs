using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWeatherForecastRepository
    {
        Task<IEnumerable<WeatherForecast>> Get();
        Task<PagedData<WeatherForecast>> SearchWeather(
               PaginationParameters<object> query);        
        Task<PagedData<WeatherForecast>> SearchWeatherByModel(
                    PaginationParameters<WeatherForecastCriteria> query);
    }
}
