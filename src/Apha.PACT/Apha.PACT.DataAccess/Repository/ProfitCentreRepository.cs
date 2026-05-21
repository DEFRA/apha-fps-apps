using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProfitCentreRepository : BaseRepository, IProfitCentreRepository
    {
        public ProfitCentreRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PactProfitCentreView>> GetAllProfitCentresAsync()
        {
            return await _context.PactProfitCentreViews
                .AsNoTracking()
                .OrderBy(p => p.ProfitCentre)
                .ToListAsync();
        }

        public async Task<PactProfitCentreView?> GetProfitCentreSettingsAsync(string profitCentre)
        {
            return await _context.PactProfitCentreViews
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProfitCentre == profitCentre);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            await _context.ProfitCentres
                .Where(p => p.ProfitCentreId == profitCentre)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Timesheet, timesheet)
                    .SetProperty(p => p.Outputsheet, outputsheet)
                    .SetProperty(p => p.Timesheetlayout, timesheetlayout));

            return true;
        }
    }
}
