using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITimeCostCalcsService
    {
        Task<PaginatedResult<TimeCostCalcsViewDto>> GetTimeCostCalcsByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<TimeCostCalcsTotalsDto> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteTimeCostCalcsAsync(string workgroup, string jobCode, string project, double month, string staffId);
    }
}
