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
    public class PactTimeCodeValidApiClient : IPactTimeCodeValidApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

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
                    string.Format(PactApiEndpoints.GetTimeCodesByJobCode, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject)));
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
                string baseUrl;
                if (!string.IsNullOrWhiteSpace(jobCode) && !string.IsNullOrWhiteSpace(parentProject))
                    baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByJobCodeAndProject, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject));
                else if (!string.IsNullOrWhiteSpace(jobCode))
                    baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByJobCode, Uri.EscapeDataString(jobCode));
                else if (!string.IsNullOrWhiteSpace(parentProject))
                    baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByProject, Uri.EscapeDataString(parentProject));
                else
                    baseUrl = PactApiEndpoints.GetPagedTimeCodes;

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
                var response = await _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(PactApiEndpoints.CreateTimeCodeValid, request);
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
                var response = await _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(PactApiEndpoints.UpdateTimeCodeValid, request);
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
                var url = string.Format(PactApiEndpoints.DeleteTimeCodeValid, Uri.EscapeDataString(workGroup), Uri.EscapeDataString(timeCode), Uri.EscapeDataString(parentProject));
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
                var url = string.Format(PactApiEndpoints.DeleteTimeCodesByJobCode, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject));
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
                var url = string.Format(PactApiEndpoints.CopyWorkGroup, Uri.EscapeDataString(sourceJobCode), Uri.EscapeDataString(targetJobCode), Uri.EscapeDataString(parentProject));
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

        public async Task<ApiResponseDto<bool>> DeleteBulkAsync(BulkDeleteTimeCodeRequestDto request)
        {
            try
            {
                var body = new BulkDeleteTimeCodeReq
                {
                    ParentProject = request.ParentProject,
                    Items = request.Items
                        .Select(i => new TimeCodeKeyItem { WorkGroup = i.WorkGroup, TimeCode = i.TimeCode })
                        .ToList()
                };
                var response = await _http.PostAsync<BulkDeleteTimeCodeReq, bool>(PactApiEndpoints.DeleteBulkTimeCodes, body);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to bulk delete time codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopySelectedWorkGroupsAsync(BulkCopyWorkGroupRequestDto request)
        {
            try
            {
                var body = new BulkCopyWorkGroupReq
                {
                    ParentProject = request.ParentProject,
                    SourceJobCode = request.SourceJobCode,
                    TargetJobCode = request.TargetJobCode,
                    WorkGroups = request.WorkGroups
                };
                var response = await _http.PostAsync<BulkCopyWorkGroupReq, List<TimeCodeValidRes>>(PactApiEndpoints.CopySelectedWorkGroups, body);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to copy selected work group time codes", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
