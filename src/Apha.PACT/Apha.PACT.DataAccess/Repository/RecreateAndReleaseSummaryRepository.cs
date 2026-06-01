using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class RecreateAndReleaseSummaryRepository : IRecreateSummariesLogRepository
    {
        private readonly FpsDbContext _context;

        public RecreateAndReleaseSummaryRepository(FpsDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecreateSummariesLog>> GetAllLogsAsync()
        {
            return await _context.RecreateSummariesLogs
                .Include(r => r.User)
                .OrderByDescending(r => r.DateDone)
                .ToListAsync();
        }
    }
}
