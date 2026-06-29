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
    public class FpsProjectGroupStaffPlanApiClient : IFpsProjectGroupStaffPlanApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProjectGroupStaffPlanApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProjectGroupStaffPlan, query);

            var response = await _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(response);
            return ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
