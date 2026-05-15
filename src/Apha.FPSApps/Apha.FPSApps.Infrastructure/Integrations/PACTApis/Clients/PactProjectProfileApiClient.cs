using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactProjectProfileApiClient : IPactProjectProfileApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactProjectProfileApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectProfileDto>>> GetProfileDataAsync(string project)
        {
            var response = await _http.GetAsync<List<ProjectProfileRes>>(
                string.Format(PactApiEndpoints.GetProjectProfile, Uri.EscapeDataString(project)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(response);
            return ApiResponseDto<List<ProjectProfileDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectProfileCumulativeDto>>> GetCumulativeDataAsync(string project)
        {
            var response = await _http.GetAsync<List<ProjectProfileCumulativeRes>>(
                string.Format(PactApiEndpoints.GetProjectProfileCumulative, Uri.EscapeDataString(project)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(response);
            return ApiResponseDto<List<ProjectProfileCumulativeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
