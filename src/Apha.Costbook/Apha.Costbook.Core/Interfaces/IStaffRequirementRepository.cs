using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces;

public interface IStaffRequirementRepository
{
    Task<PagedData<StaffRequirementDetailView>> GetByProjectYearAsync(string project, int year, PaginationParameters<string> query);
    Task<StaffRequirement> AddAsync(StaffRequirement staffRequirement);
    Task<StaffRequirement> UpdateAsync(StaffRequirement staffRequirement);
    Task<bool> DeleteAsync(int srIdentity);
}
