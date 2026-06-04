using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTestSupplierApiClient
    {
        Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedTestSupplierAsync(
            QueryParameters<string> query, string testCode, bool showRejected);
    }
}
