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
    public class PactWorkGroupApiClient : IPactWorkGroupApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactWorkGroupApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetAllWorkGroupsAsync()
        {
            var response = await _http.GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetAllWorkGroups);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(response);
            return ApiResponseDto<List<WorkGroupDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<WorkGroupTimeCodeDto>>> GetPagedWorkGroupTimeCodesAsync(
            QueryParameters<string> query, string? workGroup, int? monthNumber)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";
            if (monthNumber.HasValue)
                url += $"&monthNumber={monthNumber.Value}";

            var response = await _http.GetAsync<List<WorkGroupTimeCodeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupTimeCodeDto>>>(response);
            return ApiResponseDto<List<WorkGroupTimeCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>> GetPagedWorkGroupValidTimeCodesAsync(
            QueryParameters<string> query, string workGroup)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedWorkGroupValidTimeCodes, query);
            if (!string.IsNullOrWhiteSpace(workGroup))
                url += $"&workGroup={Uri.EscapeDataString(workGroup)}";

            var response = await _http.GetAsync<List<WorkGroupValidTimeCodeRes>>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupValidTimeCodeDto>>>(response);
            return ApiResponseDto<List<WorkGroupValidTimeCodeDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
