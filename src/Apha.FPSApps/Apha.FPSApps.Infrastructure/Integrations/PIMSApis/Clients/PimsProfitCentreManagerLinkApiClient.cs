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
    public class PimsProfitCentreManagerLinkApiClient : IPimsProfitCentreManagerLinkApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsProfitCentreManagerLinkApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync()
        {
            var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(PimsApiEndpoints.GetAllProfitCentreManagerLinks);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync()
        {
            var response = await _http.GetAsync<List<ProfitCentreLookupRes>>(PimsApiEndpoints.GetProfitCentres);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProfitCentreLookupDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreLookupDto>>>(response);
            return ApiResponseDto<List<ProfitCentreLookupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByProfitCentreAsync(string profitCentre)
        {
            var url = string.Format(PimsApiEndpoints.GetProfitCentreManagerLinksByProfitCentre, Uri.EscapeDataString(profitCentre));
            var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByManagerAsync(string manager)
        {
            var url = string.Format(PimsApiEndpoints.GetProfitCentreManagerLinksByManager, Uri.EscapeDataString(manager));
            var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreManagerLinkDto>>>(response);
            return ApiResponseDto<List<ProfitCentreManagerLinkDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedByManagerAsync(QueryParameters<string> query, string manager)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedProfitCentreManagerLinks, query);
            url += $"&manager={Uri.EscapeDataString(manager)}";

            var response = await _http.GetAsync<List<ProfitCentreManagerLinkRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<ProfitCentreManagerLinkDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<ProfitCentreManagerLinkDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }


        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager)
        {
            var url = string.Format(PimsApiEndpoints.GetProfitCentreManagerLinkById, Uri.EscapeDataString(profitCentre), Uri.EscapeDataString(manager));
            var response = await _http.GetAsync<ProfitCentreManagerLinkRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
            return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto)
        {
            var request = _mapper.Map<ProfitCentreManagerLinkReq>(dto);
            var response = await _http.PostAsync<ProfitCentreManagerLinkReq, ProfitCentreManagerLinkRes>(PimsApiEndpoints.CreateProfitCentreManagerLink, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ProfitCentreManagerLinkDto>>(response);
            return ApiResponseDto<ProfitCentreManagerLinkDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager)
        {
            var url = string.Format(PimsApiEndpoints.DeleteProfitCentreManagerLink, Uri.EscapeDataString(profitCentre), Uri.EscapeDataString(manager));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
