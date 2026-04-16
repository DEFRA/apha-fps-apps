using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactTimeCodeValidApiClient : IPactTimeCodeValidApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;        

        public PactTimeCodeValidApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetByJobCodeAsync(string jobCode, string parentProject)
        {
            var response = await _http.GetAsync<List<TimeCodeValidRes>>(
                string.Format(PactApiEndpoints.GetTimeCodesByJobCode, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject)));
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
            return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject)
        {
            string baseUrl;
            if (!string.IsNullOrWhiteSpace(jobCode) && !string.IsNullOrWhiteSpace(parentProject))
                baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByJobCodeAndProject, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject));
            else if (!string.IsNullOrWhiteSpace(jobCode))
                baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByJobCode, Uri.EscapeDataString(jobCode));
            else if (!string.IsNullOrWhiteSpace(parentProject))
                baseUrl = string.Format(PactApiEndpoints.GetPagedTimeCodesByProject, Uri.EscapeDataString(parentProject));
            else
                baseUrl = PactApiEndpoints.GetPagedTimeCodes;

            var url = QueryStringHelper.AddQueryString(baseUrl, query);

            var response = await _http.GetAsync<List<TimeCodeValidRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
            return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TimeCodeValidDto>> CreateTimeCodeValidAsync(TimeCodeValidDto item)
        {
            var request = _mapper.Map<TimeCodeValidReq>(item);
            var response = await _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(PactApiEndpoints.CreateTimeCodeValid, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);
            return ApiResponseDto<TimeCodeValidDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TimeCodeValidDto>> UpdateTimeCodeValidAsync(TimeCodeValidDto item)
        {
            var request = _mapper.Map<TimeCodeValidReq>(item);
            var response = await _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(PactApiEndpoints.UpdateTimeCodeValid, request);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(response);
            return ApiResponseDto<TimeCodeValidDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.DeleteTimeCodeValid, Uri.EscapeDataString(workGroup), Uri.EscapeDataString(timeCode), Uri.EscapeDataString(parentProject));
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAllByJobCodeAsync(string jobCode, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.DeleteTimeCodesByJobCode, Uri.EscapeDataString(jobCode), Uri.EscapeDataString(parentProject));
            var response = await _http.DeleteAsync<bool?>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject)
        {
            var url = string.Format(PactApiEndpoints.CopyWorkGroup, Uri.EscapeDataString(sourceJobCode), Uri.EscapeDataString(targetJobCode), Uri.EscapeDataString(parentProject));
            var response = await _http.PostAsync<object, List<TimeCodeValidRes>>(url, new { });
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
            return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteBulkAsync(BulkDeleteTimeCodeRequestDto request)
        {
            var body = new BulkDeleteTimeCodeReq
            {
                ParentProject = request.ParentProject,
                Items = request.Items
                    .Select(i => new TimeCodeKeyItem { WorkGroup = i.WorkGroup, TimeCode = i.TimeCode })
                    .ToList()
            };
            var response = await _http.PostAsync<BulkDeleteTimeCodeReq, bool>(PactApiEndpoints.DeleteBulkTimeCodes, body);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<TimeCodeValidDto>>> CopySelectedWorkGroupsAsync(BulkCopyWorkGroupRequestDto request)
        {
            var body = new BulkCopyWorkGroupReq
            {
                ParentProject = request.ParentProject,
                SourceJobCode = request.SourceJobCode,
                TargetJobCode = request.TargetJobCode,
                WorkGroups = request.WorkGroups
            };
            var response = await _http.PostAsync<BulkCopyWorkGroupReq, List<TimeCodeValidRes>>(PactApiEndpoints.CopySelectedWorkGroups, body);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(response);
            return ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
