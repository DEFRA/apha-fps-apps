using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectGroupRepository : IProjectGroupRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProjectGroupRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<ProjectGroup>> GetAllProjectGroupsAsync()
        {
            return await _dbContext.ProjectGroups
                .AsNoTracking()                
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectGroup>> GetAllProjectGroupsByUserAsync()
        {
            return await _dbContext.ProjectGroupViews
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Select(p => new ProjectGroup
                {
                    ProjectGroupName = p.ProjectGroupName
                })
                .Distinct()
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
