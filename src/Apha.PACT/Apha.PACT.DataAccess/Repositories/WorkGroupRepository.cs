using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repositories
{
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        public WorkGroupRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }
    }
}
