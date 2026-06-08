using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsPurchasesApiClient : IFpsPurchasesApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsPurchasesApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http   = http   ?? throw new ArgumentNullException(nameof(http));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResponseDto<List<PurchaseDto>>> GetPurchasesAsync(string WorkGroupName, string account)
        {
            var response = await _http.GetAsync<List<PurchaseRes>>(string.Format(FpsApiEndpoints.GetGenericPurchases, WorkGroupName, account));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<PurchaseDto>>>(response);
            return ApiResponseDto<List<PurchaseDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription)
        {
            var response = await _http.GetAsync<PurchaseRes>(string.Format(FpsApiEndpoints.GetPurchaseByKeys, WorkGroupName, account, itemDescription));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> CreatePurchaseAsync(PurchaseDto purchase)
        {
            var req = _mapper.Map<PurchaseReq>(purchase);
            var response = await _http.PostAsync<PurchaseReq, PurchaseRes>(FpsApiEndpoints.CreateGenericPurchase, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PurchaseDto>> UpdatePurchaseAsync(PurchaseDto purchase)
        {
            var req = _mapper.Map<PurchaseReq>(purchase);
            var response = await _http.PutAsync<PurchaseReq, PurchaseRes>(FpsApiEndpoints.UpdateGenericPurchase, req);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<PurchaseDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<PurchaseDto>>(response);
            return ApiResponseDto<PurchaseDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeletePurchaseAsync(PurchaseDto purchase)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteGenericPurchase, purchase.WorkGroupName, purchase.Account, purchase.ItemDescription));

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
