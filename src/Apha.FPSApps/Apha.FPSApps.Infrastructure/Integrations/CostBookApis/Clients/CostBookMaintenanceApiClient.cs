/*
 * TRANSFORMENGINE MIGRATION — CostBookMaintenanceApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend HTTP API client created for frmMaintainance Tabs 1, 2, and 4
 *   - Implements ICostBookMaintenanceApiClient via ICostBookHttpExecutor
 *   - GetSettingsAsync()            → GET  api/v1/maintenance/settings
 *   - UpdateSettingsAsync()         → PUT  api/v1/maintenance/settings
 *   - GetAccountCategoriesAsync()   → GET  api/v1/maintenance/account-categories
 *   - UpdateAccountCategoryAsync()  → PUT  api/v1/maintenance/account-categories/{accShortName}
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - AutoMapper used to map Req/Res backend contracts to/from frontend DTOs
 *
 * PRESERVED:
 *   - Backend MaintenanceController route template api/v1/maintenance preserved exactly
 *   - accShortName route parameter URL-encoded via HttpUtility.UrlEncode
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether a paginated overload is needed for account-categories (none exists on backend)
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear filter is needed on GetAccountCategoriesAsync (currently server-side derived)
 */

using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookMaintenanceApiClient : ICostBookMaintenanceApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: Backend route api/v{version:apiVersion}/maintenance — exact match required
        private const string SettingsEndpoint = "api/v1/maintenance/settings";
        private const string AccountCategoriesEndpoint = "api/v1/maintenance/account-categories";

        public CostBookMaintenanceApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET api/v1/maintenance/settings → Tab 1 (Inflation) + Tab 4 (Profit) settings grid
        public async Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync()
        {
            try
            {
                var response = await _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve maintenance settings", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/maintenance/settings → Tab 1 + Tab 4 bulk update; Admin role required on backend
        public async Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            try
            {
                var request = _mapper.Map<MaintenanceSettingsReq>(dto);
                var response = await _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update maintenance settings", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/maintenance/account-categories → Tab 2 account categories grid
        public async Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryMaintenanceDto>>>(response);
                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve account categories", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/maintenance/account-categories/{accShortName} → Tab 2 CSG7 group update; Admin role required on backend
        public async Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto)
        {
            try
            {
                var request = _mapper.Map<AccountCategoryMaintenanceReq>(dto);
                var url = $"{AccountCategoriesEndpoint}/{HttpUtility.UrlEncode(accShortName)}";
                var response = await _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(url, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(response);
                return ApiResponseDto<AccountCategoryMaintenanceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccountCategoryMaintenanceDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update account category", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
