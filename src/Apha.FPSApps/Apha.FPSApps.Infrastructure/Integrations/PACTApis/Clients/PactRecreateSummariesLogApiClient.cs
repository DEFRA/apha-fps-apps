using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactRecreateSummariesLogApiClient : IPactRecreateSummariesLogApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactRecreateSummariesLogApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<RecreateSummariesLogDto>>> GetAllLogsAsync()
        {
            var response = await _http.GetAsync<List<RecreateSummariesLogRes>>(PactApiEndpoints.GetAllRecreateSummariesLogs);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<RecreateSummariesLogDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<RecreateSummariesLogDto>>>(response);
            return ApiResponseDto<List<RecreateSummariesLogDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
