using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories;

public class TestRequirementRepository : ITestRequirementRepository
{
    private readonly CostbookDbContext _context;

    public TestRequirementRepository(CostbookDbContext context) => _context = context;

    public async Task<IEnumerable<TestRequirement>> GetByProjectYearAsync(string project, int year)
        => await _context.TestRequirements
            .AsNoTracking()
            .Where(t => t.Project == project && t.Year == year)
            .OrderBy(t => t.TestCode)
            .ToListAsync();

    public async Task<TestRequirement> AddAsync(TestRequirement testRequirement)
    {
        _context.TestRequirements.Add(testRequirement);
        await _context.SaveChangesAsync();
        return testRequirement;
    }

    public async Task<TestRequirement> UpdateAsync(TestRequirement testRequirement)
    {
        _context.TestRequirements.Update(testRequirement);
        await _context.SaveChangesAsync();
        return testRequirement;
    }

    public async Task<bool> DeleteAsync(string project, int year, string testCode)
    {
        var deleted = await _context.TestRequirements
            .Where(t => t.Project == project && t.Year == year && t.TestCode == testCode)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }
}
