using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class StaffJobRepository : IStaffJobRepository
    {       
        private readonly FpsDbContext _dbContext;
        private readonly IProgramRepository _programRepository;
        private readonly IProjectRepository _projectRepository;

        public StaffJobRepository(FpsDbContext dbContext,
            IProjectRepository projectRepository, IProgramRepository programRepository)
        {           
            _dbContext = dbContext;
            _projectRepository = projectRepository;
            _programRepository = programRepository;
        }

        public IQueryable<StaffJob> Get()
        {
            var projects = _projectRepository.Get();

            return (from p in _dbContext.StaffJobs
                    join ap in projects
                        on p.JobCode equals ap.ParentProject
                    select p).AsQueryable();
        }

        public async Task<PagedData<StaffJobView>> GetJobStaffCostAsync(PaginationParameters<string> query)
        {

            var programs = _programRepository.Get();
            var projects = _projectRepository.Get();
            var staffs = Get();
            var employee = from wg in _dbContext.WgEmployees
                           join e in _dbContext.Employees
                                on wg.SpNumber equals e.SPNumber
                           select new
                           {
                               StaffId = wg.PactId,
                               Name = (e.LastName ?? "") + ", " + (e.FirstName ?? ""),
                               WorkGroupGrade = wg.WorkGroupGrade
                           };
            var dutyHours = await _dbContext.TblSettings.Where(e => e.Id == "HoursInDay").Select(e => e.Setting).FirstOrDefaultAsync();

            var queryStaffJob = (from sj in staffs
                                join s in employee on sj.StaffId equals s.StaffId
                                join wg in _dbContext.WorkgroupGrades on s.WorkGroupGrade equals wg.WgGrade
                                join pc in _dbContext.ProfitcentreGrades on wg.ProfitCentreGrade equals pc.PcGrade
                                join p in projects on sj.JobCode equals p.ParentProject
                                join prg in programs on p.Program equals prg.ProgramNo                               
                                select new StaffJobView
                                {
                                    StaffID = sj.StaffId,
                                    JobCode = sj.JobCode,
                                    PlannedHours = sj.PlannedHours,
                                    Name = s.Name,
                                    WorkGroupGrade = s.WorkGroupGrade,
                                    ChargeRate = p.IsDefraProject == -1
                                                    ? pc.DefraChargeRate
                                                    : pc.ChargeRate,
                                    StaffCost =
                                            (decimal)sj.PlannedHours *
                                            (p.IsDefraProject == -1 ? pc.DefraChargeRate : pc.ChargeRate) *
                                            (prg.SectorName == "charge" ? 1m : 0m),
                                    GradeCode = wg.GradeCode,
                                    WorkGroup = wg.Workgroup,
                                    SectorName = prg.SectorName,
                                    Days = dutyHours != null ? sj.PlannedHours / Convert.ToDouble(dutyHours) : 0
                                }).OrderBy(e => e.Name).AsQueryable();

            //With ExpandoObject
            if (!String.IsNullOrEmpty(query.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(query.Filter);
                if (filterModel != null)
                {
                    var dict = (IDictionary<string, object>)filterModel;
                    if (dict.ContainsKey("Name") && dict["Name"] != null)
                    {
                        queryStaffJob = queryStaffJob.Where(x => x.Name!.Contains(dict["Name"].ToString()));
                    }
                    if (dict.ContainsKey("PlannedHours") && dict["PlannedHours"] != null)
                    {
                        queryStaffJob = queryStaffJob.Where(x => x.PlannedHours.ToString().Contains(dict["PlannedHours"].ToString()));
                    }
                }
            }

            var result = await queryStaffJob.ToListAsync();

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<List<StaffWorkgroupLookup>> GetStaffWorkgroupLookup()
        {
            var query = (from wgemp in _dbContext.WgEmployees
                         join emp in _dbContext.Employees
                             on wgemp.SpNumber equals emp.SPNumber
                         join wgg in _dbContext.WorkgroupGrades
                             on wgemp.WorkGroupGrade equals wgg.WgGrade
                         join wg in _dbContext.Workgroups
                             on wgg.Workgroup equals wg.WorkgroupName
                         join pc in _dbContext.ProfitCentres
                             on wg.ProfitCentre equals pc.Profitcentre
                         join upc in _dbContext.UserProfitcentres
                             on pc.Profitcentre equals upc.ProfitCentre
                         join u in _dbContext.Users
                             on upc.UserId equals u.UserId
                         where u.Username == "dbo" && wgemp.MakeAvailable == -1
                         orderby emp.LastName, emp.FirstName
                         select new StaffWorkgroupLookup
                         {
                             StaffID = wgemp.PactId,
                             Name = (emp.LastName ?? "") + ", " + (emp.FirstName ?? ""),
                             WorkGroupGrade = wgemp.WorkGroupGrade,
                             HrsAvail = wgemp.HrsAvail
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
                PlannedHours = staffJob.PlannedHours
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

            await _dbContext.SaveChangesAsync();

            return existingStaffJob;
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(jobCode);

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var staffJob = await _dbContext.StaffJobs
                    .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);

                if (staffJob is null)
                    return false;

                _dbContext.StaffJobs.Remove(staffJob);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private PagedData<T> ApplyPaging<T>(
                    IEnumerable<T> source,
                    int page,
                    int pageSize)
        {
            var list = source.ToList();
            var totalRecords = list.Count;

            var result = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(result, pagination);
        }
    }
}
