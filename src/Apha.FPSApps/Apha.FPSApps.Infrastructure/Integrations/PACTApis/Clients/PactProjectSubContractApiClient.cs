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
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactProjectSubContractApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            try
            {
                string baseUrl = string.IsNullOrWhiteSpace(project)
                    ? PactApiEndpoints.GetPagedProjectSubContracts
                    : QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedProjectSubContracts, new { project });
                string url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<ProjectSubContractRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProjectSubContractDto>>>(response);
                return ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectSubContractDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project sub-contracts", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter)
        {
            try
            {
                var response = await _http.GetAsync<ProjectSubContractRes>(
                    string.Format(PactApiEndpoints.GetProjectSubContractById, subContCounter));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve project sub-contract", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto)
        {
            try
            {
                ProjectSubContractReq request = _mapper.Map<ProjectSubContractReq>(dto);
                var response = await _http.PostAsync<ProjectSubContractReq, ProjectSubContractRes>(
                    PactApiEndpoints.CreateProjectSubContract, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create project sub-contract", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto)
        {
            try
            {
                ProjectSubContractReq request = _mapper.Map<ProjectSubContractReq>(dto);
                var response = await _http.PutAsync<ProjectSubContractReq, ProjectSubContractRes>(
                    string.Format(PactApiEndpoints.UpdateProjectSubContract, subContCounter), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectSubContractDto>>(response);
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectSubContractDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update project sub-contract", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(
                    string.Format(PactApiEndpoints.DeleteProjectSubContract, subContCounter));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete project sub-contract", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project)
        {
            try
            {
                string url = string.IsNullOrWhiteSpace(project)
                    ? PactApiEndpoints.GetProjectSubContractTotalAmount
                    : QueryStringHelper.AddQueryString(PactApiEndpoints.GetProjectSubContractTotalAmount, new { project });
                var response = await _http.GetAsync<decimal>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<decimal>>(response);

                var dto = _mapper.Map<ApiResponseDto<decimal>>(response);
                return ApiResponseDto<decimal>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<decimal>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve sub-contract total amount", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
