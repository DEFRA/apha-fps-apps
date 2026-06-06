using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsBudgetBidsApiClient : IFpsBudgetBidsApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsBudgetBidsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            var response = await _http.GetAsync<List<BidViewRes>>(string.Format(FpsApiEndpoints.GetBids, workgroup));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
            return ApiResponseDto<List<BidViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string workgroupName, string account)
        {
            var response = await _http.GetAsync<BidRes>(string.Format(FpsApiEndpoints.GetBidByWorkgroupAccount, workgroupName, account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PostAsync<BidReq, BidRes>(FpsApiEndpoints.CreateBudgetBid, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PutAsync<BidReq, BidRes>(FpsApiEndpoints.UpdateBudgetBid, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteBudgetBid, bid.WorkgroupName, bid.Account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            var response = await _http.GetAsync<List<AccountCategoryRes>>(FpsApiEndpoints.GetBudgetBidsAccounts);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
