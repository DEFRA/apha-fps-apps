using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsWgGradeApiClient
    {
        Task<ApiResponseDto<List<WorkgroupGradeDto>>> GetWgGradesAsync(QueryParameters<string> query, string pcGrade);
        Task<ApiResponseDto<bool>> DeleteWgGradeAsync(string wgGrade);
    }
}
