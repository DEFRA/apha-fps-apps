using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITestsRequiredByWgService
    {
        Task<List<TestsRequiredByWgDto>> GetTestsRequiredByWgByYearAsync(string? profitCentre, int fpsYear);
    }
}
