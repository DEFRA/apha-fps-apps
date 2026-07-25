using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessUserLevelApiClient : IPimsAccessUserLevelApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend AccessUserLevelController [Route("api/v{version:apiVersion}/accessuserlevel")]
        private const string BaseUrl = "api/v1/accessuserlevel";

        public PimsAccessUserLevelApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/paged — paged, sorted, filtered list; builds query string from QueryParameters
        public async Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAsync(QueryParameters<string> request)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", request);
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                {
                    var items       = _mapper.Map<List<AccessUserLevelDto>>(response.Data ?? []);
                    var pageNumber  = response.Pagination?.PageNumber  ?? request.Page;
                    var pageSize    = response.Pagination?.PageSize    ?? request.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<AccessUserLevelDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged AccessUserLevel data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int} — scoped by system
        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}";
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin} — scoped by user within system; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetByUserAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}";
                var response = await _http.GetAsync<List<AccessUserLevelRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(response);
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserLevelDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by user", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int} — triple composite PK get; Uri.EscapeDataString on ntlogin
        public async Task<ApiResponseDto<AccessUserLevelDto>> GetByIdAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}/{accesslevelid}";
                var response = await _http.GetAsync<AccessUserLevelRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUserLevel by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/accessuserlevel — create assignment
        public async Task<ApiResponseDto<AccessUserLevelDto>> CreateAsync(AccessUserLevelDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserLevelReq>(dto);
                var response = await _http.PostAsync<AccessUserLevelReq, AccessUserLevelRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(response);
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserLevelDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/accessuserlevel/{systemid:int}/{ntlogin}/{accesslevelid:int} — triple composite PK delete; no PUT (no mutable fields)
        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin, int accesslevelid)
        {
            try
            {
                var url = $"{BaseUrl}/{systemid}/{Uri.EscapeDataString(ntlogin)}/{accesslevelid}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessUserLevel", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
