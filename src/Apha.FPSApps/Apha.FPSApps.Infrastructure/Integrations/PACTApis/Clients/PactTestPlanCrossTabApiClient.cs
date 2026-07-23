using Apha.Common.Constants;
using Apha.Common.Contracts.PACT;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactTestPlanCrossTabApiClient : IPactTestPlanCrossTabApiClient
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;

        public PactTestPlanCrossTabApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<TestPlanCrossTabDto>> GetPagedTestPlanCrossTabAsync(
            QueryParameters<string> query)
        {
            var url = QueryStringHelper.AddQueryString(PactApiEndpoints.GetPagedTestPlanCrossTab, query);
            var response = await _http.GetAsync<TestPlanCrossTabRes>(url);

            if (response.Success && response.Data is not null)
            {
                return ApiResponseDto<TestPlanCrossTabDto>.SuccessResponse(new TestPlanCrossTabDto
                {
                    Columns = response.Data.Columns,
                    Rows = response.Data.Rows,
                    TotalCount = response.Data.TotalCount,
                    Page = response.Data.Page,
                    PageSize = response.Data.PageSize
                });
            }

            var dto = _mapper.Map<ApiResponseDto<TestPlanCrossTabDto>>(response);
            return ApiResponseDto<TestPlanCrossTabDto>.FailureResponse(dto.Errors, dto.Meta);
        }
    }
}