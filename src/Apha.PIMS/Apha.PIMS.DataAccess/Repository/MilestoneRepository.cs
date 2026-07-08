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

           
            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<Milestone?> GetMilestoneAsync(string project, string number)
            => await _dbContext.Milestones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Project == project && m.Number == number);

        public async Task<Milestone> AddMilestoneAsync(Milestone entity, string? changedBy)
        {
            _dbContext.Milestones.Add(entity);
            await _dbContext.SaveChangesAsync();

            try
            {
                _dbContext.LogMilestones.Add(BuildLogEntry(entity, 'I', changedBy));
                await _dbContext.SaveChangesAsync();
            }
            catch { /* log write failure must not affect the milestone operation */ }

            return entity;
        }

        public async Task<Milestone> UpdateMilestoneAsync(Milestone entity, string? changedBy)
        {
            _dbContext.Milestones.Update(entity);
            await _dbContext.SaveChangesAsync();

            try
            {
                _dbContext.LogMilestones.Add(BuildLogEntry(entity, 'U', changedBy));
                await _dbContext.SaveChangesAsync();
            }
            catch { /* log write failure must not affect the milestone operation */ }

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

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
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
            if (string.IsNullOrEmpty(sortBy) || string.Equals(sortBy, "number", StringComparison.OrdinalIgnoreCase))
                    return ApplyOrder(query, m => m.Number, descending);

            return query.OrderBy(m => m.Number);
        }

        private static IQueryable<Milestone> ApplyOrder<T>(
            IQueryable<Milestone> query,
            Expression<Func<Milestone, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        private static IQueryable<StagingMilestone> ApplyStagingSorting(IQueryable<StagingMilestone> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy) || string.Equals(sortBy, "number", StringComparison.OrdinalIgnoreCase))
                return ApplyStagingOrder(query, m => m.Number, descending);

            return query.OrderBy(m => m.Number);
        }
        private static IQueryable<StagingMilestone> ApplyStagingOrder<T>(
            IQueryable<StagingMilestone> query,
            Expression<Func<StagingMilestone, T>> keySelector,
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
                string left  = string.IsNullOrWhiteSpace(numberPart1) ? "%" : numberPart1;
                string right = string.IsNullOrWhiteSpace(numberPart2) ? "%" : numberPart2;
                numberPattern = $"{left}/{right}";
            }

            IQueryable<LogMilestone> logQuery = _dbContext.LogMilestones.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(project))
                logQuery = logQuery.Where(l => l.Project == project);

            if (!string.IsNullOrWhiteSpace(numberPattern))
                logQuery = logQuery.Where(l => EF.Functions.Like(l.Number!, numberPattern));

            IQueryable<LogMilestone> query =
                from l in logQuery
                join pm in _dbContext.ProjectManagers.AsNoTracking()
                    on l.ChangedBy equals pm.Mnumber into pmGroup
                from pm in pmGroup.DefaultIfEmpty()
                orderby l.DateChanged descending
                select new LogMilestone
                {
                    Id                   = l.Id,
                    Project              = l.Project,
                    Number               = l.Number,
                    Description          = l.Description,
                    DateDue              = l.DateDue,
                    DateCompleted        = l.DateCompleted,
                    DateFormReceived     = l.DateFormReceived,
                    UnderSdReview        = l.UnderSdReview,
                    OnTarget             = l.OnTarget,
                    ProjectLeaderComment = l.ProjectLeaderComment,
                    CapsComment          = l.CapsComment,
                    IdType               = l.IdType,
                    DateChanged          = l.DateChanged,
                    ChangedBy            = pm != null
                                               ? pm.Projectmanager
                                               : l.ChangedBy != null ? "(" + l.ChangedBy + ")" : null,
                    UpdateType           = l.UpdateType
                };

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        private static LogMilestone BuildLogEntry(Milestone m, char updateType, string? changedBy)
            => new()
            {
                Project              = m.Project,
                Number               = m.Number,
                Description          = m.Description,
                DateDue              = DateTime.SpecifyKind(m.DateDue, DateTimeKind.Unspecified),
                DateCompleted        = m.DateCompleted.HasValue
                                           ? DateTime.SpecifyKind(m.DateCompleted.Value, DateTimeKind.Unspecified)
                                           : null,
                DateFormReceived     = m.DateFormReceived.HasValue
                                           ? DateTime.SpecifyKind(m.DateFormReceived.Value, DateTimeKind.Unspecified)
                                           : null,
                UnderSdReview        = m.UnderSdReview,
                OnTarget             = m.OnTarget,
                ProjectLeaderComment = m.ProjectLeaderComment,
                CapsComment          = m.CapsComment,
                IdType               = string.IsNullOrEmpty(m.IdType) ? null : m.IdType[0],
                DateChanged          = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ChangedBy            = changedBy,
                UpdateType           = updateType
            };

        // ── Staging / Import ─────────────────────────────────────────────────
        public async Task<PagedData<StagingMilestone>> GetAllStagingRowsAsync(PaginationParameters<string> parameters)
        {

            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones
               .AsNoTracking();

            //query = ApplyFilter(query, parameters.Filter);
            query = ApplyStagingSorting(query, parameters.SortBy, parameters.Descending);


            return await ApplyPaging(query, parameters.Page, parameters.PageSize);


        }

        public async Task<List<StagingMilestone>> GetStagingRowsAsync(string? project)
        {
            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(project))
                query = query.Where(s => s.Project == project);
            return await query.ToListAsync();
        }

        public async Task<StagingMilestone> AddStagingRowAsync(StagingMilestone entity)
        {
            _dbContext.StagingMilestones.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<StagingMilestone> UpdateStagingRowAsync(StagingMilestone entity)
        {
            _dbContext.StagingMilestones.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteStagingRowAsync(int id)
        {
            int rows = await _dbContext.StagingMilestones
                .Where(s => s.Id == id)
                .ExecuteDeleteAsync();
            return rows > 0;
        }

        public async Task<int> ClearStagingAsync(string project)
            => await _dbContext.StagingMilestones
                .ExecuteDeleteAsync();

        public async Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode)
        {
            List<StagingMilestone> rows = await _dbContext.StagingMilestones
                .Where(s => s.Project == project)
                .ToListAsync();

            foreach (StagingMilestone row in rows)
            {
                // Skip fully empty rows
                if (string.IsNullOrWhiteSpace(row.Description) &&
                    //string.IsNullOrWhiteSpace(row.AltDescription) &&
                    string.IsNullOrWhiteSpace(row.Number)
                    //&& string.IsNullOrWhiteSpace(row.AltNumber)
                    )
                {
                    _dbContext.StagingMilestones.Remove(row);
                    continue;
                }

                //// Deliverable mode: swap alt fields into primary fields
                //if (isDeliverableMode && string.IsNullOrWhiteSpace(row.Number) && !string.IsNullOrWhiteSpace(row.AltNumber))
                //{
                //    row.Description = row.AltDescription;
                //    row.DateDue     = DateTime.TryParse(row.AltDate, out DateTime altDt) ? altDt : row.DateDue;
                //    row.Number      = row.AltNumber;
                //}

                row.TypeId = typeId;
                row.Note = null;

                // Deliverable mode: auto-assign next number when number is missing
                //if (isDeliverableMode && string.IsNullOrWhiteSpace(row.Number) && year.HasValue)
                //    row.Number = await GetNextMilestoneNumberAsync(project, year.Value);

                // Validate date
                if (row.DateDue == default)
                    row.Note = (row.Note ?? string.Empty) + "(*Please check this date*)";

                // Validate number format: must match YY/NN
                if (!string.IsNullOrWhiteSpace(row.Number) &&
                    !System.Text.RegularExpressions.Regex.IsMatch(row.Number.Trim(), @"^\d{2}/\d{2}$"))
                {
                    row.Note = (row.Note ?? string.Empty) + " Please check this number format.";
                }
                else if (!string.IsNullOrWhiteSpace(row.Number))
                {
                    string trimmed = row.Number.Trim();
                    bool exists = await _dbContext.Milestones
                        .AnyAsync(m => m.Project == project && m.Number == trimmed);
                    if (exists)
                        row.Note = (row.Note ?? string.Empty) + " This Project Milestone already exists.";
                    else
                        row.Number = trimmed;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> ImportStagingAsync(string project, string? changedBy)
        {
            List<StagingMilestone> validRows = await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Project == project && s.Note == null)
                .ToListAsync();

            if (validRows.Count == 0)
                return 0;

            List<Milestone> newMilestones = validRows.Select(s => new Milestone
            {
                Project = project,
                Number = s.Number!,
                Description = s.Description,
                DateDue = DateTime.SpecifyKind(s.DateDue, DateTimeKind.Unspecified),
                IdType = s.TypeId
            }).ToList();

            await _dbContext.Milestones.AddRangeAsync(newMilestones);
            await _dbContext.SaveChangesAsync();

            foreach (Milestone m in newMilestones)
            {
                try
                {
                    _dbContext.LogMilestones.Add(BuildLogEntry(m, 'I', changedBy));
                }
                catch { /* log write failure must not affect the import */ }
            }

            try { await _dbContext.SaveChangesAsync(); }
            catch { /* log write failure must not affect the import */ }

            return newMilestones.Count;
        }

        public async Task<int> ImportWithOverwriteAsync(string project, string? changedBy)
        {
            // Set project on all staging rows first
            await _dbContext.StagingMilestones
                .Where(s => s.Project == null || s.Project == string.Empty)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, project));

            // Find staging rows that match an existing milestone (by project + number)
            List<StagingMilestone> stagingRows = await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Project == project)
                .ToListAsync();

            int updated = 0;
            foreach (StagingMilestone stagingRow in stagingRows)
            {
                Milestone? existing = await _dbContext.Milestones
                    .FirstOrDefaultAsync(m => m.Project == project && m.Number == stagingRow.Number);

                if (existing is null)
                    continue;

                existing.DateDue = DateTime.SpecifyKind(stagingRow.DateDue, DateTimeKind.Unspecified);
                existing.Description = stagingRow.Description;
                _dbContext.Milestones.Update(existing);
                await _dbContext.SaveChangesAsync();

                try
                {
                    _dbContext.LogMilestones.Add(BuildLogEntry(existing, 'U', changedBy));
                    await _dbContext.SaveChangesAsync();
                }
                catch { /* log write failure must not affect the overwrite */ }

                await _dbContext.StagingMilestones
                    .Where(s => s.Project == project && s.Number == stagingRow.Number)
                    .ExecuteDeleteAsync();

                updated++;
            }

            return updated;
        }

        public async Task<string> GetNextMilestoneNumberAsync(string project, int year)
        {
            string yr2d = year.ToString()[^2..];

            string? latestInMilestone = await _dbContext.Milestones
                .AsNoTracking()
                .Where(m => m.Project == project && m.Number != null && m.Number.StartsWith(yr2d))
                .MaxAsync(m => m.Number);

            string? latestInStaging = await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Number != null && s.Number.StartsWith(yr2d))
                .MaxAsync(s => s.Number);

            if (string.IsNullOrEmpty(latestInMilestone) && string.IsNullOrEmpty(latestInStaging))
                return $"{yr2d}/01";

            int milestoneSeq = ParseSeq(latestInMilestone);
            int stagingSeq = ParseSeq(latestInStaging);
            int next = Math.Max(milestoneSeq, stagingSeq) + 1;
            return $"{yr2d}/{next:D2}";
        }

        private static int ParseSeq(string? number)
        {
            if (string.IsNullOrWhiteSpace(number)) return 0;
            int slash = number.IndexOf('/');
            if (slash < 0 || slash >= number.Length - 1) return 0;
            return int.TryParse(number[(slash + 1)..], out int seq) ? seq : 0;
        }
    }
}
