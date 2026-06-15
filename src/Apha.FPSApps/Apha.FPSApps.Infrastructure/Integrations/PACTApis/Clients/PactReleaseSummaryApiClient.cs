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

        public async Task<ApiResponseDto<ReleaseSummaryDto>> GetReleaseSummariesAsync()
        {
            var response = await _http.GetAsync<ReleaseSummaryRes>(PactApiEndpoints.GetReleaseSummaries);

            if (response.Success && response.Data is not null)
                return ApiResponseDto<ReleaseSummaryDto>.SuccessResponse(_mapper.Map<ReleaseSummaryDto>(response.Data));

            var failDto = _mapper.Map<ApiResponseDto<ReleaseSummaryDto>>(response);
            return ApiResponseDto<ReleaseSummaryDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>> GetReleasePeriodsAsync()
        {
            var response = await _http.GetAsync<IReadOnlyList<ReleasePeriodRes>>(PactApiEndpoints.GetReleasePeriods);

            if (response.Success && response.Data is not null)
                return ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.SuccessResponse(_mapper.Map<IReadOnlyList<ReleasePeriodDto>>(response.Data));

            var failDto = _mapper.Map<ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>>(response);
            return ApiResponseDto<IReadOnlyList<ReleasePeriodDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<ReleasePeriodDto>> SetFinalSummaryRunAsync(string? periodName, short? finalSummariesRun, string? sendEmail)
        {
            var request = new ReleasePeriodReq { PeriodName = periodName, FinalSummariesRun = finalSummariesRun, SendEmail = sendEmail };
            var response = await _http.PutAsync<ReleasePeriodReq, ReleasePeriodRes>(PactApiEndpoints.SetFinalSummaryRun, request);

            if (response.Success && response.Data is not null)
                return ApiResponseDto<ReleasePeriodDto>.SuccessResponse(_mapper.Map<ReleasePeriodDto>(response.Data));

            var failDto = _mapper.Map<ApiResponseDto<ReleasePeriodDto>>(response);
            return ApiResponseDto<ReleasePeriodDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}