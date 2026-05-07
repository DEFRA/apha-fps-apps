using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMonthlyOutputService
    {
        Task<PaginatedResult<MonthlyOutputDto>> GetByProjectAsync(QueryParameters<string> query, string projectCode);
        Task<double> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup);
    }
}
