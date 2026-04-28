using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        public MonthlyTimeRepository(FpsDbContext context) : base(context) { }

        public async Task<bool> HasDependentRowsAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(m => m.WorkGroup == workGroup && m.TimeCode == timeCode && m.ParentProject == parentProject);
        }
    }
}
