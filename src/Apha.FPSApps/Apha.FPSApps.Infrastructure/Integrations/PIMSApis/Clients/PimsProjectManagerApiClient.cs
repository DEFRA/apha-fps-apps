/*
 * TRANSFORMENGINE MIGRATION — PimsProjectManagerApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New HTTP API client implementing IPimsProjectManagerApiClient
 *   - Binds to backend ProjectManagerController routes:
 *       GET    /api/v1/projectmanager                    — full list
 *       GET    /api/v1/projectmanager/{projectmanager}   — natural varchar PK get
 *       POST   /api/v1/projectmanager                    — create
 *       PUT    /api/v1/projectmanager/{projectmanager}   — update; route PK is authoritative
 *       DELETE /api/v1/projectmanager/{projectmanager}   — delete
 *   - Natural varchar string PK (projectmanager name) — Uri.EscapeDataString applied before URL embedding
 *   - Every HTTP call wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - Req/Res contracts: ProjectManagerReq, ProjectManagerRes from Apha.Common.Contracts.PIMS
 *
 * PRESERVED:
 *   - Natural string PK semantics (projectmanager name as identifier)
 *   - All CRUD semantics matching IPimsProjectManagerApiClient interface
 *   - Return types wrapped in ApiResponseDto<T>
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm rename scenario handling (delete+create vs update-in-place) at implementation layer
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsProjectManagerApiClient : IPimsProjectManagerApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        // TRANSFORMENGINE: S1192 — repeated error code extracted to const
        private const string InternalCodeError = "INTERNAL_ERROR";
        // TRANSFORMENGINE: S1192 — base URL extracted to const; matches backend ProjectManagerController [Route("api/v{version:apiVersion}/projectmanager")]
        private const string BaseUrl = "api/v1/projectmanager";

        public PimsProjectManagerApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET /api/v1/projectmanager — full list
        public async Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<ProjectManagerRes>>(BaseUrl);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProjectManagerDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<ProjectManagerDto>>>(response);
                return ApiResponseDto<List<ProjectManagerDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProjectManagerDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProjectManager data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET /api/v1/projectmanager/{projectmanager} — Uri.EscapeDataString applied to natural varchar PK
        public async Task<ApiResponseDto<ProjectManagerDto>> GetByIdAsync(string projectmanager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectmanager)}";
                var response = await _http.GetAsync<ProjectManagerRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve ProjectManager by ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST /api/v1/projectmanager
        public async Task<ApiResponseDto<ProjectManagerDto>> CreateAsync(ProjectManagerDto dto)
        {
            try
            {
                var request = _mapper.Map<ProjectManagerReq>(dto);
                var response = await _http.PostAsync<ProjectManagerReq, ProjectManagerRes>(BaseUrl, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT /api/v1/projectmanager/{projectmanager} — route PK is authoritative; Uri.EscapeDataString applied
        public async Task<ApiResponseDto<ProjectManagerDto>> UpdateAsync(string projectmanager, ProjectManagerDto dto)
        {
            try
            {
                var request = _mapper.Map<ProjectManagerReq>(dto);
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectmanager)}";
                var response = await _http.PutAsync<ProjectManagerReq, ProjectManagerRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<ProjectManagerDto>>(response);
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<ProjectManagerDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE /api/v1/projectmanager/{projectmanager} — Uri.EscapeDataString applied to natural varchar PK
        public async Task<ApiResponseDto<bool>> DeleteAsync(string projectmanager)
        {
            try
            {
                var url = $"{BaseUrl}/{Uri.EscapeDataString(projectmanager)}";
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete ProjectManager", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
