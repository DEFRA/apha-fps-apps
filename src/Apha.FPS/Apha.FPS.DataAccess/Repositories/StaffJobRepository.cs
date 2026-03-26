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
        private readonly IFpsYearContext _yearContext;
        private readonly int userId = 42;

        public StaffJobRepository(FpsDbContext dbContext, IFpsYearContext fpsYearContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _yearContext = fpsYearContext;
        }

        public async Task<PagedData<StaffJobView>> GetJobStaffCostAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryStaffJob = await BuildJobStaffCostQueryAsync(jobCode);
            // Apply filtering
            queryStaffJob = ApplyStaffJobFilter(queryStaffJob, query.Filter);

            queryStaffJob = (IQueryable<StaffJobView>)ApplySorting(queryStaffJob, query.SortBy, query.Descending);

            var result = await queryStaffJob.ToListAsync();

            return base.ApplyPaging(result, query.Page, query.PageSize);
        }       

        public async Task<List<StaffWorkgroupLookup>> GetStaffWorkgroupLookup()
        {
            var query = (from s in _dbContext.StaffViews
                         join sp in _dbContext.StaffPickViews on s.StaffId equals sp.StaffId
                         where s.UserId == userId
                         select new StaffWorkgroupLookup
                         {
                             StaffID = s.StaffId ?? "",
                             Name = s.Name ?? "",
                             WorkGroupGrade = s.WorkgroupGrade ?? "",
                             HrsAvail = s.HrsAvail ?? 0
                         }).OrderBy(e => e.Name);

            return await query.ToListAsync();
        }

        public async Task<decimal?> GetStaffChargeRate(string staffId, string jobcode)
        {
            var result =
                    from wg in _dbContext.WgEmployees
                    join e in _dbContext.Employees
                        on wg.SpNumber equals e.SPNumber
                    join w in _dbContext.WorkgroupGrades
                        on wg.WorkGroupGrade equals w.WgGrade
                    join p in _dbContext.ProfitcentreGrades
                        on w.ProfitCentreGrade equals p.PcGrade
                    join s in _dbContext.StaffJobs
                        on wg.PactId equals s.StaffId
                    join t in _dbContext.Projects
                        on s.JobCode equals t.ParentProject
                    where s.StaffId == staffId && t.ParentProject == jobcode
                    select new
                    {
                        ChargeRate = t.IsDefraProject == -1
                            ? p.DefraChargeRate
                            : p.ChargeRate
                    };

            decimal? changeRate = await result.Select(e => e.ChargeRate).FirstOrDefaultAsync();
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
            return await queryStaffJob.Where(e => e.StaffID == staffId).FirstOrDefaultAsync();
        }

        public async Task<StaffJob> AddAsync(StaffJob staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);

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
                FpsCalYear = _yearContext.FPSYear
            };

            _dbContext.StaffJobs.Add(newStaffJob);
            await _dbContext.SaveChangesAsync();

            return newStaffJob;
        }

        public async Task<StaffJob> UpdateAsync(StaffJob staffJob)
        {
            ArgumentNullException.ThrowIfNull(staffJob);
            ArgumentOutOfRangeException.ThrowIfNegative(staffJob.PlannedHours);

            var existingStaffJob = await _dbContext.StaffJobs
                .FirstOrDefaultAsync(sj => sj.StaffId == staffJob.StaffId
                                        && sj.JobCode == staffJob.JobCode);

            if (existingStaffJob is null)
                throw new InvalidOperationException(
                    $"Staff job with StaffId {staffJob.StaffId} and JobCode {staffJob.JobCode} not found");

            existingStaffJob.PlannedHours = staffJob.PlannedHours;
            existingStaffJob.FpsCalYear = _yearContext.FPSYear;

            await _dbContext.SaveChangesAsync();

            return existingStaffJob;
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jobCode);

            var staffJob = await _dbContext.StaffJobs
                   .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);

            if (staffJob is null)
                return false;

            _dbContext.StaffJobs.Remove(staffJob);
            await _dbContext.SaveChangesAsync();            

            return true;
        }

        private async Task<IQueryable<StaffJobView>> BuildJobStaffCostQueryAsync(string jobCode)
        {
            var dutyHours = await _dbContext.TblSettings
                .Where(e => e.Id == "HoursInDay")
                .Select(e => e.Setting)
                .FirstOrDefaultAsync();

            return (from sj in _dbContext.StaffJobTblViews
                    join s in _dbContext.StaffGeneralViews on sj.StaffId equals s.StaffId
                    join wg in _dbContext.WorkgroupGrades on s.WorkGroupGrade equals wg.WgGrade
                    join pc in _dbContext.ProfitcentreGrades on wg.ProfitCentreGrade equals pc.PcGrade
                    join p in _dbContext.ProjectViews on
                        new { sj.JobCode, sj.UserId } equals new { JobCode = p.ParentProject, p.UserId }
                    join prg in _dbContext.ProgramViews on
                        new { p.Program, sj.UserId } equals new { Program = prg.ProgramNo, prg.UserId }
                    let dailyRate = (p.IsDefraProject == -1 ? pc.DefraChargeRate : pc.ChargeRate)
                    where sj.JobCode == jobCode && p.UserId == userId
                    select new StaffJobView
                    {
                        StaffID = sj.StaffId,
                        JobCode = sj.JobCode,
                        PlannedHours = sj.PlannedHours ?? 0,
                        Name = s.Name,
                        WorkGroupGrade = s.WorkGroupGrade,
                        ChargeRate = dailyRate,
                        StaffCost =
                                   (decimal)(sj.PlannedHours ?? 0) *
                                   (dailyRate) *
                                   ((prg.SectorName ?? "").ToLower().Trim() == "charge" ? 1m : 0m),
                        GradeCode = wg.GradeCode,
                        WorkGroup = wg.Workgroup,
                        SectorName = prg.SectorName,
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
                queryStaffJob = queryStaffJob.Where(x => x.Name!.Contains(name.ToString()!));
            }

            if (dict.TryGetValue("PlannedHours", out var plannedHours) && plannedHours != null)
            {
                queryStaffJob = queryStaffJob.Where(x => x.PlannedHours.ToString().Contains(plannedHours.ToString()!));
            }

            return queryStaffJob;
        }
    }
}
