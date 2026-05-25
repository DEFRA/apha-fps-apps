using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsProfitCentreGradeApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreGradeDto>>> GetProfitCentreGradesAsync(QueryParameters<string> query, string profitCentre);
    }
}
