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

        public async Task<ApiResponseDto<List<ProjectProfileGraphDto>>> GetProfileGraphDataAsync(string project)
        {
            var response = await _http.GetAsync<List<ProjectProfileGraphRes>>(
                string.Format(PactApiEndpoints.GetProjectProfileGraph, Uri.EscapeDataString(project)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(response);
            return ApiResponseDto<List<ProjectProfileGraphDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>> GetCumulativeGraphDataAsync(string project)
        {
            var response = await _http.GetAsync<List<ProjectProfileCumulativeGraphRes>>(
                string.Format(PactApiEndpoints.GetProjectProfileCumulativeGraph, Uri.EscapeDataString(project)));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(response);
            return ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
