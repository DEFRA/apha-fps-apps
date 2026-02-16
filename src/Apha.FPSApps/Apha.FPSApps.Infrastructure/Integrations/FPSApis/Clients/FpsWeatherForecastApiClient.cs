using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.DTOs;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsWeatherForecastApiClient : IFpsWeatherForecastApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsWeatherForecastApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }
        public async Task<ApiResponseDto<List<WeatherForecastDto>>> GetWeatherForecast()
        {      
            try
            {
                var response = await _http.GetAsync<List<WeatherForecastRes>>(
                    $"api/weather");

                if (response.Success && response.Data != null)
                {
                    return _mapper.Map<ApiResponseDto<List<WeatherForecastDto>>>(response);
                }
                else
                {
                    var responseDto = _mapper.Map<ApiResponseDto<List<WeatherForecastDto>>>(response);
                    return ApiResponseDto<List<WeatherForecastDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
                }            
            }
            catch (Exception ex)
            {
                var apiErrosDto = new List<ApiErrorDto> {
                   new ApiErrorDto {
                        Message = "Failed to retrieve user",
                        Code = "INTERNAL_ERROR",
                        Details = null
                    }
                };
                return ApiResponseDto<List<WeatherForecastDto>>.FailureResponse(apiErrosDto,
                   new ApiMetaDto());
            }
        }
    }
}
