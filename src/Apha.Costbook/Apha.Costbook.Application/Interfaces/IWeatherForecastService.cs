using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IWeatherForecastService
    {
        Task<IEnumerable<WeatherForecastDto>> Get();
        Task<PaginatedResult<WeatherForecastDto>> SearchWeather(
                QueryParameters<object> query);        
        Task<PaginatedResult<WeatherForecastDto>> SearchWeatherByModel(
            QueryParameters<WeatherForecastCriteriaDto> queryFilter);
    }
}
