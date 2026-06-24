/*
 * TRANSFORMENGINE MIGRATION — CostBookAccountGroupApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New frontend HTTP API client created for frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Implements ICostBookAccountGroupApiClient via ICostBookHttpExecutor
 *   - GetAllAccountGroupsAsync()     → GET    api/v1/accountgroup
 *   - GetAccountGroupAsync()         → GET    api/v1/accountgroup/{csg7Group}
 *   - AddAccountGroupAsync()         → POST   api/v1/accountgroup
 *   - UpdateAccountGroupAsync()      → PUT    api/v1/accountgroup/{csg7Group}
 *   - DeleteAccountGroupAsync()      → DELETE api/v1/accountgroup/{csg7Group}
 *   - GetAllAccountGroupsAsync() also serves as the CSG7 dropdown source for AccountCategory modal (Tab 2)
 *   - All HTTP calls wrapped in try/catch(Exception) returning FailureResponse with InternalCodeError
 *   - AutoMapper used to map AccountGroupReq/Res backend contracts to/from AccountGroupDto
 *
 * PRESERVED:
 *   - Backend AccountGroupController route template api/v1/accountgroup preserved exactly
 *   - Csg7Group route parameter URL-encoded via HttpUtility.UrlEncode
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Csg7Group max length (varchar 15) validation is enforced at service/controller level
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
    public class CostBookAccountGroupApiClient : ICostBookAccountGroupApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        // TRANSFORMENGINE: Backend route api/v{version:apiVersion}/accountgroup — exact match required
        private const string BaseUrl = "api/v1/accountgroup";

        public CostBookAccountGroupApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GET api/v1/accountgroup → full list for Tab 3 grid + CSG7 dropdown in AccountCategory modal (Tab 2)
        public async Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccountGroupRes>>(BaseUrl);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<List<AccountGroupDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccountGroupDto>>>(response);
                return ApiResponseDto<List<AccountGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccountGroupDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve account groups", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: GET api/v1/accountgroup/{csg7Group} → single record lookup for edit/delete modal
        public async Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group)
        {
            try
            {
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(csg7Group)}";
                var response = await _http.GetAsync<AccountGroupRes>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
                return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccountGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve account group", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: POST api/v1/accountgroup → create from Tab 3 add modal (formTblCsg7); Admin role required on backend
        public async Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<AccountGroupReq>(dto);
                var response = await _http.PostAsync<AccountGroupReq, AccountGroupRes>(BaseUrl, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
                return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccountGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to add account group", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: PUT api/v1/accountgroup/{csg7Group} → update from Tab 3 edit modal; Admin role required on backend
        public async Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            try
            {
                var request = _mapper.Map<AccountGroupReq>(dto);
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(csg7Group)}";
                var response = await _http.PutAsync<AccountGroupReq, AccountGroupRes>(url, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
                return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccountGroupDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update account group", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        // TRANSFORMENGINE: DELETE api/v1/accountgroup/{csg7Group} → delete from Tab 3 confirm modal; Admin role required on backend
        public async Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group)
        {
            try
            {
                var url = $"{BaseUrl}/{HttpUtility.UrlEncode(csg7Group)}";
                var response = await _http.DeleteAsync<bool?>(url);

                if (response.Success && response.Data.HasValue)
                    return ApiResponseDto<bool>.SuccessResponse(response.Data.Value);

                if (response.Success && !response.Data.HasValue)
                    return ApiResponseDto<bool>.SuccessResponse(true);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete account group", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
