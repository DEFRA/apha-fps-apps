using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
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
