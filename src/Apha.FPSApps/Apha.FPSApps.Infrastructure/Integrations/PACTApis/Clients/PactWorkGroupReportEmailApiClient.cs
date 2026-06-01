using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactWorkGroupReportEmailApiClient : IPactWorkGroupReportEmailApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactWorkGroupReportEmailApiClient(IPactHttpExecutor http, IMapper mapper)
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
    }
}
