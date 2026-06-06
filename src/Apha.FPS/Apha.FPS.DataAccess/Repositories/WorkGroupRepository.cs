using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<WorkGroupView>> GetWorkGroupsAsync(string profitCentre)
        {
            var results = await _context.WorkGroupViews
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre
                    && w.FpsYear == _requestContext.FpsYear
                    && w.UserEmail != null && w.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(w => w.WorkgroupName)
                .ToListAsync();

            return results.DistinctBy(w => w.WorkgroupName).ToList();
        }
    }
}
