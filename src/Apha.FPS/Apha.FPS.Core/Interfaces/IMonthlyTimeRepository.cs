using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<PagedData<MonthlyTime>> GetByProjectAsync(PaginationParameters<string> query, string parentProject);
        Task<MonthlyTime?> GetByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<MonthlyTime> UpsertAsync(MonthlyTime entity);
        Task<bool> DeleteAsync(string pactStaffId, string timeCode, double month, string parentProject);
    }
}
