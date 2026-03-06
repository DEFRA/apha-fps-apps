using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Interfaces
{
    public interface IStaffJobService
    {
        Task<PaginatedResult<StaffJobViewDto>> GetJobStaffCostAsync(QueryParameters<string> queryFilter);
        Task<List<StaffWorkgroupLookupDto>> GetStaffWorkgroupLookup();
        Task<decimal?> GetStaffChargeRate(string staffId, string jobcode);
        Task<StaffJobDto?> GetByIdAsync(string staffId, string jobCode);
        Task<StaffJobDto> AddAsync(StaffJobDto staffJob);
        Task<StaffJobDto> UpdateAsync(StaffJobDto staffJob);
        Task<bool> DeleteAsync(string staffId, string jobCode);
    }
}
