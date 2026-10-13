using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITestsRequiredByRcRepository
    {
        Task<List<TestsRequiredByRcView>> GetTestsRequiredByRcByYearAsync(string? profitCentre, int fpsYear);
    }
}
