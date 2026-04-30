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
    public class FpsMonthlyOutputCalcsApiClient : IFpsMonthlyOutputCalcsApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsMonthlyOutputCalcsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>> GetByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetMonthlyOutputCalcsByProject, projectCode), query);
            var response = await _http.GetAsync<List<MonthlyOutputCalcsViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyOutputCalcsViewDto>>>(response);
            return ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<MonthlyOutputCalcsTotalsDto>> GetTotalActualByProjectAsync(string projectCode)
        {
            var url = string.Format(FpsApiEndpoints.GetMonthlyOutputCalcsTotalsByProject, projectCode);
            var response = await _http.GetAsync<MonthlyOutputCalcsTotalsRes>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<MonthlyOutputCalcsTotalsDto>>(response);

            var dto = _mapper.Map<ApiResponseDto<MonthlyOutputCalcsTotalsDto>>(response);
            return ApiResponseDto<MonthlyOutputCalcsTotalsDto>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteMonthlyOutputCalcsAsync(string buyer, string testCode, double month, string workGroup)
        {
            var req = new MonthlyOutputCalcsReq
            {
                Buyer     = buyer,
                TestCode  = testCode,
                Month     = month,
                WorkGroup = workGroup
            };
            var response = await _http.DeleteAsync<MonthlyOutputCalcsReq, bool>(FpsApiEndpoints.DeleteMonthlyOutputCalcs, req);
            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(true);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}