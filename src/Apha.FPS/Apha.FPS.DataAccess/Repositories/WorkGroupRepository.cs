using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for Workgroup lookup data access.
    /// </summary>
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<string>> GetAllWorkGroupNamesAsync()
        {
            return await _dbContext.Workgroups
                .AsNoTracking()
                .Select(e => e.WorkGroupName)
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<WorkGroupView>> GetWorkGroupsByProfitCentreAsync(string profitCentre)
        {
            return await _dbContext.WorkGroupViews
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre
                         && w.UserEmail != null && w.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }
    }
}
