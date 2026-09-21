using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsReviewItemApiClient : IPimsReviewItemApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsReviewItemApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync()
        {
            var response = await _http.GetAsync<List<ReviewItemRes>>(PimsApiEndpoints.GetAllReviewItems);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ReviewItemDto>>>(response);
            return ApiResponseDto<List<ReviewItemDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedReviewItems, query);
            var response = await _http.GetAsync<List<ReviewItemRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<ReviewItemDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<ReviewItemDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<ReviewItemDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<ReviewItemDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }


        public async Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId)
        {
            var url = string.Format(PimsApiEndpoints.GetReviewItemById, itemId);
            var response = await _http.GetAsync<ReviewItemRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
            return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto)
        {
            var request = _mapper.Map<ReviewItemReq>(dto);
            var response = await _http.PostAsync<ReviewItemReq, ReviewItemRes>(PimsApiEndpoints.CreateReviewItem, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
            return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto)
        {
            var request = _mapper.Map<ReviewItemReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateReviewItem, itemId);
            var response = await _http.PutAsync<ReviewItemReq, ReviewItemRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReviewItemDto>>(response);
            return ApiResponseDto<ReviewItemDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId)
        {
            var url = string.Format(PimsApiEndpoints.DeleteReviewItem, itemId);
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
