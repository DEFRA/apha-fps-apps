using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMonthlyOutputCalcsService
    {
        Task<PaginatedResult<MonthlyOutputCalcsViewDto>> GetByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<MonthlyOutputCalcsTotalsDto> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup);
    }
}
