// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — frmMaintGrade → FpsGradeApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet8-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-10
 *
 * CHANGED:
 *   - New frontend HTTP API client created in Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
 *   - Implements IFpsGradeApiClient with five async methods mirroring backend GradeController
 *   - Backend route: api/v{version:apiVersion}/Grade → BaseUrl: "api/v1/Grade"
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse on error
 *   - InternalCodeError and BaseUrl declared as private const string (Sonar S1192 compliance)
 *   - _http and _mapper declared as private readonly (Sonar S2933 compliance)
 *   - UpdateAsync takes originalCode string as path param to support GradeCode rename
 *   - DeleteAsync calls _http.DeleteAsync<bool?> returning ApiResponseDto<bool>
 *     (consistent with FpsDivisionGradeApiClient pattern)
 *
 * PRESERVED:
 *   - Five REST endpoints match backend GradeController exactly:
 *       GET  api/v1/Grade/paged         → GetAllPagedAsync
 *       GET  api/v1/Grade/{gradeCode}   → GetByIdAsync
 *       POST api/v1/Grade               → CreateAsync
 *       PUT  api/v1/Grade/{gradeCode}   → UpdateAsync (originalCode in path)
 *       DELETE api/v1/Grade/{gradeCode} → DeleteAsync
 *   - Request/response mapping uses GradeReq/GradeRes contracts (Apha.Common.Contracts.FPS)
 *   - Success path uses AutoMapper for response DTO conversion (never manual construction)
 *   - Error path maps response first then calls FailureResponse (consistent with existing clients)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: When FpsApiEndpoints.cs Grade constants are added (PENDING in
 *     Interface changes log), replace inline BaseUrl and paged-URL literals with
 *     FpsApiEndpoints.GetPagedGrades, FpsApiEndpoints.GetGradeById, etc.
 *   - TRANSFORMENGINE TODO: Register IFpsGradeApiClient → FpsGradeApiClient in
 *     Apha.FPSApps.Infrastructure ServiceCollectionExtension.AddHttpClients() before running.
 *   - TRANSFORMENGINE TODO: Verify FpsApiDtoMapper has GradeDto <-> GradeReq and
 *     GradeDto <-> GradeRes mappings registered (PENDING in Interface changes log).
 */

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
    public class FpsGradeApiClient : IFpsGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: InternalCodeError as private const — Sonar S1192 compliance
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: BaseUrl matches backend GradeController [Route("api/v{version:apiVersion}/Grade")]
        // TRANSFORMENGINE TODO: Replace with FpsApiEndpoints.CreateGrade / FpsApiEndpoints.GetPagedGrades etc.
        //   once Grade endpoint constants are added to FpsApiEndpoints.cs (PENDING in Interface changes log).
        private const string BaseUrl = "api/v1/Grade";

        public FpsGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GET api/v1/Grade/paged → GradeController.GetAllPagedAsync
        //   QueryParameters<string> appended as query string via QueryStringHelper.AddQueryString
        public async Task<ApiResponseDto<List<GradeDto>>> GetAllPagedAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString($"{BaseUrl}/paged", query);
                var response = await _http.GetAsync<List<GradeRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<GradeDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<GradeDto>>>(response);
                return ApiResponseDto<List<GradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<GradeDto>>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Grade data", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/Grade/{gradeCode} → GradeController.GetByIdAsync
        public async Task<ApiResponseDto<GradeDto>> GetByIdAsync(string gradeCode)
        {
            try
            {
                var url = $"{BaseUrl}/{gradeCode}";
                var response = await _http.GetAsync<GradeRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to retrieve Grade by ID", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/Grade → GradeController.CreateAsync
        //   GradeDto mapped to GradeReq for the request body
        public async Task<ApiResponseDto<GradeDto>> CreateAsync(GradeDto dto)
        {
            try
            {
                var request = _mapper.Map<GradeReq>(dto);
                var response = await _http.PostAsync<GradeReq, GradeRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to create Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/Grade/{originalCode} → GradeController.UpdateAsync
        //   originalCode placed in path to support GradeCode rename (dto.GradeCode may differ from originalCode)
        public async Task<ApiResponseDto<GradeDto>> UpdateAsync(string originalCode, GradeDto dto)
        {
            try
            {
                var request = _mapper.Map<GradeReq>(dto);
                var url = $"{BaseUrl}/{originalCode}";
                var response = await _http.PutAsync<GradeReq, GradeRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<GradeDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<GradeDto>>(response);
                return ApiResponseDto<GradeDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<GradeDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to update Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/Grade/{gradeCode} → GradeController.DeleteAsync
        //   bool? used as generic arg (nullable response body); return type is ApiResponseDto<bool>
        public async Task<ApiResponseDto<bool>> DeleteAsync(string gradeCode)
        {
            try
            {
                var url = $"{BaseUrl}/{gradeCode}";
                var response = await _http.DeleteAsync<bool?>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Message = "Failed to delete Grade", Code = InternalCodeError } },
                    new ApiMetaDto());
            }
        }
    }
}
