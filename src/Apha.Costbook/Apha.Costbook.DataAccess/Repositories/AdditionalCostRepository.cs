using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Repositories;

public class AdditionalCostRepository : IAdditionalCostRepository
{
    private readonly CostbookDbContext _context;

    public AdditionalCostRepository(CostbookDbContext context) => _context = context;

    public async Task<IEnumerable<AdditionalCost>> GetByProjectYearAsync(string project, int year)
        => await _context.AdditionalCosts
            .AsNoTracking()
            .Where(ac => ac.Project == project && ac.Year == year)
            .OrderBy(ac => ac.Description)
            .ToListAsync();

    public async Task<AdditionalCost> AddAsync(AdditionalCost additionalCost)
    {
        _context.AdditionalCosts.Add(additionalCost);
        await _context.SaveChangesAsync();
        return additionalCost;
    }

    public async Task<AdditionalCost> UpdateAsync(AdditionalCost additionalCost)
    {
        _context.AdditionalCosts.Update(additionalCost);
        await _context.SaveChangesAsync();
        return additionalCost;
    }

    public async Task<bool> DeleteAsync(int acIdentity)
    {
        var deleted = await _context.AdditionalCosts
            .Where(ac => ac.AcIdentity == acIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<IEnumerable<AccountCategoryLookup>> GetProjectSpecificAccountCategoriesAsync()
        => await _context.FpsAccountCategories
            .AsNoTracking()
            .Where(ac => ac.ProjectSpecific == 1)
            .Join(
                _context.AccountGroups.AsNoTracking(),
                ac => ac.Csg7Group,
                ag => ag.Csg7group,
                (ac, ag) => new AccountCategoryLookup(ac.AccShortName, ag.Useinflation == true))
            .OrderBy(x => x.AccShortName)
            .ToListAsync();
}
