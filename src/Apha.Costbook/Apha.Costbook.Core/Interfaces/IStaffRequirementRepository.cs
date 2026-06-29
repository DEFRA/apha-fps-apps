using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces;

public interface IStaffRequirementRepository
{
    Task<PagedData<StaffRequirementDetailView>> GetStaffRequirementsByProjectYearAsync(string project, int year, PaginationParameters<string> query);
    Task<StaffRequirement> AddStaffRequirementAsync(StaffRequirement staffRequirement);
    Task<StaffRequirement> UpdateStaffRequirementAsync(StaffRequirement staffRequirement);
    Task<bool> DeleteStaffRequirementAsync(int srIdentity);
    Task<IEnumerable<PayRateLookup>> GetPayRatesAsync(string projectId, int year, bool isDefra);
}
