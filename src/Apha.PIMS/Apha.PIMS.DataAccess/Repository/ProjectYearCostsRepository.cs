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

        public async Task<PagedData<MyProjSubContract>> GetAdditionalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyProjSubContract> query = _context.MyProjSubcontracts
                .AsNoTracking()
                .Where(s => s.Project == project
                         && s.Year == year
                         && !AnimalAcctCodes.Contains(s.Acctcode));

            query = ApplyActualsSearch(query, paging.Search);
            query = ApplyActualsSorting(query, paging.SortBy, paging.Descending);
            List<MyProjSubContract> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<MyTblAdditionalCosts>> GetAdditionalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyTblAdditionalCosts> query = _context.MyTblAdditionalCosts
                .AsNoTracking()
                .Where(s => s.Jobcode == project && s.Year == year);

            query = ApplyPlansSearch(query, paging.Search);
            query = ApplyPlansSorting(query, paging.SortBy, paging.Descending);
            List<MyTblAdditionalCosts> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<MyProjSubContract>> GetAnimalActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyProjSubContract> query = _context.MyProjSubcontracts
                .AsNoTracking()
                .Where(s => s.Project == project
                         && s.Year == year
                         && AnimalAcctCodes.Contains(s.Acctcode));

            query = ApplyActualsSearch(query, paging.Search);
            query = ApplyActualsSorting(query, paging.SortBy, paging.Descending);
            List<MyProjSubContract> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<MyProjectAnimalPlan>> GetAnimalPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyProjectAnimalPlan> query = _context.MyProjectAnimalPlans
                .AsNoTrackingWithIdentityResolution()
                .Where(s => s.Parentproject == project && s.Year == year);

            query = ApplyAnimalPlanSearch(query, paging.Search);
            query = ApplyAnimalPlanSorting(query, paging.SortBy, paging.Descending);
            List<MyProjectAnimalPlan> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        private static IQueryable<MyProjSubContract> ApplyActualsSearch(
            IQueryable<MyProjSubContract> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Acctcode    != null && x.Acctcode.ToLower().Contains(s)) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                (x.Supplier    != null && x.Supplier.ToLower().Contains(s)));
        }

        private static IQueryable<MyTblAdditionalCosts> ApplyPlansSearch(
            IQueryable<MyTblAdditionalCosts> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Account     != null && x.Account.ToLower().Contains(s)) ||
                (x.Description != null && x.Description.ToLower().Contains(s)) ||
                (x.Supplier    != null && x.Supplier.ToLower().Contains(s)));
        }

        private static IQueryable<MyProjSubContract> ApplyActualsSorting(
            IQueryable<MyProjSubContract> query, string? sortBy, bool descending)
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

        private static IQueryable<MyTblAdditionalCosts> ApplyPlansSorting(
            IQueryable<MyTblAdditionalCosts> query, string? sortBy, bool descending)
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

        private static IQueryable<MyProjectAnimalPlan> ApplyAnimalPlanSearch(
            IQueryable<MyProjectAnimalPlan> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x => x.Animaltype != null && x.Animaltype.ToLower().Contains(s));
        }

        private static IQueryable<MyProjectAnimalPlan> ApplyAnimalPlanSorting(
            IQueryable<MyProjectAnimalPlan> query, string? sortBy, bool descending)
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

        public async Task<PagedData<MyTlkpTestReqmt>> GetTestPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyTlkpTestReqmt> query = _context.MyTlkpTestReqmts
                .AsNoTracking()
                .Where(t => t.Buyer == project && t.Year == year);

            query = ApplyTestPlanSearch(query, paging.Search);
            query = ApplyTestPlanSorting(query, paging.SortBy, paging.Descending);
            List<MyTlkpTestReqmt> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<(MyMonthlyOutput Output, MyTlkpTestReqmt Reqmt)>> GetTestActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            List<(MyMonthlyOutput Output, MyTlkpTestReqmt Reqmt)> joined = await (
                from mo in _context.MyMonthlyOutputs.AsNoTracking()
                join tr in _context.MyTlkpTestReqmts.AsNoTracking()
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

        private static IQueryable<MyTlkpTestReqmt> ApplyTestPlanSearch(
            IQueryable<MyTlkpTestReqmt> query, string? search)
        {
            if (string.IsNullOrWhiteSpace(search)) return query;
            string s = search.ToLower();
            return query.Where(x =>
                (x.Testcode != null && x.Testcode.ToLower().Contains(s)) ||
                (x.Buyer    != null && x.Buyer.ToLower().Contains(s)));
        }

        private static IQueryable<MyTlkpTestReqmt> ApplyTestPlanSorting(
            IQueryable<MyTlkpTestReqmt> query, string? sortBy, bool descending)
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

        public async Task<PagedData<MyProjectStaffPlan>> GetStaffPlansAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyProjectStaffPlan> query = _context.MyProjectStaffPlans
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

            List<MyProjectStaffPlan> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<PagedData<MyTimeCostCalcs>> GetStaffActualsAsync(
            string project, short year, PaginationParameters<string> paging)
        {
            IQueryable<MyTimeCostCalcs> query = _context.MyTimeCostCalcs
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

            List<MyTimeCostCalcs> all = await query.ToListAsync();
            return ApplyPaging(all, paging.Page, paging.PageSize);
        }

        public async Task<Projects?> GetProjectYearDetailsAsync(string project, short year)
        {
            return await _context.MyTlkpProjects
                .AsNoTracking()
                .Where(p => p.Parentproject == project && p.Year == year)
                .FirstOrDefaultAsync();
        }
    }
}
