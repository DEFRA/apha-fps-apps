using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class AdditionalCostRepository : RepositoryBase<AdditionalCost>, IAdditionalCostRepository
{
    public AdditionalCostRepository(CostbookDbContext context) : base(context) { }

    public async Task<PagedData<AdditionalCostDetailView>> GetAdditionalCostsByProjectYearAsync(
        string project, int year, PaginationParameters<string> query)
    {
        var decodedProject = HttpUtility.UrlDecode(project);

        var baseQuery = _context.AdditionalCosts
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
            .Distinct();

        baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);

        var result = await baseQuery.ToListAsync();
        return ApplyPaging(result, query.Page, query.PageSize);
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

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IQueryable<AdditionalCostDetailView> ApplySorting(
        IQueryable<AdditionalCostDetailView> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(ac => ac.Description);

        return sortBy.ToLower() switch
        {
            "acidentity"  => ApplyOrder(query, ac => ac.AcIdentity, descending),
            "project"     => ApplyOrder(query, ac => ac.Project, descending),
            "year"        => ApplyOrder(query, ac => ac.Year, descending),
            "description" => ApplyOrder(query, ac => ac.Description, descending),
            "itemcost"    => ApplyOrder(query, ac => ac.ItemCost, descending),
            "costentered" => ApplyOrder(query, ac => ac.CostEntered, descending),
            "accountcat"  => ApplyOrder(query, ac => ac.AccountCat, descending),
            "freq"        => ApplyOrder(query, ac => ac.Freq, descending),
            _             => query.OrderBy(ac => ac.Description)
        };
    }

    private static IQueryable<AdditionalCostDetailView> ApplyOrder<TKey>(
        IQueryable<AdditionalCostDetailView> query,
        System.Linq.Expressions.Expression<Func<AdditionalCostDetailView, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
