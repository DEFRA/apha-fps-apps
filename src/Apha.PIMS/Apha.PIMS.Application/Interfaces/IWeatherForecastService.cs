using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Application.Interfaces
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
