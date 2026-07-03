using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for project-specific component charges (TestRequirementRCCost) CRUD.
    /// Scoped to the fsubTestequirementRCPrice project component charges tab use case.
    /// Composite PK on fps.tbltestrequirementrccost: TestCode + Buyer + ProfitCentre + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestRequirementRCCostRepository
    {
        Task<IEnumerable<TestRequirementRCCost>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<TestRequirementRCCost?> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        Task<bool> ExistsAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        Task<TestRequirementRCCost> AddAsync(TestRequirementRCCost testRequirementRCCost);

        Task<TestRequirementRCCost> UpdateAsync(TestRequirementRCCost testRequirementRCCost);

        Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
