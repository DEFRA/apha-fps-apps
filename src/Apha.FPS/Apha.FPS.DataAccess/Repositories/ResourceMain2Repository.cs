using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for the Stage 2 Check Resource Allocation
    /// (frmResourceMain2) read-only grids.
    /// </summary>
    public class ResourceMain2Repository : IResourceMain2Repository
    {
        private readonly FpsDbContext _dbContext;

        public ResourceMain2Repository(FpsDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<ResourceStaffAllocationView>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroupGrade);

            return await _dbContext.ResourceStaffAllocationViews
                .AsNoTracking()
                .Where(r => r.WorkGroupGrade == workGroupGrade)
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<List<ResourceStaffJobView>> GetStaffJobsByStaffIdAsync(int staffId)
        {
            return await _dbContext.ResourceStaffJobViews
                .AsNoTracking()
                .Where(r => r.StaffId == staffId)
                .OrderBy(r => r.Project)
                .ToListAsync();
        }
    }
}
