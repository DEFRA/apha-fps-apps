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
    public class FpsAccountCategoryApiClient : IFpsAccountCategoryApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsAccountCategoryApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetFilteredAccountCategoriesAsync(QueryParameters<string> criteria, string? filterType = null)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetFilteredAccountCategories, filterType ?? "all"), criteria);
            var response = await _http.GetAsync<List<AccountCategoryRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
                return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> GetAccountCategoryByIdAsync(string accShortName)
        {
            var response = await _http.GetAsync<AccountCategoryRes>(string.Format(FpsApiEndpoints.GetAccountCategoryById, accShortName));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
                return ApiResponseDto<AccountCategoryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> CreateAccountCategoryAsync(AccountCategoryDto accountCategory)
        {
            var accountCategoryReq = _mapper.Map<AccountCategoryReq>(accountCategory);
            var response = await _http.PostAsync<AccountCategoryReq, AccountCategoryRes>(FpsApiEndpoints.CreateAccountCategory, accountCategoryReq);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
                return ApiResponseDto<AccountCategoryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<AccountCategoryDto>> UpdateAccountCategoryAsync(string originalAccShortName, AccountCategoryDto accountCategory)
        {
            var accountCategoryReq = _mapper.Map<AccountCategoryReq>(accountCategory);
            var response = await _http.PutAsync<AccountCategoryReq, AccountCategoryRes>(string.Format(FpsApiEndpoints.UpdateAccountCategory, originalAccShortName), accountCategoryReq);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<AccountCategoryDto>>(response);
                return ApiResponseDto<AccountCategoryDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAccountCategoryAsync(string accShortName)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteAccountCategory, accShortName));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
