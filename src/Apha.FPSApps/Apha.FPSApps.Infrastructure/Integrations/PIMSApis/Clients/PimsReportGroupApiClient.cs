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
    public class PimsReportGroupApiClient : IPimsReportGroupApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;

        public PimsReportGroupApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }


        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync()
        {
            var response = await _http.GetAsync<List<ReportGroupRes>>(PimsApiEndpoints.GetAllReportGroups);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);
            return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId)
        {
            var url = string.Format(PimsApiEndpoints.GetReportGroupsByReportId, reportId);
            var response = await _http.GetAsync<List<ReportGroupRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<List<ReportGroupDto>>>(response);
            return ApiResponseDto<List<ReportGroupDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null)
        {
            string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetPagedReportGroups, query);
            if (reportId.HasValue)
                url += $"&reportid={reportId.Value}";

            var response = await _http.GetAsync<List<ReportGroupRes>>(url);
            if (response.Success)
            {
                var items = _mapper.Map<List<ReportGroupDto>>(response.Data ?? []);
                var pageNumber = response.Pagination?.PageNumber ?? query.Page;
                var pageSize = response.Pagination?.PageSize ?? query.PageSize;
                var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                var paged = new PaginatedResult<ReportGroupDto>(items, totalRecords, pageNumber, pageSize);
                return ApiResponseDto<PaginatedResult<ReportGroupDto>>.SuccessResponse(paged);
            }

            return ApiResponseDto<PaginatedResult<ReportGroupDto>>.FailureResponse(
                _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                _mapper.Map<ApiMetaDto>(response.Meta));
        }


        public async Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId)
        {
            var url = string.Format(PimsApiEndpoints.GetReportGroupById, groupId);
            var response = await _http.GetAsync<ReportGroupRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
            return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto)
        {
            var request = _mapper.Map<ReportGroupReq>(dto);
            var response = await _http.PostAsync<ReportGroupReq, ReportGroupRes>(PimsApiEndpoints.CreateReportGroup, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
            return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto)
        {
            var request = _mapper.Map<ReportGroupReq>(dto);
            var url = string.Format(PimsApiEndpoints.UpdateReportGroup, groupId);
            var response = await _http.PutAsync<ReportGroupReq, ReportGroupRes>(url, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<ReportGroupDto>>(response);
            return ApiResponseDto<ReportGroupDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }


        public async Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId)
        {
            var url = string.Format(PimsApiEndpoints.DeleteReportGroup, groupId);
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
