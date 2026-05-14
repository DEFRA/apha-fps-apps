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
    }
}
