using Apha.Common.Utilities.StateManagement;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
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
