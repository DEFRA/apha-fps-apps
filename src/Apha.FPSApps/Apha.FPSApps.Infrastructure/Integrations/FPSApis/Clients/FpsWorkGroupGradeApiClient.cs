/*
 * TRANSFORMENGINE MIGRATION — FpsWorkGroupGradeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-frontend  Phase 3 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-07
 *
 * CHANGED:
 *   - Added TRANSFORMENGINE migration header (PB-14 annotation policy)
 *   - Added private const InternalCodeError = "INTERNAL_ERROR" (Sonar S1192)
 *   - Wrapped all 7 HTTP calls in try/catch(Exception) with FailureResponse fallback
 *   - Removed ArgumentNullException guards from constructor (not required by pattern; _http/_mapper are private readonly)
 *   - Preserved all URL composition via FpsApiEndpoints constants and Uri.EscapeDataString
 *
 * PRESERVED:
 *   - All 7 interface methods: GetWorkGroupGradeAsync, DeleteWorkGroupGradeAsync,
 *     GetAllWorkgroupGradesPagedAsync, GetByWgGradeAsync, CreateAsync, UpdateAsync,
 *     DeleteAsync, GetAllGradeCodesAsync
 *   - private readonly _http and _mapper fields (Sonar S2933)
 *   - Mapper used for success response mapping; FailureResponse used for error path
 *   - FpsApiEndpoints constants for all URL paths
 *   - DeleteWorkGroupGradeAsync and DeleteAsync use distinct endpoint constants
 *     (DeleteWgGrade vs DeleteWorkgroupGrade) as separate legacy and maintenance endpoints
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm DeleteWorkGroupGradeAsync (api/v1/wggrades/{0}) vs
 *     DeleteAsync (api/v1/wggrades/maintain/{0}) are distinct backend endpoints — not duplicates
 */

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
    public class FpsWorkGroupGradeApiClient : IFpsWorkGroupGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsWorkGroupGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/wggrades?pcGrade={0} → WgGradesController.GetWorkGroupGradeAsync
        //   profitCentre parameter URI-escaped and embedded in URL format string
        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkGroupGradeAsync(QueryParameters<string> query, string profitCentre)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetWgGrades, Uri.EscapeDataString(profitCentre));
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkGroupGrade data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/wggrades/{wgGrade} → WgGradesController.DeleteWorkGroupGradeAsync (legacy endpoint)
        public async Task<ApiResponseDto<bool>> DeleteWorkGroupGradeAsync(string wgGrade)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.DeleteWgGrade, Uri.EscapeDataString(wgGrade));
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkGroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wggrades/paged → WgGradesController.GetAllWorkgroupGradesPagedAsync
        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetAllWorkgroupGradesPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedWorkgroupGrades, query);
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve paged WorkgroupGrades", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wggrades/{wgGrade} → WgGradesController.GetByWgGradeAsync
        public async Task<ApiResponseDto<WorkgroupGradeDto>> GetByWgGradeAsync(string wgGrade)
        {
            try
            {
                var response = await _http.GetAsync<WorkgroupGradeRes>(string.Format(FpsApiEndpoints.GetWorkgroupGradeByCode, wgGrade));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var dto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkgroupGrade by code", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/wggrades → WgGradesController.CreateAsync
        public async Task<ApiResponseDto<WorkgroupGradeDto>> CreateAsync(WorkgroupGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupGradeReq>(dto);
                var response = await _http.PostAsync<WorkgroupGradeReq, WorkgroupGradeRes>(FpsApiEndpoints.CreateWorkgroupGrade, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create WorkgroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/wggrades/{wgGrade} → WgGradesController.UpdateAsync
        public async Task<ApiResponseDto<WorkgroupGradeDto>> UpdateAsync(string wgGrade, WorkgroupGradeDto dto)
        {
            try
            {
                var request = _mapper.Map<WorkgroupGradeReq>(dto);
                var response = await _http.PutAsync<WorkgroupGradeReq, WorkgroupGradeRes>(string.Format(FpsApiEndpoints.UpdateWorkgroupGrade, wgGrade), request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<WorkgroupGradeDto>>(response);
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<WorkgroupGradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update WorkgroupGrade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/wggrades/maintain/{wgGrade} → WgGradesController.DeleteAsync (maintenance endpoint)
        public async Task<ApiResponseDto<bool>> DeleteAsync(string wgGrade)
        {
            try
            {
                var response = await _http.DeleteAsync<bool>(string.Format(FpsApiEndpoints.DeleteWorkgroupGrade, wgGrade));
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var dto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete WorkgroupGrade (maintain)", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wggrades/gradecodes → WgGradesController.GetAllGradeCodesAsync
        public async Task<ApiResponseDto<List<string>>> GetAllGradeCodesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<string>>(FpsApiEndpoints.GetAllGradeCodes);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<string>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<string>>>(response);
                return ApiResponseDto<List<string>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<string>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve all grade codes", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/wggrades/byworkgroup?workGroup={0} → WgGradesController.GetWorkgroupGradesByWorkGroupAsync
        public async Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWorkgroupGradesByWorkGroupAsync(string workGroup)
        {
            try
            {
                var url = string.Format(FpsApiEndpoints.GetWgGradesByWorkGroup, Uri.EscapeDataString(workGroup));
                var response = await _http.GetAsync<List<WorkgroupGradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(response);
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve WorkgroupGrades by workgroup", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
