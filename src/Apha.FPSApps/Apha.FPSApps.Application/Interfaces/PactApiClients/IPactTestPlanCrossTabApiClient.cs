using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
public interface IPactTestPlanCrossTabApiClient
{
    Task<ApiResponseDto<TestPlanCrossTabDto>> GetPagedTestPlanCrossTabAsync(QueryParameters<string> query);
}
}