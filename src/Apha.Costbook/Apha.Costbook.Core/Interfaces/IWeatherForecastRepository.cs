using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
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
