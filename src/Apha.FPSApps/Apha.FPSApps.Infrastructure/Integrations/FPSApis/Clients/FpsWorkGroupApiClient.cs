using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsWorkGroupApiClient : IFpsWorkGroupApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsWorkGroupApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
        {
            var response = await _http.GetAsync<List<WorkGroupViewDto>>(string.Format(FpsApiEndpoints.GetWorkGroups, profitCentre));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
