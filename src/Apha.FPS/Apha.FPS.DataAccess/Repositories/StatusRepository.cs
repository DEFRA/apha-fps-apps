using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class StatusRepository : IStatusRepository
    {
        private readonly FpsDbContext _dbContext;
        public StatusRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<Status>> GetAllStatusesAsync()
        {
            return await _dbContext.Statuses
                .AsNoTracking()
                .OrderBy(s => s.StatusValue)
                .ToListAsync();
        }
    }
}
