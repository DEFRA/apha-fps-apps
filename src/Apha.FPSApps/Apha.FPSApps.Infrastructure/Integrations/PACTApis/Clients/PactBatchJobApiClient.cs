using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactBatchJobApiClient : IPactBatchJobApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactBatchJobApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>> GetBatchJobHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetBatchJobHistory, query);
            url += $"&jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<List<BatchJobHistoryRes>>(url);

            if (response.Success)
            {
                var dto = _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
                var pagination = response.Pagination;
                var result = new PaginatedResult<BatchJobHistoryDto>(
                    dto.Data ?? [],
                    pagination?.TotalRecords ?? 0,
                    pagination?.PageNumber ?? query.Page,
                    pagination?.PageSize ?? query.PageSize);
                return ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(result);
            }

            var failDto = _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(response);
            return ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> CanRunBatchJobAsync(string jobName)
        {
            var url = $"{PactApiEndpoints.CanRunBatchJob}?jobName={Uri.EscapeDataString(jobName)}";
            var response = await _http.GetAsync<bool>(url);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(response.Data);

            var failDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failDto.Errors, failDto.Meta);
        }

        public async Task<ApiResponseDto<BatchJobQueueDto>> TriggerRecreateSummariesJobAsync(int month)
        {
            var request = new TriggerRecreateSummariesReq { Month = month };
            var response = await _http.PostAsync<TriggerRecreateSummariesReq, BatchJobQueueRes>(
                PactApiEndpoints.TriggerRecreateSummariesJob, request);

            if (response.Success && response.Data is not null)
                return ApiResponseDto<BatchJobQueueDto>.SuccessResponse(_mapper.Map<BatchJobQueueDto>(response.Data));

            var failDto = _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(response);
            return ApiResponseDto<BatchJobQueueDto>.FailureResponse(failDto.Errors, failDto.Meta);
        }
    }
}
