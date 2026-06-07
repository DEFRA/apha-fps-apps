using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
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
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<string>>> GetAllWorkGroupNamesAsync()
        {
            var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllWorkGroupNames);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<string>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
            return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
        {
            var response = await _http.GetAsync<List<WorkGroupRes>>(string.Format(FpsApiEndpoints.GetWorkGroups, profitCentre));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
