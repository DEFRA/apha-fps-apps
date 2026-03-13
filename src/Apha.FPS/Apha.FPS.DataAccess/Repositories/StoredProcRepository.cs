using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class StoredProcRepository : IStoredProcRepository
    {
        private readonly FpsDbContext _dbContext;
        public StoredProcRepository(FpsDbContext context)
        {
            _dbContext = context;
        }

        public async Task<IEnumerable<CostCentreWorkgroup>> GetAllCostCentreWorkgroupAsync()
        {
            return await _dbContext.Workgroups
                 .Where(wg => wg.CostCentre != null)
                 .GroupBy(wg => new { wg.CostCentre, wg.ProfitCentre })
                 .Select(g => new CostCentreWorkgroup
                 {
                     CostCentre = g.Key.CostCentre,
                     ProfitCentre = g.Key.ProfitCentre,
                     WGs = string.Join(", ", g.Select(x => x.WorkgroupName))
                 })
                 .ToListAsync();
        }

    }
}
