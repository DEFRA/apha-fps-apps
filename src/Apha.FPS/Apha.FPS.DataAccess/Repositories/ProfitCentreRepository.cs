using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProfitCentreRepository : BaseRepository, IProfitCentreRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProfitCentreRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<List<ProfitCentreView>> GetProfitCentresAsync()
        {
            return await _dbContext.ProfitCentreViews
                .AsNoTracking()
                .Where(x => x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(x => x.ProfitCentreId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProfitCentre>> GetAllProfitCentresAsync()
        {
            return await _context.ProfitCentres
                .AsNoTracking()
                .OrderBy(p => p.ProfitCentreId)
                .ToListAsync();
        }

        public async Task<ProfitCentre?> GetProfitCentreByIdAsync(string profitCentre)
        {
            return await _context.ProfitCentres
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProfitCentreId == profitCentre);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            var entities = await _context.ProfitCentres
                .Where(p => p.ProfitCentreId == profitCentre)
                .ToListAsync();

            foreach (var p in entities)
            {
                p.Timesheet = timesheet;
                p.OutputSheet = outputsheet;
                p.TimesheetLayout = timesheetlayout;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
