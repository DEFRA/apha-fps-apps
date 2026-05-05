using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsResourceCentreGradeApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetResourceCentreGradesAsync(QueryParameters<string> query, string profitCentre);
    }
}
