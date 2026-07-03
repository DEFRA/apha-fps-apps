using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for component charges per profit centre (TestRCCost) CRUD operations.
    /// Orchestrates repository calls and enforces FK guard checks from SP/VBA logic.
    /// Composite PK on fps.tbltestrccost: TestCode + ProfitCentre + FpsYear.
    /// </summary>
    public interface ITestRCCostService
    {
        Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        Task<TestRCCostDto> CreateAsync(TestRCCostDto dto);

        Task<TestRCCostDto> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto);

        Task<bool> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
