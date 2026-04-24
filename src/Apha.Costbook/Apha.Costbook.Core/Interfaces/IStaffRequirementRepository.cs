using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface IStaffRequirementRepository
{
    Task<IEnumerable<StaffRequirementDetailView>> GetByProjectYearAsync(string project, int year);
    Task<StaffRequirement> AddAsync(StaffRequirement staffRequirement);
    Task<StaffRequirement> UpdateAsync(StaffRequirement staffRequirement);
    Task<bool> DeleteAsync(int srIdentity);
}
