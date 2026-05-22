using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Apha.FPS.DataAccess.Repositories
{
    public class StaffJobRepository : BaseRepository, IStaffJobRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public StaffJobRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<PagedData<StaffJobView>> GetJobStaffCostAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryStaffJob = await BuildJobStaffCostQueryAsync(jobCode);
            // Apply filtering
            queryStaffJob = ApplyStaffJobFilter(queryStaffJob, query.Filter);

            queryStaffJob = (IQueryable<StaffJobView>)ApplySorting(queryStaffJob, query.SortBy, query.Descending);

            var result = (await queryStaffJob.ToListAsync())
                .Select(ComputeStaffCost)
                .ToList();

            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalStaffCostAsync(string jobCode)
        {
            var query = await BuildJobStaffCostQueryAsync(jobCode);
            var result = (await query.ToListAsync()).Select(ComputeStaffCost).ToList();
            return result != null ? ((result.Sum(x => x.StaffCost)) ?? 0m) : 0m;
        }

        public async Task<List<StaffWorkgroupLookup>> GetStaffWorkgroupLookup()
        {
            var query = (from s in _dbContext.StaffViews
                         join sp in _dbContext.StaffPickViews on s.StaffId equals sp.StaffId
                         where s.UserEmail != null && s.UserEmail.ToLower() == _requestContext.UserEmailId
                         select new StaffWorkgroupLookup
                         {
                             StaffID = s.StaffId ?? "",
                             Name = s.Name ?? "",
                             WorkGroupGrade = s.WorkgroupGrade ?? "",
                             HrsAvail = s.HrsAvail ?? 0
                         }).Distinct().OrderBy(e => e.Name);

            return await query.ToListAsync();
        }

        public async Task<decimal?> GetStaffChargeRate(string staffId, string jobcode)
        {
            var result =
                    from wg in _dbContext.WorkGroupEmployees
                    join e in _dbContext.Employees
                        on wg.SpNumber equals e.SPNumber
                    join w in _dbContext.WorkgroupGrades
                        on wg.WorkGroupGrade equals w.WgGrade
                    join p in _dbContext.ProfitCentreGrades
                        on w.ProfitCentreGrade equals p.PcGrade
                    join s in _dbContext.StaffJobs
                        on wg.PactId equals s.StaffId
                    join t in _dbContext.Projects
                        on s.JobCode equals t.ParentProject
                    where s.StaffId == staffId 
                    select new
                    {
                        ParentProject = t.ParentProject,
                        ChargeRate = t.IsDefraProject == -1
                            ? p.DefraChargeRate
                            : p.ChargeRate
                    };

            decimal? changeRate = await result.Where(e => e.ParentProject == jobcode).Select(e => e.ChargeRate).FirstOrDefaultAsync();
            changeRate ??= await result.Select(e => e.ChargeRate).FirstOrDefaultAsync(); 
            return changeRate;
        }

        public async Task<StaffJob?> GetByIdAsync(string staffId, string jobCode)
        {
            var query = await _dbContext.StaffJobs
                    .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);
            return query;
        }

        public async Task<StaffJobView?> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var queryStaffJob = await BuildJobStaffCostQueryAsync(jobCode);
            var record = await queryStaffJob.Where(e => e.StaffID == staffId).FirstOrDefaultAsync();
            return record != null ? ComputeStaffCost(record) : null;
        }

        public async Task<StaffJob> AddAsync(StaffJob staffJob)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingStaffJob = await _dbContext.StaffJobs
                        .FirstOrDefaultAsync(sj => sj.StaffId == staffJob.StaffId
                                                && sj.JobCode == staffJob.JobCode);

                    if (existingStaffJob is not null)
                        throw new InvalidOperationException(
                            $"Staff job with StaffId {staffJob.StaffId} and JobCode {staffJob.JobCode} already exists");

                    var newStaffJob = new StaffJob
                    {
                        StaffId = staffJob.StaffId,
                        JobCode = staffJob.JobCode,
                        PlannedHours = staffJob.PlannedHours,
                        FpsYear = _requestContext.FpsYear
                    };

                    var logEntry = CreateStaffJobLogEntry(newStaffJob.StaffId, newStaffJob.JobCode, newStaffJob.PlannedHours, "I");

                    _dbContext.StaffJobs.Add(newStaffJob);
                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return newStaffJob;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<StaffJob> UpdateAsync(StaffJob staffJob)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingStaffJob = await _dbContext.StaffJobs
                        .FirstOrDefaultAsync(sj => sj.StaffId == staffJob.StaffId
                                                && sj.JobCode == staffJob.JobCode);

                    if (existingStaffJob is null)
                        throw new InvalidOperationException(
                            $"Staff job with StaffId {staffJob.StaffId} and JobCode {staffJob.JobCode} not found");

                    existingStaffJob.PlannedHours = staffJob.PlannedHours;
                    existingStaffJob.FpsYear = _requestContext.FpsYear;

                    var logEntry = CreateStaffJobLogEntry(existingStaffJob.StaffId, existingStaffJob.JobCode, existingStaffJob.PlannedHours, "U");

                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return existingStaffJob;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            var staffJob = await _dbContext.StaffJobs
                   .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);

            if (staffJob is null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateStaffJobLogEntry(staffJob.StaffId, staffJob.JobCode, staffJob.PlannedHours, "D");

                    _dbContext.StaffJobs.Remove(staffJob);
                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static StaffJobView ComputeStaffCost(StaffJobView view)
        {
            view.StaffCost = (decimal)view.PlannedHours *
                             (view.ChargeRate ?? 0m) *
                             ((view.SectorName ?? "").Trim().ToLower() == "charge" ? 1m : 0m);
            return view;
        }

        private StaffJobLog CreateStaffJobLogEntry(string staffId, string jobCode, double plannedHours, string insertDelete)
        {
            return new StaffJobLog
            {
                StaffId = staffId,
                JobCode = jobCode,
                PlannedHours = plannedHours,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _requestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _requestContext.FpsYear
            };
        }

        private async Task<IQueryable<StaffJobView>> BuildJobStaffCostQueryAsync(string jobCode)
        {
            var dutyHours = await _dbContext.TblSettings
                .Where(e => e.Id == "HoursInDay")
                .Select(e => e.Setting)
                .FirstOrDefaultAsync();


            var projProgram = (from p in _dbContext.ProjectViews
                              join prg in _dbContext.ProgramViews on
                                  new { p.Program, p.UserId } equals new { Program = prg.ProgramNo, prg.UserId }
                              where p.ParentProject == jobCode
                                    && p.UserEmail != null
                                    && p.UserEmail.ToLower() == _requestContext.UserEmailId
                              select new
                              {
                                  p.ParentProject,
                                  prg.SectorName,
                                  p.IsDefraProject, 
                                  prg.UserId,
                                  prg.UserEmail
                              }).Distinct();

            return (from sj in _dbContext.StaffJobTblViews
                    join s in _dbContext.StaffGeneralViews on sj.StaffId equals s.StaffId
                    join wg in _dbContext.WorkgroupGrades on s.WorkGroupGrade equals wg.WgGrade
                    join pc in _dbContext.ProfitCentreGrades on wg.ProfitCentreGrade equals pc.PcGrade
                    join pp in projProgram on
                        new { sj.JobCode, sj.UserId } equals new { JobCode = pp.ParentProject, pp.UserId }
                    let dailyRate = (pp.IsDefraProject == -1 ? pc.DefraChargeRate : pc.ChargeRate)
                    where sj.JobCode == jobCode
                    select new StaffJobView
                    {
                        StaffID = sj.StaffId,
                        JobCode = sj.JobCode,
                        PlannedHours = sj.PlannedHours ?? 0,
                        Name = s.Name,
                        WorkGroupGrade = s.WorkGroupGrade,
                        ChargeRate = dailyRate,
                        StaffCost = 0m,
                        GradeCode = wg.GradeCode,
                        WorkGroup = wg.Workgroup,
                        SectorName = pp.SectorName,
                        Days = dutyHours != null ? (sj.PlannedHours ?? 0) / Convert.ToDouble(dutyHours) : 0
                    }).Distinct().OrderBy(e => e.Name).AsQueryable();            
        }

        private static IQueryable ApplySorting(IQueryable<StaffJobView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<StaffJobView> query, string property, bool descending)
        {
            return property switch
            {
                "name" => ApplyOrder(query, i => i.Name, descending),
                "chargerate" => ApplyOrder(query, i => i.ChargeRate, descending),
                "plannedhours" => ApplyOrder(query, i => i.PlannedHours, descending),
                "days" => ApplyOrder(query, i => i.Days, descending),
                "staffcost" => ApplyOrder(query, i => i.StaffCost, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<StaffJobView> query, Expression<Func<StaffJobView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<StaffJobView> ApplyStaffJobFilter(IQueryable<StaffJobView> queryStaffJob, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryStaffJob;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryStaffJob;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Name", out var name) && name != null)
            {
                queryStaffJob = queryStaffJob.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));
            }

            if (dict.TryGetValue("PlannedHours", out var plannedHours) && plannedHours != null)
            {
                queryStaffJob = queryStaffJob.Where(x => EF.Functions.ILike(x.PlannedHours.ToString(), $"%{plannedHours}%"));
            }

            return queryStaffJob;
        }
    }
}
