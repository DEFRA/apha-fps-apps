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
    public class FpsProjectStaffPlanApiClient : IFpsProjectStaffPlanApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsProjectStaffPlanApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<ProjectStaffPlanViewDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(FpsApiEndpoints.GetPagedProjectStaffPlan, query);

            // ApiResponseActionFilter on the FPS API already unwraps PaginationRes<T>:
            // $.data  → List<ProjectStaffPlanViewRes>  (the items)
            // $.pagination → Pagination               (page metadata)
            var response = await _http.GetAsync<List<ProjectStaffPlanViewRes>>(url);

            if (response.Success)
                return _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(response);

            var dto = _mapper.Map<ApiResponseDto<List<ProjectStaffPlanViewDto>>>(response);
            return ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}
