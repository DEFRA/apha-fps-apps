using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;


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
                .Where(m => m.Project == project);

            query = ApplyFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

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

        private static IQueryable<Milestone> ApplyFilter(IQueryable<Milestone> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Number", out var number) && number != null)
            {
                string val = number.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Number, $"%{val}%"));
            }

            return query;
        }

        private static IQueryable<Milestone> ApplySorting(IQueryable<Milestone> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy) || sortBy.ToLower() == "number")
                return ApplyOrder(query, m => m.Number, descending);

            return query.OrderBy(m => m.Number);
        }

        private static IQueryable<Milestone> ApplyOrder<T>(
            IQueryable<Milestone> query,
            Expression<Func<Milestone, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        public async Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired)
        {
            int rows = await _dbContext.ProjectRadTrackData
                .Where(p => p.Parentproject == parentproject)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Formrequired, formRequired));
            return rows > 0;
        }

        public async Task<PagedData<LogMilestone>> GetLogMilestonesAsync(PaginationParameters<string> parameters,string? project,string? numberPart1,string? numberPart2)
        {
            
            string numberPattern;
            if (string.IsNullOrWhiteSpace(numberPart1) && string.IsNullOrWhiteSpace(numberPart2))
            {
                numberPattern = string.Empty; 
            }
            else
            {
                string left = string.IsNullOrWhiteSpace(numberPart1) ? "%" : numberPart1;
                string right = string.IsNullOrWhiteSpace(numberPart2) ? "%" : numberPart2;
                numberPattern = $"{left}/{right}";
            }

            IQueryable<LogMilestone> logQuery = _dbContext.LogMilestones.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(project))
                logQuery = logQuery.Where(l => l.Project == project);

            if (!string.IsNullOrWhiteSpace(numberPattern))
                logQuery = logQuery.Where(l => EF.Functions.Like(l.Number!, numberPattern));

           
            var joined = from l in logQuery
                         join pm in _dbContext.ProjectManagers.AsNoTracking()
                             on l.ChangedBy equals pm.Mnumber into pmGroup
                         from pm in pmGroup.DefaultIfEmpty()
                         orderby l.DateChanged descending
                         select new LogMilestone
                         {
                             Id = l.Id,
                             Project = l.Project,
                             Number = l.Number,
                             Description = l.Description,
                             DateDue = l.DateDue,
                             DateCompleted = l.DateCompleted,
                             DateFormReceived = l.DateFormReceived,
                             UnderSdReview = l.UnderSdReview,
                             OnTarget = l.OnTarget,
                             ProjectLeaderComment = l.ProjectLeaderComment,
                             CapsComment = l.CapsComment,
                             IdType = l.IdType,
                             DateChanged = l.DateChanged,
                             ChangedBy = pm != null ? pm.Projectmanager : (l.ChangedBy != null ? "(" + l.ChangedBy + ")" : null),
                             UpdateType = l.UpdateType
                         };

            List<LogMilestone> all = await joined.ToListAsync();
            return ApplyPaging(all, parameters.Page, parameters.PageSize);
        }
    }
}
