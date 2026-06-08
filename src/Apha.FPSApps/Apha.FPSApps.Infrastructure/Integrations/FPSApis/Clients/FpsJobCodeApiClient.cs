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

        public async Task<ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>> GetZtJobCodesAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ZtJobCodeRes>>(FpsApiEndpoints.GetZtJobCodes);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>>(response);
            }
            var responseDto = _mapper.Map<ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>>(response);
            return ApiResponseDto<IEnumerable<FpsZtJobCodeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
