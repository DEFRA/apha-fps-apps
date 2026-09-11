using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using MapsterMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsRiskApiClient : IPimsRiskApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsRiskApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        // GET /api/v1/risk-ratings
        public async Task<ApiResponseDto<List<RiskDto>>> GetAllRiskRatingsAsync()
        {
            var response = await _http.GetAsync<List<RiskRes>>(PimsApiEndpoints.GetAllRiskRatings);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<RiskDto>>>(response);
            return ApiResponseDto<List<RiskDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // GET /api/v1/risk-ratings/paged
        public async Task<ApiResponseDto<PaginatedResult<RiskDto>>> GetPagedRiskRatingsAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedRiskRatings, query);
            var response = await _http.GetAsync<List<RiskRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<RiskDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<RiskDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<RiskDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<RiskDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }

        // GET /api/v1/risk-ratings/{riskid:int}
        public async Task<ApiResponseDto<RiskDto>> GetRiskRatingByIdAsync(int riskId)
        {
            var url = string.Format(PimsApiEndpoints.GetRiskRatingById, riskId);
            var response = await _http.GetAsync<RiskRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RiskDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RiskDto>>(response);
            return ApiResponseDto<RiskDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // POST /api/v1/risk-ratings
        public async Task<ApiResponseDto<RiskDto>> CreateRiskRatingAsync(RiskDto dto)
        {
            var request = _mapper.Map<RiskReq>(dto);
            var response = await _http.PostAsync<RiskReq, RiskRes>(PimsApiEndpoints.CreateRiskRating, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RiskDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RiskDto>>(response);
            return ApiResponseDto<RiskDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // PUT /api/v1/risk-ratings/{riskid:int}
        public async Task<ApiResponseDto<RiskDto>> UpdateRiskRatingAsync(int riskId, RiskDto dto)
        {
            var request = _mapper.Map<RiskReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateRiskRating, riskId);
            var response = await _http.PutAsync<RiskReq, RiskRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RiskDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RiskDto>>(response);
            return ApiResponseDto<RiskDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // DELETE /api/v1/risk-ratings/{riskid:int}
        public async Task<ApiResponseDto<bool>> DeleteRiskRatingAsync(int riskId)
        {
            var url = string.Format(PimsApiEndpoints.DeleteRiskRating, riskId);
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
