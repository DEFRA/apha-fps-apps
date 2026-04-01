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
    public class PactTimeCodeValidApiClient : IPactTimeCodeValidApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";
        private const string BaseEndpoint = "api/timecodevalid";

        public PactTimeCodeValidApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            try
            {
                var response = await _http.GetAsync<List<TimeCodeValidRes>>(
                    $"{BaseEndpoint}/jobcode/{Uri.EscapeDataString(jobCode)}/project/{Uri.EscapeDataString(parentProject)}");
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve time codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
        {
            try
            {
                var baseUrl = $"{BaseEndpoint}/paged";
                var sep = "?";
                if (!string.IsNullOrWhiteSpace(jobCode))
                {
                    baseUrl += $"{sep}jobCode={Uri.EscapeDataString(jobCode)}";
                    sep = "&";
                }
                if (!string.IsNullOrWhiteSpace(parentProject))
                    baseUrl += $"{sep}parentProject={Uri.EscapeDataString(parentProject)}";

                var url = QueryStringHelper.AddQueryString(baseUrl, query);
                var response = await _http.GetAsync<List<TimeCodeValidRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged time codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TimeCodeValidDto>> CreateTimeCodeValidAsync(TimeCodeValidDto item)
        {
            try
            {
                var request = _mapper.Map<TimeCodeValidReq>(item);
                var response = await _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(BaseEndpoint, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);
                return ApiResponseDto<TimeCodeValidDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TimeCodeValidDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create time code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<TimeCodeValidDto>> UpdateTimeCodeValidAsync(TimeCodeValidDto item)
        {
            try
            {
                var request = _mapper.Map<TimeCodeValidReq>(item);
                var response = await _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(BaseEndpoint, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);
                return ApiResponseDto<TimeCodeValidDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<TimeCodeValidDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update time code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            try
            {
                var url = $"{BaseEndpoint}/{Uri.EscapeDataString(workGroup)}/{Uri.EscapeDataString(timeCode)}/{Uri.EscapeDataString(parentProject)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete time code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            try
            {
                var url = $"{BaseEndpoint}/jobcode/{Uri.EscapeDataString(jobCode)}/project/{Uri.EscapeDataString(parentProject)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete time codes for job code", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
        {
            try
            {
                var url = $"{BaseEndpoint}/copy?sourceJobCode={Uri.EscapeDataString(sourceJobCode)}&targetJobCode={Uri.EscapeDataString(targetJobCode)}&parentProject={Uri.EscapeDataString(parentProject)}";
                var response = await _http.PostAsync<object, List<TimeCodeValidRes>>(url, new { });
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to copy work group time codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
