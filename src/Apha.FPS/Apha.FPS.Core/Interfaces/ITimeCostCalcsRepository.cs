using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITimeCostCalcsRepository
    {
        Task<PagedData<TimeCostCalcsView>> GetTimeCostCalcsByProjectAsync(PaginationParameters<string> query, string projectCode);
        Task<(double TotalHours, double TotalCost)> GetTotalActualByProjectAsync(string projectCode);
        Task<bool> DeleteAsync(string workgroup, string jobCode, string project, double month, string staffId);
    }
}
