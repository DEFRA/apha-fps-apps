using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class JobCodeRepository : IJobCodeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;
        public JobCodeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<JobCode>> GetAllJobCodesAsync()
        {
            return await _dbContext.JobCodes
                .AsNoTracking()
                .OrderBy(j => j.JobCodeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ZtJobCodeLookup>> GetZtJobCodesAsync()
        {
            var baseQuery = (from jc in _dbContext.ProjectViews
                             where jc.Program != null && jc.Program.ToLower() == "zt_prog"
                             && jc.UserEmail != null && jc.UserEmail.ToLower() == _requestContext.UserEmailId
                             select new ZtJobCodeLookup
                             {
                                 JobCode = jc.ParentProject,
                                 Description = jc.ProjectTitle
                             }).Distinct().AsQueryable();

            return await baseQuery.AsNoTracking().ToListAsync();
        }
    }
}