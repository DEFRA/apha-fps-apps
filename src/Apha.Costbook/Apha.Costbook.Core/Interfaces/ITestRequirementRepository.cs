using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces;

public interface ITestRequirementRepository
{
    Task<PagedData<TestRequirementDetailView>> GetTestRequirementsByProjectYearAsync(string project, int year, PaginationParameters<string> query);
    Task<TestRequirement> AddTestRequirementAsync(TestRequirement testRequirement);
    Task<TestRequirement> UpdateTestRequirementAsync(TestRequirement testRequirement);
    Task<bool> DeleteTestRequirementAsync(string project, int year, string testCode);
    Task<IEnumerable<TestCodeLookup>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra);
}


