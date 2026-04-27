using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class TestRequirementRepository : ITestRequirementRepository
{
    private readonly CostbookDbContext _context;
    private readonly IFPSYearContext _fpsYearContext;

    public TestRequirementRepository(CostbookDbContext context, IFPSYearContext fpsYearContext)
    {
        _context = context;
        _fpsYearContext = fpsYearContext;
    }

    public async Task<IEnumerable<TestRequirementDetailView>> GetTestRequirementsByProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var fpsYear = _fpsYearContext.FPSYear;

        var query =
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
                Project       = tr.Project,
                Year          = tr.Year,
                TestCode      = tr.TestCode,
                UnitPrice     = tr.UnitPrice,
                NumberOfTests = tr.NumberOfTests,
                TestCost      = tr.UnitPrice * tr.NumberOfTests,
                TestDescription = test != null ? test.ItemDescription : null,
                Programme     = proj != null ? proj.Programme : null,
                EuroConvRate  = proj != null ? proj.Euroconvrate : null
            };

        return await query.OrderBy(t => t.TestCode).ToListAsync();
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

    public async Task<IEnumerable<TestCodeLookup>> GetTestCodeLookupsAsync(bool isDefra)
        => (await _context.FpsTestorProducts
                .AsNoTracking()
                .Where(t => !t.ItemCode.StartsWith("PA") && !t.ItemCode.EndsWith("ND"))
                .OrderBy(t => t.ItemCode)
                .Select(t => new
                {
                    t.ItemCode,
                    t.ItemDescription,
                    UnitPrice = isDefra ? t.DefraUnitPrice : t.UnitPriceVla
                })
                .ToListAsync())
            .Select(t => new TestCodeLookup(t.ItemCode, t.ItemDescription, t.UnitPrice));
}
