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
    public class PimsFrequencyApiClient : IPimsFrequencyApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsFrequencyApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync()
        {
            var response = await _http.GetAsync<List<FrequencyRes>>(PimsApiEndpoints.GetAllFrequencies);
            if (response.Success && response.Data != null)
                return _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<FrequencyDto>>>(response);
            return ApiResponseDto<List<FrequencyDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedFrequencies, query);
            var response = await _http.GetAsync<List<FrequencyRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<FrequencyDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<FrequencyDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<FrequencyDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<FrequencyDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }


        public async Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId)
        {
            var url = string.Format(PimsApiEndpoints.GetFrequencyById, frequencyId);
            var response = await _http.GetAsync<FrequencyRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
            return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto)
        {
            var request = _mapper.Map<FrequencyReq>(dto);
            var response = await _http.PostAsync<FrequencyReq, FrequencyRes>(PimsApiEndpoints.CreateFrequency, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
            return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto)
        {
            var request = _mapper.Map<FrequencyReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateFrequency, frequencyId);
            var response = await _http.PutAsync<FrequencyReq, FrequencyRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<FrequencyDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<FrequencyDto>>(response);
            return ApiResponseDto<FrequencyDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId)
        {
            var url = string.Format(PimsApiEndpoints.DeleteFrequency, frequencyId);
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
