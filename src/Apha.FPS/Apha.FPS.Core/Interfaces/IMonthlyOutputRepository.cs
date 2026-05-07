using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMonthlyOutputRepository
    {
        Task<PagedData<MonthlyOutput>> GetByProjectAsync(PaginationParameters<string> query, string projectCode);
        Task<double> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteAsync(string buyer, string testCode, double month, string workGroup);
    }
}
