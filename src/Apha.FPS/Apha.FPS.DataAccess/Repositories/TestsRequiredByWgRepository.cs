using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestsRequiredByWgRepository : BaseRepository, ITestsRequiredByWgRepository
    {
        public TestsRequiredByWgRepository(FpsDbContext context) : base(context)
        {
        }

        public Task<List<TestsRequiredByWgView>> GetTestsRequiredByWgByYearAsync(string? profitCentre, int fpsYear)
        {
            return TestsRequiredByYearQuery.GetByWorkGroupAsync(_context, profitCentre, fpsYear);
        }
    }
}
