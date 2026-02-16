using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Application.Interfaces
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
