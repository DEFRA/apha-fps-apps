using Apha.Costbook.Core.Entities;

namespace Apha.Costbook.Core.Interfaces;

public interface ITestRequirementRepository
{
    Task<IEnumerable<TestRequirementDetailView>> GetTestRequirementsByProjectYearAsync(string project, int year);
    Task<TestRequirement> AddTestRequirementAsync(TestRequirement testRequirement);
    Task<TestRequirement> UpdateTestRequirementAsync(TestRequirement testRequirement);
    Task<bool> DeleteTestRequirementAsync(string project, int year, string testCode);
    Task<IEnumerable<TestCodeLookup>> GetTestCodeLookupsAsync(bool isDefra);
}

public record TestCodeLookup(string ItemCode, string? ItemDescription, decimal? UnitPrice);
