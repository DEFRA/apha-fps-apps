using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectGroupRepository : IProjectGroupRepository
    {
        private readonly FpsDbContext _dbContext;
        public ProjectGroupRepository(FpsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ProjectGroup>> GetAllProjectGroupsAsync()
        {
            return await _dbContext.ProjectGroups
                .AsNoTracking()                
                .ToListAsync();
        }
    }
}
