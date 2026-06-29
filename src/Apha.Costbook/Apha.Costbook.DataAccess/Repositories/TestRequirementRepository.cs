using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class TestRequirementRepository : RepositoryBase, ITestRequirementRepository
{
    private readonly IFPSYearContext _fpsYearContext;
    private readonly ISettingsRepository _settingsRepo;
    private readonly IProjectRepository _projectRepo;

    public TestRequirementRepository(CostbookDbContext context, IFPSYearContext fpsYearContext, ISettingsRepository settingsRepo, IProjectRepository projectRepo)
        : base(context)
    {
        _fpsYearContext = fpsYearContext;
        _settingsRepo = settingsRepo;
        _projectRepo = projectRepo;
    }

    public async Task<PagedData<TestRequirementDetailView>> GetTestRequirementsByProjectYearAsync(
        string project, int year, PaginationParameters<string> query)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var fpsYear = _fpsYearContext.FPSYear;

        var baseQuery =
            from tr in _context.TestRequirements.AsNoTracking()

            join test in _context.FpsTestorProducts.AsNoTracking().IgnoreQueryFilters()
                on new { ItemCode = tr.TestCode, FpsYear = fpsYear }
                equals new { test.ItemCode, test.FpsYear } into testJoin
            from test in testJoin.DefaultIfEmpty()

            join proj in _context.Projects.AsNoTracking()
                on tr.Project equals proj.ProjectId into projJoin
            from proj in projJoin.DefaultIfEmpty()

            where tr.Project == decodedProject && tr.Year == year
            select new TestRequirementDetailView
            {
                Project         = tr.Project,
                Year            = tr.Year,
                TestCode        = tr.TestCode,
                UnitPrice       = tr.UnitPrice,
                NumberOfTests   = tr.NumberOfTests,
                TestCost        = tr.UnitPrice * tr.NumberOfTests,
                TestDescription = test != null ? test.ItemDescription : null,
                Programme       = proj != null ? proj.Programme : null,
                EuroConvRate    = proj != null ? proj.Euroconvrate : null
            };

        baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);
       
        return await ApplyPaging(baseQuery, query.Page, query.PageSize);
    }

    public async Task<TestRequirement> AddTestRequirementAsync(TestRequirement testRequirement)
    {
        testRequirement.Project= HttpUtility.UrlDecode(testRequirement.Project);

        _context.TestRequirements.Add(testRequirement);
        await _context.SaveChangesAsync();

        return testRequirement;
    }

    public async Task<TestRequirement> UpdateTestRequirementAsync(TestRequirement testRequirement)
    {
        testRequirement.Project = HttpUtility.UrlDecode(testRequirement.Project);

        _context.TestRequirements.Update(testRequirement);
        await _context.SaveChangesAsync();

        return testRequirement;
    }

    public async Task<bool> DeleteTestRequirementAsync(string project, int year, string testCode)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var decodedTestCode = HttpUtility.UrlDecode(testCode);

        var deleted = await _context.TestRequirements
            .Where(t => t.Project == decodedProject && t.Year == year && t.TestCode == decodedTestCode)
            .ExecuteDeleteAsync();

        return deleted > 0;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IQueryable<TestRequirementDetailView> ApplySorting(
        IQueryable<TestRequirementDetailView> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(t => t.TestCode);

        return sortBy.ToLower() switch
        {
            "testcode"      => ApplyOrder(query, t => t.TestCode, descending),
            "testdescription" => ApplyOrder(query, t => t.TestDescription, descending),
            "numberoftests" => ApplyOrder(query, t => t.NumberOfTests, descending),
            "unitprice"     => ApplyOrder(query, t => t.UnitPrice, descending),
            "testcost"      => ApplyOrder(query, t => t.TestCost, descending),
            _               => query.OrderBy(t => t.TestCode)
        };
    }

    private static IQueryable<TestRequirementDetailView> ApplyOrder<TKey>(
        IQueryable<TestRequirementDetailView> query,
        System.Linq.Expressions.Expression<Func<TestRequirementDetailView, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }

    public async Task<IEnumerable<TestCodeLookup>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra)
    {
        var decodedId = HttpUtility.UrlDecode(projectId);

        var currentYearSetting = await _settingsRepo.GetSettingValueByIdAsync("CurrentYear");

        if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
        {
            throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
        }

        var results = await _context.FpsTestorProducts
            .AsNoTracking()
            .Where(t => !t.ItemCode.StartsWith("PA") && !t.ItemCode.EndsWith("ND"))
            .OrderBy(t => t.ItemCode)
            .Select(t => new
            {
                t.ItemCode,
                t.ItemDescription,
                UnitPrice = isDefra ? t.DefraUnitPrice : t.UnitPriceVla
            })
            .ToListAsync();

        double inflationFactor = await _projectRepo.GetInflationFactorAsync("InflationTests", decodedId, year, fyear);

        return results.Select(t => new TestCodeLookup
        {
            ItemCode = t.ItemCode,
            ItemDescription = t.ItemDescription,
            UnitPrice = t.UnitPrice,
            UnitPriceWithInflamation = t.UnitPrice.HasValue
                ? (decimal?)(Convert.ToDouble(t.UnitPrice.Value) * inflationFactor)
                : null
        });
    }
}
