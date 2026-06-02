using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactReleaseSummaryApiClient : IPactReleaseSummaryApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactReleaseSummaryApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>> GetReleaseSummariesAsync()
        {
            var response = await _http.GetAsync<List<ReleasePeriodRes>>(PactApiEndpoints.GetReleaseSummaries);

            if (response.Success)
            {
                var dtoList = _mapper.Map<IReadOnlyList<ReleasePeriodDto>>(response.Data ?? new List<ReleasePeriodRes>());
                return ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(dtoList);
            }

            var failDto = _mapper.Map<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(response);
            return ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<short>> SetFinalSummaryRunAsync(string periodName, short finalSummariesRun)
        {
            var request = new ReleasePeriodReq { PeriodName = periodName, FinalSummariesRun = finalSummariesRun };
            var response = await _http.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(PactApiEndpoints.SetFinalSummaryRun, request);

            if (response.Success && response.Data is not null)
                return ApiResponseDto<short>.SuccessResponse(response.Data.FinalSummariesRun ?? 0);

            var failDto = _mapper.Map<ApiResponseDto<short>>(response);
            return ApiResponseDto<short>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}