using Apha.Common.Utilities.StateManagement;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly IWeatherForecastRepository _weatherForecastRepository;
        private readonly IMapper _mapper;
        private readonly IAppStateService _cacheService;
        public WeatherForecastService(IWeatherForecastRepository weatherForecastRepository, IMapper mapper, IAppStateService cacheService)
        {
            _weatherForecastRepository = weatherForecastRepository;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<WeatherForecastDto>> Get()
        {
            //var cacheKey = Constants.Weather.WeatherList(2026);

            //var cachedWeather = await _cacheService.GetCacheValueAsync<IEnumerable<WeatherForecastDto>>(cacheKey);
            //if (cachedWeather != null)
            //{
            //    return _mapper.Map<IEnumerable<WeatherForecast>>(cachedWeather);
            //}
            var wether = await _weatherForecastRepository.Get();
            var weatherDtos = _mapper.Map<IEnumerable<WeatherForecastDto>>(wether);
            //await _cacheService.SetCacheValueAsync(cacheKey, weatherDtos, TimeSpan.FromMinutes(10));
            return weatherDtos; // Fixed the return type mapping
        }

        public async Task<PaginatedResult<WeatherForecastDto>> SearchWeather(QueryParameters<object> queryFilter)
        {
            var filter = _mapper.Map<PaginationParameters<object>>(queryFilter);
            var wether = await _weatherForecastRepository.SearchWeather(filter);
            return  _mapper.Map<PaginatedResult<WeatherForecastDto>>(wether);          
        }

        public async Task<PaginatedResult<WeatherForecastDto>> SearchWeatherByModel(QueryParameters<WeatherForecastCriteriaDto> queryFilter)
        {
            var filter = _mapper.Map<PaginationParameters<WeatherForecastCriteria>>(queryFilter);
            var wether = await _weatherForecastRepository.SearchWeatherByModel(filter);
            return _mapper.Map<PaginatedResult<WeatherForecastDto>>(wether);
        }
    }
}
