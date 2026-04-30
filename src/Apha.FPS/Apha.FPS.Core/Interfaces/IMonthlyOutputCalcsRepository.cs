using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMonthlyOutputCalcsRepository
    {
        Task<PagedData<MonthlyOutputCalcsView>> GetByProjectAsync(PaginationParameters<string> query, string projectCode);
        Task<(double TotalVolume, double TotalCost)> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup);
    }
}
