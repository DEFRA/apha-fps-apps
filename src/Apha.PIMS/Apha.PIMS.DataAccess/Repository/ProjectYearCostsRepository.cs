using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProjectYearCostsRepository : IProjectYearCostsRepository
    {
        private static readonly string[] AnimalAcctCodes = ["LargeAnimals", "SmallAnimals", "Mice"];
        private readonly PimsDbContext _context;

        public ProjectYearCostsRepository(PimsDbContext context)
        {
            _context = context;
        }

        public async Task<PagedData<ProjSubContract>> GetAdditionalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<ProjSubContract> query = _context.ProjSubContracts
                .AsNoTracking()
                .Where(s => s.Project == project
                         && s.Year == year
                         && !AnimalAcctCodes.Contains(s.Acctcode));

            query = ApplyActualsSearch(query, paging.Search);
            query = ApplyActualsSorting(query, paging.SortBy, paging.Descending);
            List<ProjSubContract> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<AdditionalCosts>> GetAdditionalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<AdditionalCosts> query = _context.AdditionalCosts
                .AsNoTracking()
                .Where(s => s.Jobcode == project && s.Year == year);

            query = ApplyPlansSearch(query, paging.Search);
            query = ApplyPlansSorting(query, paging.SortBy, paging.Descending);
            List<AdditionalCosts> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<ProjSubContract>> GetAnimalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<ProjSubContract> query = _context.ProjSubContracts
                .AsNoTracking()
                .Where(s => s.Project == project
                         && s.Year == year
                         && AnimalAcctCodes.Contains(s.Acctcode));

            query = ApplyActualsSearch(query, paging.Search);
            query = ApplyActualsSorting(query, paging.SortBy, paging.Descending);
            List<ProjSubContract> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<ProjectAnimalPlan>> GetAnimalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<ProjectAnimalPlan> query = _context.ProjectAnimalPlans
                .AsNoTrackingWithIdentityResolution()
                .Where(s => s.Parentproject == project && s.Year == year);

            query = ApplyAnimalPlanSearch(query, paging.Search);
            query = ApplyAnimalPlanSorting(query, paging.SortBy, paging.Descending);
            List<ProjectAnimalPlan> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        private static IQueryable<ProjSubContract> ApplyActualsSearch(
            IQueryable<ProjSubContract> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Acctcode    != null && x.Acctcode.ToLower().Contains(s)) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                (x.Supplier    != null && x.Supplier.ToLower().Contains(s)));
        }

        private static IQueryable<AdditionalCosts> ApplyPlansSearch(
            IQueryable<AdditionalCosts> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Account     != null && x.Account.ToLower().Contains(s)) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                (x.Supplier    != null && x.Supplier.ToLower().Contains(s)));
        }

        private static IQueryable<ProjSubContract> ApplyActualsSorting(
            IQueryable<ProjSubContract> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "acctcode"    => ApplyOrder(query, x => x.Acctcode,    descending),
                "description" => ApplyOrder(query, x => x.Description, descending),
                "amount"      => ApplyOrder(query, x => x.Amount,      descending),
                "month"       => ApplyOrder(query, x => x.Month,       descending),
                "supplier"    => ApplyOrder(query, x => x.Supplier,    descending),
                _             => query.OrderBy(x => x.Month)
                                      .ThenBy(x => x.Acctcode)
                                      .ThenBy(x => x.Subcontcounter)  // ✅ stable tie-breaker
            };
        }

        private static IQueryable<AdditionalCosts> ApplyPlansSorting(
            IQueryable<AdditionalCosts> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "account"     => ApplyOrder(query, x => x.Account,     descending),
                "description" => ApplyOrder(query, x => x.Description, descending),
                "itemcost"    => ApplyOrder(query, x => x.Itemcost,    descending),
                _             => query.OrderBy(x => x.Account)
            };
        }

        private static IQueryable<T> ApplyOrder<T, TKey>(
            IQueryable<T> query, Expression<Func<T, TKey>> keySelector, bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        private static IQueryable<ProjectAnimalPlan> ApplyAnimalPlanSearch(
            IQueryable<ProjectAnimalPlan> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x => x.Animaltype != null && x.Animaltype.ToLower().Contains(s));
        }

        private static IQueryable<ProjectAnimalPlan> ApplyAnimalPlanSorting(
            IQueryable<ProjectAnimalPlan> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "animaltype"      => ApplyOrder(query, x => x.Animaltype,      descending),
                "numberofdays"    => ApplyOrder(query, x => x.Numberofdays,    descending),
                "numberofanimals" => ApplyOrder(query, x => x.Numberofanimals, descending),
                "rate"            => ApplyOrder(query, x => x.Rate,            descending),
                "cost"            => ApplyOrder(query, x => x.Cost,            descending),
                _                 => query.OrderBy(x => x.Animaltype)
            };
        }

        public async Task<PagedData<TestReqmt>> GetTestPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<TestReqmt> query = _context.TestReqmts
                .AsNoTracking()
                .Where(t => t.Buyer == project && t.Year == year);

            query = ApplyTestPlanSearch(query, paging.Search);
            query = ApplyTestPlanSorting(query, paging.SortBy, paging.Descending);
            List<TestReqmt> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<(MonthlyOutput Output, TestReqmt Reqmt)>> GetTestActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            List<(MonthlyOutput Output, TestReqmt Reqmt)> joined = await (
                from mo in _context.MonthlyOutputs.AsNoTracking()
                join tr in _context.TestReqmts.AsNoTracking()
                    on new { mo.Year, mo.Testcode, mo.Buyer }
                    equals new { tr.Year, tr.Testcode, tr.Buyer }
                where mo.Buyer == project && mo.Year == year
                select new { mo, tr }
            ).ToListAsync().ContinueWith(t => t.Result.Select(x => (x.mo, x.tr)).ToList());

            string? search = paging.Search?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
            {
                joined = joined.Where(x =>
                    (x.Output.Testcode?.ToLower().Contains(search) ?? false) ||
                    (x.Output.Workgroup?.ToLower().Contains(search) ?? false) ||
                    (x.Output.Buyer?.ToLower().Contains(search) ?? false))
                    .ToList();
            }

            joined = (paging.SortBy?.ToLower()) switch
            {
                "testcode"  => paging.Descending ? joined.OrderByDescending(x => x.Output.Testcode).ToList()  : joined.OrderBy(x => x.Output.Testcode).ToList(),
                "month"     => paging.Descending ? joined.OrderByDescending(x => x.Output.Month).ToList()     : joined.OrderBy(x => x.Output.Month).ToList(),
                "workgroup" => paging.Descending ? joined.OrderByDescending(x => x.Output.Workgroup).ToList() : joined.OrderBy(x => x.Output.Workgroup).ToList(),
                "volume"    => paging.Descending ? joined.OrderByDescending(x => x.Output.Volume).ToList()    : joined.OrderBy(x => x.Output.Volume).ToList(),
                _           => joined.OrderBy(x => x.Output.Testcode).ThenBy(x => x.Output.Month).ToList()
            };

            return ApplyPaging(joined, paging.Page, paging.PageSize);
        }

        private static IQueryable<TestReqmt> ApplyTestPlanSearch(
            IQueryable<TestReqmt> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Testcode != null && x.Testcode.ToLower().Contains(s)) ||
                (x.Buyer    != null && x.Buyer.ToLower().Contains(s)));
        }

        private static IQueryable<TestReqmt> ApplyTestPlanSorting(
            IQueryable<TestReqmt> query, string? sortBy, bool descending)
        {
            return (sortBy?.ToLower()) switch
            {
                "testcode"    => ApplyOrder(query, x => x.Testcode,    descending),
                "buyer"       => ApplyOrder(query, x => x.Buyer,       descending),
                "unitprice"   => ApplyOrder(query, x => x.Unitprice,   descending),
                "norequired"  => ApplyOrder(query, x => x.Norequired,  descending),
                _             => query.OrderBy(x => x.Testcode)
            };
        }

        private static PagedData<T> ApplyPaging<T>(List<T> data, int page, int pageSize)
        {
            if (page == -1)
                return new PagedData<T>(data, new PaginationData
                {
                    PageNumber   = page,
                    PageSize     = data.Count,
                    TotalRecords = data.Count,
                    TotalPages   = 1
                });

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            int total = data.Count;
            int totalPages = (int)Math.Ceiling((double)total / pageSize);
            List<T> paged = data.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedData<T>(paged, new PaginationData
            {
                PageNumber   = page,
                PageSize     = pageSize,
                TotalRecords = total,
                TotalPages   = totalPages
            });
        }

        public async Task<PagedData<ProjectStaffPlan>> GetStaffPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<ProjectStaffPlan> query = _context.ProjectStaffPlans
                .AsNoTracking()
                .Where(s => s.Parentproject == project && s.Year == year);

            string? search = paging.Search?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    (x.Workgroupgrade != null && x.Workgroupgrade.ToLower().Contains(search)) ||
                    (x.Name           != null && x.Name.ToLower().Contains(search)));

            query = (paging.SortBy?.ToLower()) switch
            {
                "wggrade"      => ApplyOrder(query, x => x.Workgroupgrade, paging.Descending),
                "name"         => ApplyOrder(query, x => x.Name,          paging.Descending),
                "plannedhours" => ApplyOrder(query, x => x.Plannedhours,  paging.Descending),
                "rate"         => ApplyOrder(query, x => x.Rate,          paging.Descending),
                "cost"         => ApplyOrder(query, x => x.Cost,          paging.Descending),
                _              => query.OrderBy(x => x.Workgroupgrade)
            };

            List<ProjectStaffPlan> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<TimeCostCalcs>> GetStaffActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<TimeCostCalcs> query = _context.TimeCostCalcs
                .AsNoTracking()
                .Where(s => s.Project == project && s.Year == year);

            string? search = paging.Search?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    (x.Jobcode    != null && x.Jobcode.ToLower().Contains(search)) ||
                    (x.Name       != null && x.Name.ToLower().Contains(search)) ||
                    (x.Workgroup  != null && x.Workgroup.ToLower().Contains(search)) ||
                    (x.Gradecode  != null && x.Gradecode.ToLower().Contains(search)));

            query = (paging.SortBy?.ToLower()) switch
            {
                "jobcode"   => ApplyOrder(query, x => x.Jobcode,    paging.Descending),
                "name"      => ApplyOrder(query, x => x.Name,       paging.Descending),
                "workgroup" => ApplyOrder(query, x => x.Workgroup,  paging.Descending),
                "gradecode" => ApplyOrder(query, x => x.Gradecode,  paging.Descending),
                "month"     => ApplyOrder(query, x => x.Month,      paging.Descending),
                "time"      => ApplyOrder(query, x => x.Time,       paging.Descending),
                "cost"      => ApplyOrder(query, x => x.Cost,       paging.Descending),
                _           => query.OrderBy(x => x.Jobcode).ThenBy(x => x.Month)
            };

            List<TimeCostCalcs> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<Projects?> GetProjectYearDetailsAsync(string project, short year)
        {
            return await _context.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Parentproject == project && p.Year == year)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedData<PactPayCalc>> GetPactPayAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            // Mirror qryProjectTimeCostCalcs: GROUP BY Year, Project, Month
            // SUM(Pay), SUM(NonPay), SUM(Cost) AS StaffCosts, SUM(Overhead)
            IQueryable<PactPayCalc> query = _context.TimeCostCalcs
                .AsNoTracking()
                .Where(s => s.Project == project && s.Year == year)
                .GroupBy(s => new { s.Year, s.Project, s.Month })
                .Select(g => new PactPayCalc
                {
                    Year       = g.Key.Year,
                    Project    = g.Key.Project,
                    Month      = g.Key.Month,
                    Pay        = g.Sum(x => x.Pay        ?? 0m),
                    NonPay     = g.Sum(x => x.Nonpay     ?? 0m),
                    StaffCosts = (decimal)g.Sum(x => x.Cost     ?? 0d),
                    Overhead   = g.Sum(x => x.Overhead   ?? 0m)
                });

            query = (paging.SortBy?.ToLower()) switch
            {
                "month"      => ApplyOrder(query, x => x.Month,      paging.Descending),
                "pay"        => ApplyOrder(query, x => x.Pay,        paging.Descending),
                "nonpay"     => ApplyOrder(query, x => x.NonPay,     paging.Descending),
                "staffcosts" => ApplyOrder(query, x => x.StaffCosts, paging.Descending),
                "overhead"   => ApplyOrder(query, x => x.Overhead,   paging.Descending),
                _            => query.OrderBy(x => x.Month)
            };

            List<PactPayCalc> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<ProjectMonthFinal>> GetMonthlyPactDataAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<ProjectMonthFinal> query = _context.ProjectMonthFinals
                .AsNoTracking()
                .Where(m => m.Project == project && m.Year == year);

            string? search = paging.Search?.ToLower();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Periodname != null && m.Periodname.ToLower().Contains(search));

            query = (paging.SortBy?.ToLower()) switch
            {
                "monthno"      => ApplyOrder(query, m => m.Monthno,      paging.Descending),
                "periodname"   => ApplyOrder(query, m => m.Periodname,   paging.Descending),
                "nonanimals"   => ApplyOrder(query, m => m.Nonanimals,   paging.Descending),
                "animals"      => ApplyOrder(query, m => m.Animals,      paging.Descending),
                "timecosts"    => ApplyOrder(query, m => m.Timecosts,    paging.Descending),
                "transfercosts"=> ApplyOrder(query, m => m.Transfercosts,paging.Descending),
                "totalcost"    => ApplyOrder(query, m => m.Totalcost,    paging.Descending),
                "totalhours"   => ApplyOrder(query, m => m.Totalhours,   paging.Descending),
                "invoices"     => ApplyOrder(query, m => m.Invoices,     paging.Descending),
                "coiw"         => ApplyOrder(query, m => m.Coiw,         paging.Descending),
                _              => query.OrderBy(m => m.Monthno)
            };

            List<ProjectMonthFinal> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<FpsYearTotal?> GetFpsYearTotalsAsync(string project, short year)
        {
            return await (
                from yearTotal in _context.FpsYearTotals.AsNoTracking()
                join projectYear in _context.MyTlkpProjects.AsNoTracking()
                    on new { yearTotal.Parentproject, yearTotal.Year }
                    equals new { projectYear.Parentproject, projectYear.Year }
                where yearTotal.Parentproject == project && yearTotal.Year == year
                select new FpsYearTotal
                {
                    Year                 = yearTotal.Year,
                    Parentproject        = yearTotal.Parentproject,
                    Program              = yearTotal.Program,
                    Totaladditionalcosts = yearTotal.Totaladditionalcosts,
                    Totalanimalcosts     = yearTotal.Totalanimalcosts,
                    Totalstaffcosts      = yearTotal.Totalstaffcosts,
                    Totaltestcosts       = yearTotal.Totaltestcosts,
                    Totalcosts           = yearTotal.Totalcosts,
                    Custincome           = projectYear.Custincome ?? 0m,
                    Transferincome       = projectYear.Transferincome ?? 0m,
                    Totalincome          = (projectYear.Custincome ?? 0m) + (projectYear.Transferincome ?? 0m),
                    BudgetCvl            = yearTotal.BudgetCvl,
                    Requiredprofit       = yearTotal.Requiredprofit,
                    Manager              = projectYear.Manager,
                    Customer             = projectYear.Customer,
                    Projectstatus        = projectYear.Projectstatus ?? yearTotal.Projectstatus,
                    Pvsincome            = projectYear.Pvsincome ?? yearTotal.Pvsincome,
                    Plancaseworkdebit    = projectYear.Plancaseworkdebit ?? yearTotal.Plancaseworkdebit,
                    Totalpaycosts        = yearTotal.Totalpaycosts
                })
                .FirstOrDefaultAsync();
        }
    }
}
