using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;


namespace Apha.PIMS.DataAccess.Repository
{
    public class MilestoneRepository : BaseRepository, IMilestoneRepository
    {
        private readonly PimsDbContext _dbContext;

        public MilestoneRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedData<Milestone>> GetAllMilestonesAsync(PaginationParameters<string> parameters, string project)
        {
            IQueryable<Milestone> query = _dbContext.Milestones
                .AsNoTracking()
                .Where(m => m.Project == project)
                .OrderBy(m => m.Number);

            List<Milestone> all = await query.ToListAsync();
            return ApplyPaging(all, parameters.Page, parameters.PageSize);
        }

        public async Task<Milestone?> GetMilestoneAsync(string project, string number)
            => await _dbContext.Milestones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Project == project && m.Number == number);

        public async Task<Milestone> AddMilestoneAsync(Milestone entity)
        {
            _dbContext.Milestones.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Milestone> UpdateMilestoneAsync(Milestone entity)
        {
            _dbContext.Milestones.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteMilestoneAsync(string project, string number)
        {
            int rows = await _dbContext.Milestones
                .Where(m => m.Project == project && m.Number == number)
                .ExecuteDeleteAsync();
            return rows > 0;
        }
        public async Task<List<MilestoneType>> GetMilestoneTypesAsync(string? milestoneDeliverable = null)
        {
            IQueryable<MilestoneType> query = _dbContext.MilestoneTypes.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(milestoneDeliverable))
            {
                char filter = milestoneDeliverable[0];
                query = query.Where(t => t.MilestoneDeliverable == filter);
            }
            return await query.OrderBy(t => t.Type).ToListAsync();
        }

        public async Task<PagedData<MilestoneFormDates>> GetAllMilestoneFormDatesAsync(PaginationParameters<string> parameters, string parentProject)
        {
            IQueryable<MilestoneFormDates> query = _dbContext.MilestoneFormDates
                .AsNoTracking()
                .Where(f => f.ParentProject == parentProject)
                .OrderByDescending(f => f.Year);

            List<MilestoneFormDates> all = await query.ToListAsync();
            return ApplyPaging(all, parameters.Page, parameters.PageSize);
        }

        public async Task<MilestoneFormDates?> GetMilestoneFormDatesAsync(short year, string parentProject)
            => await _dbContext.MilestoneFormDates
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Year == year && f.ParentProject == parentProject);

        public async Task<MilestoneFormDates> AddMilestoneFormDatesAsync(MilestoneFormDates entity)
        {
            _dbContext.MilestoneFormDates.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<MilestoneFormDates> UpdateMilestoneFormDatesAsync(MilestoneFormDates entity)
        {
            _dbContext.MilestoneFormDates.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject)
        {
            int rows = await _dbContext.MilestoneFormDates
                .Where(f => f.Year == year && f.ParentProject == parentProject)
                .ExecuteDeleteAsync();
            return rows > 0;
        }

        public async Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired)
        {
            int rows = await _dbContext.ProjectRadTrackData
                .Where(p => p.Parentproject == parentproject)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Formrequired, formRequired));
            return rows > 0;
        }
    }
}
