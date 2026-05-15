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
    public class PactProjectSubContractApiClient : IPactProjectSubContractApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactProjectSubContractApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        private const string InternalCodeError = "INTERNAL_ERROR";

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            string baseUrl = string.IsNullOrWhiteSpace(project)
                ? PactApiEndpoints.GetPagedProjectSubContracts
                : QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedProjectSubContracts, new { project });
            string url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<ProjectSubContractRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);
            return ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter)
        {
            var response = await _http.GetAsync<ProjectSubContractRes>(
                string.Format(PactApiEndpoints.GetProjectSubContractById, subContCounter));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
            return ApiResponseDto<ProjectSubContractDto>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto)
        {
            ProjectSubContractReq request = _mapper.Map<ProjectSubContractReq>(dto);
            var response = await _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>(
                PactApiEndpoints.CreateProjectSubContract, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
            return ApiResponseDto<ProjectSubContractDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto)
        {
            ProjectSubContractReq request = _mapper.Map<ProjectSubContractReq>(dto);
            var response = await _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>(
                string.Format(PactApiEndpoints.UpdateProjectSubContract, subContCounter), request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
            return ApiResponseDto<ProjectSubContractDto>.FailureResponse(responseDto.Errors, responseDto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter)
        {
            var response = await _http.DeleteAsync<bool?>(
                string.Format(PactApiEndpoints.DeleteProjectSubContract, subContCounter));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project)
        {
            string url = string.IsNullOrWhiteSpace(project)
                ? PactApiEndpoints.GetProjectSubContractTotalAmount
                : QueryStringHelper.AddQueryString(PactApiEndpoints.GetProjectSubContractTotalAmount, new { project });

            var response = await _http.GetAsync<decimal?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<decimal>>(response);

            var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
            return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false)
        {
            string baseUrl = QueryStringHelper.AddQueryString(PactApiEndpoints.GetFpsProjectSubContracts, new { filterByAnimalAcctCodes });
            if (!string.IsNullOrWhiteSpace(project))
                baseUrl = QueryStringHelper.AddQueryString(baseUrl, new { project });
            string url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<ProjectSubContractRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);
            return ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
        }

        public async Task<ApiResponseDto<decimal>> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false)
        {
            string url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetFpsProjectSubContractTotalAmount, new { filterByAnimalAcctCodes });
            if (!string.IsNullOrWhiteSpace(project))
                url = QueryStringHelper.AddQueryString(url, new { project });

            try
            {
                var response = await _http.GetAsync<decimal?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<decimal>>(response);

                var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
                return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta ?? new ApiMetaDto());
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponseDto<decimal>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Code = InternalCodeError, Message = ex.Message } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }
        }

        public async Task<ApiResponseDto<MonthlySubContractsPivotDto>> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetMonthlySubContractsSummary, query);
            var response = await _http.GetAsync<MonthlySubContractsPivotRes>(url);
            if (response.Success)
            {
                var dto = _mapper.Map<MonthlySubContractsPivotDto>(response.Data);
                return ApiResponseDto<MonthlySubContractsPivotDto>.SuccessResponse(dto);
            }

            var failresponseDto = _mapper.Map<ApiResponseDto<MonthlySubContractsPivotDto>>(response);
            return ApiResponseDto<MonthlySubContractsPivotDto>.FailureResponse(failresponseDto.Errors, failresponseDto.Meta);
        }
    }
}
