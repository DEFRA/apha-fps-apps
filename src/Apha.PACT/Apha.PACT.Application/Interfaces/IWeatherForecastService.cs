using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Application.Interfaces
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
