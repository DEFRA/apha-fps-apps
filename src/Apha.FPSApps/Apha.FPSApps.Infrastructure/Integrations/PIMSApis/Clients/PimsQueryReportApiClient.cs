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
    public class PimsQueryReportApiClient : IPimsQueryReportApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsQueryReportApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonitoringReportDataDto>>> GetMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetMonitoringQueryReportData, query);
                url += $"&reportYear={reportYear}";
                url += $"&reportMonth={reportMonth}";
                url += $"&contractFilter={Uri.EscapeDataString(string.IsNullOrWhiteSpace(contractFilter) ? "*" : contractFilter)}";

                if (programFilter != null)
                {
                    foreach (string program in programFilter.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        url += $"&programFilter={Uri.EscapeDataString(program)}";
                    }
                }

                var response = await _http.GetAsync<List<MonitoringReportDataRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<MonitoringReportDataDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<MonitoringReportDataDto>>>(response);
                return ApiResponseDto<List<MonitoringReportDataDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<MonitoringReportDataDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve monitoring query report data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>> GetProgramCustomerMonitoringReportDataAsync(
            QueryParameters<string> query,
            short reportYear,
            short reportMonth,
            IEnumerable<string>? programFilter = null)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetProgramCustomerMonitoringQueryReportData, query);
                url += $"&reportYear={reportYear}";
                url += $"&reportMonth={reportMonth}";

                if (programFilter != null)
                {
                    foreach (string program in programFilter.Where(p => !string.IsNullOrWhiteSpace(p)))
                    {
                        url += $"&programFilter={Uri.EscapeDataString(program)}";
                    }
                }

                var response = await _http.GetAsync<List<ProgramCustomerMonitoringReportDataRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>>(response);

                var dto = _mapper.Map<ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>>(response);
                return ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>.FailureResponse(dto.Errors, dto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<ProgramCustomerMonitoringReportDataDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve program and customer monitoring report data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
