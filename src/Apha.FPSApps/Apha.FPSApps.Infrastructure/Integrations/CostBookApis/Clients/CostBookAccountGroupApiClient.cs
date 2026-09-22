using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookAccountGroupApiClient : ICostBookAccountGroupApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;

        public CostBookAccountGroupApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync()
        {
            var response = await _http.GetAsync<List<AccountGroupRes>>(CostBookApiEndpoints.GetAllAccountGroups);

            if (response.Success && response.Data != null)
                return ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(
                    _mapper.Map<List<AccountGroupDto>>(response.Data));

            return ApiResponseDto<List<AccountGroupDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors == null ? new List<ApiError>() : response.Errors),
                new ApiMetaDto());
        }

        public async Task<ApiResponseDto<List<AccountGroupDto>>> GetPaginatedAccountGroupsAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(
                CostBookApiEndpoints.GetPaginatedAccountGroups, query);

            var response = await _http.GetAsync<List<AccountGroupRes>>(url);

            if (response.Success && response.Data != null)
            {
                var dto = ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(
                    _mapper.Map<List<AccountGroupDto>>(response.Data));
                if (response.Pagination != null)
                {
                    dto.Pagination = new PaginationDto
                    {
                        PageNumber = response.Pagination.PageNumber,
                        PageSize = response.Pagination.PageSize,
                        TotalPages = response.Pagination.TotalPages,
                        TotalRecords = response.Pagination.TotalRecords
                    };
                }
                return dto;
            }

            return ApiResponseDto<List<AccountGroupDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors == null ? new List<ApiError>() : response.Errors),
                new ApiMetaDto());
        }

        public async Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group)
        {
            var url = string.Format(CostBookApiEndpoints.GetAccountGroupByCsg7, HttpUtility.UrlEncode(csg7Group));
            var response = await _http.GetAsync<AccountGroupRes>(url);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
            return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto)
        {
            var request = _mapper.Map<AccountGroupReq>(dto);
            var response = await _http.PostAsync<AccountGroupReq, AccountGroupRes>(CostBookApiEndpoints.AddAccountGroup, request);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
            return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            var request = _mapper.Map<AccountGroupReq>(dto);
            var url = string.Format(CostBookApiEndpoints.UpdateAccountGroup, HttpUtility.UrlEncode(csg7Group));
            var response = await _http.PutAsync<AccountGroupReq, AccountGroupRes>(url, request);

            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<AccountGroupDto>>(response);
            return ApiResponseDto<AccountGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group)
        {
            var url = string.Format(CostBookApiEndpoints.DeleteAccountGroup, HttpUtility.UrlEncode(csg7Group));

            var response = await _http.DeleteAsync<object>(url);

            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(true);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
