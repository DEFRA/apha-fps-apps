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
    public class PactJobCodeApiClient : IPactJobCodeApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PactJobCodeApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<JobCodeDto>>> GetJobCodesByProjectAsync(string parentProject)
        {
            try
            {
                var response = await _http.GetAsync<List<JobCodeRes>>(string.Format(PactApiEndpoints.GetJobCodesByProject, Uri.EscapeDataString(parentProject)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(response);
                return ApiResponseDto<List<JobCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<JobCodeDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve job codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<JobCodeDto>>> GetPagedJobCodesAsync(QueryParameters<string> query, string? parentProject)
        {
            try
            {
                var baseUrl = string.IsNullOrWhiteSpace(parentProject)
                    ? PactApiEndpoints.GetPagedJobCodes
                    : string.Format(PactApiEndpoints.GetPagedJobCodesByProject, Uri.EscapeDataString(parentProject));
                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<JobCodeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(response);
                return ApiResponseDto<List<JobCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<JobCodeDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged job codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<JobCodeDto>> GetJobCodeByIdAsync(string jobCodeId)
        {
            try
            {
                var response = await _http.GetAsync<JobCodeRes>(string.Format(PactApiEndpoints.GetJobCodeById, Uri.EscapeDataString(jobCodeId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<JobCodeDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<JobCodeDto>>(response);
                return ApiResponseDto<JobCodeDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<JobCodeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve job code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<string>>> GetTypesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(PactApiEndpoints.GetJobCodeTypes);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve job code types", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<JobCodeDto>> CreateJobCodeAsync(JobCodeDto jobCode)
        {
            try
            {
                var request = _mapper.Map<JobCodeReq>(jobCode);
                var response = await _http.PostAsync<JobCodeReq, JobCodeRes>(PactApiEndpoints.CreateJobCode, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<JobCodeDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<JobCodeDto>>(response);
                return ApiResponseDto<JobCodeDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<JobCodeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create job code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<JobCodeDto>> UpdateJobCodeAsync(JobCodeDto jobCode)
        {
            try
            {
                var request = _mapper.Map<JobCodeReq>(jobCode);
                var response = await _http.PutAsync<JobCodeReq, JobCodeRes>(PactApiEndpoints.UpdateJobCode, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<JobCodeDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<JobCodeDto>>(response);
                return ApiResponseDto<JobCodeDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<JobCodeDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update job code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteJobCodeAsync(string jobCodeId)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(string.Format(PactApiEndpoints.DeleteJobCode, Uri.EscapeDataString(jobCodeId)));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete job code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
