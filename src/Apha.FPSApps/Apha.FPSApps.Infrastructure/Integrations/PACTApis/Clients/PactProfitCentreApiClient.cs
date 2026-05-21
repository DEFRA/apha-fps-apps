using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactProfitCentreApiClient : IPactProfitCentreApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactProfitCentreApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>> GetAllProfitCentresAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ProfitCentreSettingsRes>>(PactApiEndpoints.GetAllProfitCentres);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>>(response);
            return ApiResponseDto<IEnumerable<ProfitCentreSettingsDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProfitCentreSettingsDto>> GetProfitCentreSettingsAsync(string profitCentre)
        {
            var url = string.Format(PactApiEndpoints.GetProfitCentreSettings, Uri.EscapeDataString(profitCentre));
            var response = await _http.GetAsync<ProfitCentreSettingsRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProfitCentreSettingsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProfitCentreSettingsDto>>(response);
            return ApiResponseDto<ProfitCentreSettingsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre    = profitCentre,
                Timesheet       = timesheet,
                Outputsheet     = outputsheet,
                TimesheetLayout = timesheetLayout
            };
            var response = await _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                PactApiEndpoints.PatchProfitCentreSettings, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
        }
    }
}
