using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestsRequiredByRcRepository : BaseRepository, ITestsRequiredByRcRepository
    {
        public TestsRequiredByRcRepository(FpsDbContext context) : base(context)
        {
        }

        public Task<List<TestsRequiredByRcView>> GetTestsRequiredByRcByYearAsync(string? profitCentre, int fpsYear)
        {
            return TestsRequiredByYearQuery.GetByResourceCentreAsync(_context, profitCentre, fpsYear);
        }
    }
}
