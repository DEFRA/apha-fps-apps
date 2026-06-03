using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsTestSupplierApiClient : IFpsTestSupplierApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsTestSupplierApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedAsync(
            QueryParameters<string> query,
            string testCode,
            bool showRejected)
        {
            var baseUrl = QueryStringHelper.AddQueryString(
                FpsApiEndpoints.GetPagedTestSupplier,
                new { testCode, showRejected });
            var url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<TestSupplierViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TestSupplierViewDto>>>(response);
            return ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TestSupplierViewDto>> GetViewByIdAsync(string testCode, string buyer)
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var pagedResponse = await GetPagedAsync(query, testCode, showRejected: true);

            if (!pagedResponse.Success || pagedResponse.Data == null)
            {
                return ApiResponseDto<TestSupplierViewDto>.FailureResponse(pagedResponse.Errors, pagedResponse.Meta);
            }

            var match = pagedResponse.Data.FirstOrDefault(x =>
                string.Equals(x.JobCode, buyer, StringComparison.OrdinalIgnoreCase));

            if (match == null)
                return ApiResponseDto<TestSupplierViewDto>.FailureResponse(
                    new List<Application.Dtos.ApiErrorDto> { new() { Code = "404", Message = $"Record for TestCode '{testCode}' and Buyer '{buyer}' not found." } },
                    new Application.Dtos.ApiMetaDto());

            return new ApiResponseDto<TestSupplierViewDto> { Success = true, Data = match };
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> GetByIdAsync(string testCode, string buyer)
        {
            var url = string.Format(FpsApiEndpoints.GetTestSupplierById, testCode, buyer);
            var response = await _http.GetAsync<TestRequirementRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);
            return ApiResponseDto<FpsTestRequirementDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> CreateAsync(FpsTestRequirementDto dto)
        {
            var req = _mapper.Map<TestRequirementReq>(dto);
            var response = await _http.PostAsync<TestRequirementReq, TestRequirementRes>(
                FpsApiEndpoints.CreateTestSupplier, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);
            return ApiResponseDto<FpsTestRequirementDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<FpsTestRequirementDto>> UpdateAsync(FpsTestRequirementDto dto)
        {
            var req = _mapper.Map<TestRequirementReq>(dto);
            var response = await _http.PutAsync<TestRequirementReq, TestRequirementRes>(
                FpsApiEndpoints.UpdateTestSupplier, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<FpsTestRequirementDto>>(response);
            return ApiResponseDto<FpsTestRequirementDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer)
        {
            var url = string.Format(FpsApiEndpoints.DeleteTestSupplier, testCode, buyer);
            var response = await _http.DeleteAsync<bool?>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
