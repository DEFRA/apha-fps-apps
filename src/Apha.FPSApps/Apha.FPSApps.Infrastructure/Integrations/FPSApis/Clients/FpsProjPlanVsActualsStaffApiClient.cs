using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProjPlanVsActualsStaffApiClient : IFpsProjPlanVsActualsStaffApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public FpsProjPlanVsActualsStaffApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<TimeCostCalcsViewDto>>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetTimeCostCalcsByProject, projectCode), query);
            var response = await _http.GetAsync<List<TimeCostCalcsViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<TimeCostCalcsViewDto>>>(response);
            return ApiResponseDto<List<TimeCostCalcsViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<TimeCostCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode)
        {
            var url = string.Format(FpsApiEndpoints.GetTimeCostCalcsTotalsByProject, projectCode);
            var response = await _http.GetAsync<TimeCostCalcsTotalsRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<TimeCostCalcsTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<TimeCostCalcsTotalsDto>>(response);
            return ApiResponseDto<TimeCostCalcsTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId)
        {
            var url = string.Format(FpsApiEndpoints.DeleteTimeCostCalcs, workgroup, jobCode, project, month, staffId);
            var response = await _http.DeleteAsync<bool>(url);
            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(true);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
