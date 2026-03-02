using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
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
