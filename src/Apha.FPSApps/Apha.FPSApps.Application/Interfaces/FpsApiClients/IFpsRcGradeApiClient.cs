using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsRcGradeApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetRcGradesAsync(QueryParameters<string> query, string profitCentre);
    }
}
