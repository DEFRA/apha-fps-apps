using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsJobCodeApiClient : IFpsJobCodeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsJobCodeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<FpsJobCodeDto>>> GetZtJobCodesAsync()
        {
            var response = await _http.GetAsync<IEnumerable<JobCodeRes>>(FpsApiEndpoints.GetZtJobCodes);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<FpsJobCodeDto>>>(response);
            }
            var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<FpsJobCodeDto>>>(response);
            return ApiResponseDto<IEnumerable<FpsJobCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
