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
    public class PimsRadTrackProgApiClient : IPimsRadTrackProgApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsRadTrackProgApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync()
        {
            var response = await _http.GetAsync<List<RadTrackProgRes>>(PimsApiEndpoints.GetAllRadTrackProgs);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<RadTrackProgDto>>>(response);
            return ApiResponseDto<List<RadTrackProgDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

       
        public async Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedRadTrackProgs, query);
            var response = await _http.GetAsync<List<RadTrackProgRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<RadTrackProgDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<RadTrackProgDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<RadTrackProgDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<RadTrackProgDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByProgramAsync(string program)
        {
            var url = string.Format(PimsApiEndpoints.GetRadTrackProgByProgram, Uri.EscapeDataString(program));
            var response = await _http.GetAsync<RadTrackProgRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
            return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto)
        {
            var request = _mapper.Map<RadTrackProgReq>(dto);
            var response = await _http.PostAsync<RadTrackProgReq, RadTrackProgRes>(PimsApiEndpoints.CreateRadTrackProg, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
            return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto)
        {
            var request = _mapper.Map<RadTrackProgReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateRadTrackProg, Uri.EscapeDataString(program));
            var response = await _http.PutAsync<RadTrackProgReq, RadTrackProgRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<RadTrackProgDto>>(response);
            return ApiResponseDto<RadTrackProgDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        
        public async Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program)
        {
            var url = string.Format(PimsApiEndpoints.DeleteRadTrackProg, Uri.EscapeDataString(program));
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        // GET /api/v1/radtrackprog/programs — distinct non-null Programme names for dropdown binding
        public async Task<ApiResponseDto<List<string>>> GetAllProgramNamesAsync()
        {
            var response = await _http.GetAsync<List<string>>(PimsApiEndpoints.GetAllRadTrackProgNames);
            if (response.Success)
                return ApiResponseDto<List<string>>.SuccessResponse(response.Data ?? []);

            return ApiResponseDto<List<string>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }
    }
}
