using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface ITestRequirementRepository
{
    Task<IEnumerable<TestRequirement>> GetByProjectYearAsync(string project, int year);
    Task<TestRequirement> AddAsync(TestRequirement testRequirement);
    Task<TestRequirement> UpdateAsync(TestRequirement testRequirement);
    Task<bool> DeleteAsync(string project, int year, string testCode);
}
