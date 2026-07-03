using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for component charges per profit centre (TestRCCost) CRUD.
    /// Scoped to the fsubTestRCPrice component charges tab use case.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// No infrastructure-specific code — Core layer only.
    /// </summary>
    public interface ITestRCCostRepository
    {
        Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        Task<bool> ExistsAsync(string testCode, string profitCentre, int fpsYear);

        Task<TestRCCost> AddAsync(TestRCCost testRCCost);

        Task<TestRCCost> UpdateAsync(TestRCCost testRCCost);

        Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
