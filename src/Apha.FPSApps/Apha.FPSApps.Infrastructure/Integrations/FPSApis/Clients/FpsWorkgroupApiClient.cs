using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsWorkgroupApiClient : IFpsWorkgroupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsWorkgroupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkgroupNamesAsync()
        {
            var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllWorkgroupNames);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
