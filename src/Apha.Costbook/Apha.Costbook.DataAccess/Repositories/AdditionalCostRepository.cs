using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class AdditionalCostRepository : IAdditionalCostRepository
{
    private readonly CostbookDbContext _context;

    public AdditionalCostRepository(CostbookDbContext context) => _context = context;

    public async Task<IEnumerable<AdditionalCostDetailView>> GetAdditionalCostsByProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        return await _context.AdditionalCosts
            .AsNoTracking()
            .Where(ac => ac.Project == decodedProject && ac.Year == year)
            .GroupJoin(
                _context.Projects.AsNoTracking(),
                ac => ac.Project,
                p => p.ProjectId,
                (ac, projJoin) => new { ac, projJoin })
            .SelectMany(
                x => x.projJoin.DefaultIfEmpty(),
                (x, p) => new AdditionalCostDetailView
                {
                    AcIdentity   = x.ac.AcIdentity,
                    Project      = x.ac.Project,
                    Year         = x.ac.Year,
                    Description  = x.ac.Description,
                    ItemCost     = x.ac.ItemCost,
                    CostEntered  = x.ac.CostEntered,
                    AccountCat   = x.ac.AccountCat,
                    Freq         = x.ac.Freq,
                    Programme    = p != null ? p.Programme : null,
                    EuroConvRate = p != null ? p.Euroconvrate : null
                })
            .Distinct()
            .OrderBy(ac => ac.Description)
            .ToListAsync();
    }

    public async Task<AdditionalCost> AddAdditionalCostAsync(AdditionalCost additionalCost)
    {
        additionalCost.Project = HttpUtility.UrlDecode(additionalCost.Project);

        _context.AdditionalCosts.Add(additionalCost);
        await _context.SaveChangesAsync();

        return additionalCost;
    }

    public async Task<AdditionalCost> UpdateAdditionalCostAsync(AdditionalCost additionalCost)
    {
        additionalCost.Project = HttpUtility.UrlDecode(additionalCost.Project);

        _context.AdditionalCosts.Update(additionalCost);
        await _context.SaveChangesAsync();

        return additionalCost;
    }

    public async Task<bool> DeleteAdditionalCostAsync(int acIdentity)
    {
        var deleted = await _context.AdditionalCosts
            .Where(ac => ac.AcIdentity == acIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    public async Task<IEnumerable<AccountCategoryLookup>> GetProjectSpecificAccountCategoriesAsync()
    {
        var results=(await _context.FpsAccountCategories
                    .AsNoTracking()
                    .Where(static ac => ac.ProjectSpecific == -1)
                    .Join(
                        _context.AccountGroups.AsNoTracking(),
                        ac => ac.Csg7Group,
                        ag => ag.Csg7group,
                        (ac, ag) => new { ac.AccShortName, UseInflation = ag.Useinflation == true })
                    .OrderBy(x => x.AccShortName)
                    .ToListAsync())
                .Select(x => new AccountCategoryLookup(x.AccShortName, x.UseInflation));

        return results;
    }
}
