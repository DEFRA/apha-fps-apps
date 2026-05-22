using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProfitCentreApiClient : IFpsProfitCentreApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProfitCentreApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProfitCentreDto>>> GetProfitCentresAsync()
        {
            var response = await _http.GetAsync<List<ProfitCentreRes>>(FpsApiEndpoints.GetProfitCentres);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreDto>>>(response);
                return ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }

        public async Task<ApiResponseDto<IEnumerable<ProfitCentreDto>>> GetAllProfitCentresAsync()
        {
            var response = await _http.GetAsync<IEnumerable<ProfitCentreRes>>(FpsApiEndpoints.GetAllProfitCentres);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<IEnumerable<ProfitCentreDto>>>(response);
            return ApiResponseDto<IEnumerable<ProfitCentreDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<ProfitCentreDto>> GetProfitCentreByIdAsync(string profitCentre)
        {
            var url = string.Format(FpsApiEndpoints.GetProfitCentreById, Uri.EscapeDataString(profitCentre));
            var response = await _http.GetAsync<ProfitCentreRes>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<ProfitCentreDto>>(response);
            return ApiResponseDto<ProfitCentreDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> UpdateProfitCentreSettingsAsync(
            string profitCentre, int timesheet, int outputsheet, short timesheetLayout)
        {
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre = profitCentre,
                Timesheet = timesheet,
                Outputsheet = outputsheet,
                TimesheetLayout = timesheetLayout
            };
            var response = await _http.PatchAsync<UpdateProfitCentreSettingsReq, bool?>(
                FpsApiEndpoints.PatchProfitCentreSettings, request);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<bool>>(response);

            var failureDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(failureDto.Errors, failureDto.Meta);
        }
    }
}
