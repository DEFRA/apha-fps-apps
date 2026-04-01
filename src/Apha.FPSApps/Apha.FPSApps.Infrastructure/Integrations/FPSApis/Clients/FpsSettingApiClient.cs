using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsSettingApiClient : IFpsSettingApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsSettingApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<decimal>> GetHoursPerDayAsync()
        {
            try
            {
                var response = await _http.GetAsync<decimal>("api/setting/hoursperday");

                if (response.Success)
                    return _mapper.Map<ApiResponseDto<decimal>>(response);

                var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
                return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<decimal>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve hours per day setting", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
