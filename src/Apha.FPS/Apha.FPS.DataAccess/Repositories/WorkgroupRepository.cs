using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for Workgroup lookup data access.
    /// </summary>
    public class WorkgroupRepository : BaseRepository, IWorkgroupRepository
    {
        private readonly FpsDbContext _dbContext;

        public WorkgroupRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<string>> GetAllWorkgroupNamesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Workgroups
                .AsNoTracking()
                .Select(e => e.WorkgroupName)
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);
        }
    }
}
