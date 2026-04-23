using Apha.Costbook.DataAccess;

namespace Apha.Costbook.Core.Interfaces;

public interface IStaffRequirementRepository
{
    Task<IEnumerable<StaffRequirement>> GetByProjectYearAsync(string project, int year);
    Task<StaffRequirement> AddAsync(StaffRequirement staffRequirement);
    Task<StaffRequirement> UpdateAsync(StaffRequirement staffRequirement);
    Task<bool> DeleteAsync(int srIdentity);
}
