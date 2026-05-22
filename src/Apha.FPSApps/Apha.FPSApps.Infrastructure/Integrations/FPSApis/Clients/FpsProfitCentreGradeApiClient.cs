using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;  // IFpsProfitCentreGradeApiClient
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsProfitCentreGradeApiClient : IFpsProfitCentreGradeApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProfitCentreGradeApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre)
        {
            var baseUrl = string.Format(FpsApiEndpoints.GetPcGrades, Uri.EscapeDataString(profitCentre));
            var url = QueryStringHelper.AddQueryString(baseUrl, query);
            var response = await _http.GetAsync<List<ProfitCentreGradeRes>>(url);
            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
            }
            else
            {
                var responseDto = _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(response);
                return ApiResponseDto<List<ProfitCentreGradeDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
        }
    }
}
