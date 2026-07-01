using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactWorkGroupReportApiClient : IPactWorkGroupReportApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactWorkGroupReportApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<WorkGroupReportEmailResultDto>>> SendEmailsAsync(string profitCentre, short monthNumber)
        {
            var request = new WorkGroupReportEmailReq
            {
                ProfitCentre = profitCentre,
                MonthNumber = monthNumber
            };

            var response = await _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                PactApiEndpoints.SendEmails, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(response);
            return ApiResponseDto<List<WorkGroupReportEmailResultDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<WorkGroupCos90SExportResultDto>> ExportCos90sAsync(string profitCentre, short monthNumber, short year, string? pactId)
        {
            var request = new WorkGroupCos90SExportReq
            {
                ProfitCentre = profitCentre,
                MonthNumber = monthNumber,
                Year = year,
                PactId = pactId
            };

            var response = await _http.PostAsync<WorkGroupCos90SExportReq, WorkGroupCos90SExportRes>(
                PactApiEndpoints.ExportCos90s, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<WorkGroupCos90SExportResultDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<WorkGroupCos90SExportResultDto>>(response);
            return ApiResponseDto<WorkGroupCos90SExportResultDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
