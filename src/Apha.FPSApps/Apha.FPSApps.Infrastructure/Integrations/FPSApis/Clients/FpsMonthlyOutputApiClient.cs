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
    public class FpsMonthlyOutputApiClient : IFpsMonthlyOutputApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsMonthlyOutputApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<MonthlyOutputDto>>> GetByProjectAsync(QueryParameters<string> query, string projectCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetMonthlyOutputByProject, projectCode), query);
            var response = await _http.GetAsync<List<MonthlyOutputRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<MonthlyOutputDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<MonthlyOutputDto>>>(response);
            return ApiResponseDto<List<MonthlyOutputDto>>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<double>> GetTotalActualByProjectAsync(string projectCode)
        {
            var url = string.Format(FpsApiEndpoints.GetMonthlyOutputTotalsByProject, projectCode);
            var response = await _http.GetAsync<double>(url);
            if (response.Success)
                return _mapper.Map<ApiResponseDto<double>>(response);

            var dto = _mapper.Map<ApiResponseDto<double>>(response);
            return ApiResponseDto<double>.FailureResponse(dto.Errors, dto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteMonthlyOutputAsync(string buyer, string testCode, double month, string workGroup)
        {
            var req = new MonthlyOutputReq
            {
                Buyer     = buyer,
                TestCode  = testCode,
                Month     = month,
                WorkGroup = workGroup
            };
            var response = await _http.DeleteAsync<MonthlyOutputReq, bool>(FpsApiEndpoints.DeleteMonthlyOutput, req);
            if (response.Success)
                return ApiResponseDto<bool>.SuccessResponse(true);

            var dto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}