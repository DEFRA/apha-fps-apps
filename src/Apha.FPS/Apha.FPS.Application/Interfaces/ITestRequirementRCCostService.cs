using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for project-specific component charges (TestRequirementRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrequirementrccost: TestCode + Buyer + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRequirementRCCostService
    {
        Task<IEnumerable<TestRequirementRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<TestRequirementRCCostDto?> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        Task<TestRequirementRCCostDto> CreateAsync(TestRequirementRCCostDto dto);

        Task<TestRequirementRCCostDto> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto);

        Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
