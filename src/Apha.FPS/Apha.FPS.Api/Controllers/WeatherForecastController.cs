using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// Controller for managing weather forecast operations.
    /// </summary>
    [ApiController]
    [Route("api/weather")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecastService _weatherForecastService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeatherForecastController"/> class.
        /// </summary>
        /// <param name="weatherForecastService">Service for weather forecast operations.</param>
        /// <param name="mapper">Mapper for DTO conversions.</param>
        public WeatherForecastController(
                        IWeatherForecastService weatherForecastService,
                        IMapper mapper)
        {
            _weatherForecastService = weatherForecastService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all weather forecasts.
        /// </summary>
        /// <returns>A list of weather forecast responses.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var forecasts = await _weatherForecastService.Get();
            if (!forecasts.Any())
                throw new KeyNotFoundException("DEFAULT_WEATHER data not found.");

            var result = _mapper.Map<IEnumerable<WeatherForecastRes>>(forecasts);
            return Ok(result);
        }

        /// <summary>
        /// Searches weather forecasts using pagination and optional search parameters.
        /// </summary>
        /// <param name="query">Pagination and search parameters.</param>
        /// <returns>Paginated weather forecast responses.</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchAsync(
                [FromQuery] PaginationReq<object> query)
        {
            var filter = _mapper.Map<QueryParameters<object>>(query);
            var forecasts = await _weatherForecastService.SearchWeather(filter);
            return Ok(_mapper.Map<PaginationRes<WeatherForecastRes>>(forecasts));
        }

        /// <summary>
        /// Searches weather forecasts using model criteria and pagination.
        /// </summary>
        /// <param name="query">Pagination and filter criteria.</param>
        /// <returns>Paginated weather forecast responses.</returns>
        [HttpPost("search")]
        public async Task<IActionResult> SearchByModelAsync(
               [FromBody] PaginationReq<WeatherForecastCriteriaReq> query)
        {
            var filter = _mapper.Map<QueryParameters<WeatherForecastCriteriaDto>>(query);
            var forecasts = await _weatherForecastService.SearchWeatherByModel(filter);
            return Ok(_mapper.Map<PaginationRes<WeatherForecastRes>>(forecasts));
        }
    }
}
