using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IStaffJobRepository
    {
        IQueryable<StaffJob> Get();        
        Task<PagedData<StaffJobView>> GetJobStaffCostAsync(PaginationParameters<string> query);
        Task<List<StaffWorkgroupLookup>> GetStaffWorkgroupLookup();
        Task<decimal?> GetStaffChargeRate(string staffId, string jobcode);
        Task<StaffJob?> GetByIdAsync(string staffId, string jobCode);
        Task<StaffJob> AddAsync(StaffJob staffJob);
        Task<StaffJob> UpdateAsync(StaffJob staffJob);
        Task<bool> DeleteAsync(string staffId, string jobCode);
    }
}
