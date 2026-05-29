using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsBudgetResourceLevelApiClient : IFpsBudgetResourceLevelApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsBudgetResourceLevelApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupViewDto>>> GetWorkGroupsAsync(string profitCentre)
        {
            var response = await _http.GetAsync<List<WorkGroupViewDto>>(string.Format(FpsApiEndpoints.GetBudgetWorkGroups, profitCentre));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(response);
            return ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<BidViewDto>>> GetBidViewAsync(string workgroup)
        {
            var response = await _http.GetAsync<List<BidViewRes>>(string.Format(FpsApiEndpoints.GetBidView, workgroup));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<BidViewDto>>>(response);
            return ApiResponseDto<List<BidViewDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> GetBidByIdAsync(string workgroupName, string account)
        {
            var response = await _http.GetAsync<BidRes>(string.Format(FpsApiEndpoints.GetBidById, workgroupName, account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> CreateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PostAsync<BidReq, BidRes>(FpsApiEndpoints.CreateBid, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<BidDto>> UpdateBidAsync(BidDto bid)
        {
            var req = _mapper.Map<BidReq>(bid);
            var response = await _http.PutAsync<BidReq, BidRes>(FpsApiEndpoints.UpdateBid, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<BidDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<BidDto>>(response);
            return ApiResponseDto<BidDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteBidAsync(BidDto bid)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteBid, bid.WorkgroupName, bid.Account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string workgroupName, string account)
        {
            var response = await _http.GetAsync<List<PurchaseRes>>(string.Format(FpsApiEndpoints.GetPurchases, workgroupName, account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(response);
            return ApiResponseDto<List<PurchaseDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            var response = await _http.GetAsync<PurchaseRes>(string.Format(FpsApiEndpoints.GetPurchaseById, workgroupName, account, itemDescription));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase)
        {
            var req = _mapper.Map<PurchaseReq>(purchase);
            var response = await _http.PostAsync<PurchaseReq, PurchaseRes>(FpsApiEndpoints.CreatePurchase, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            var req = _mapper.Map<PurchaseReq>(purchase);
            var response = await _http.PutAsync<PurchaseReq, PurchaseRes>(FpsApiEndpoints.UpdatePurchase, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeletePurchase, purchase.WorkgroupName, purchase.Account, purchase.ItemDescription));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            var response = await _http.GetAsync<List<AccountCategoryRes>>(FpsApiEndpoints.GetBudgetAccountCategories);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            var response = await _http.GetAsync<List<ProfitCentreRes>>(FpsApiEndpoints.GetBudgetProfitCentres);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
            return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
