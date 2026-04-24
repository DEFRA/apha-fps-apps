using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class StaffRequirementRepository : RepositoryBase<StaffRequirement>, IStaffRequirementRepository
{
    private readonly IFPSYearContext _fpsYearContext;

    public StaffRequirementRepository(CostbookDbContext context, IFPSYearContext fpsYearContext)
        : base(context)
    {
        _fpsYearContext = fpsYearContext;
    }

    /// <summary>
    /// LINQ equivalent of MS Access qryStaffReqGrade — with server-side sorting and paging.
    /// </summary>
    public async Task<PagedData<StaffRequirementDetailView>> GetByProjectYearAsync(
        string project, int year, PaginationParameters<string> query)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var fpsYear = _fpsYearContext.FPSYear;

        var baseQuery =
            from sr in _context.StaffRequirements.AsNoTracking()

            join wgg in _context.WorkGroupGrades.AsNoTracking().IgnoreQueryFilters()
                on new { sr.WgGrade, FpsYear = fpsYear }
                equals new { wgg.WgGrade, wgg.FpsYear } into wggJoin
            from wgg in wggJoin.DefaultIfEmpty()

            join proj in _context.Projects.AsNoTracking()
                on sr.Project equals proj.ProjectId into projJoin
            from proj in projJoin.DefaultIfEmpty()

            join eu in _context.EuGradeConversions.AsNoTracking()
                on wgg.GradeCode equals eu.VlaGrade into euJoin
            from eu in euJoin.DefaultIfEmpty()

            where sr.Project == decodedProject && sr.Year == year
            select new StaffRequirementDetailView
            {
                SrIdentity   = sr.SrIdentity,
                Project      = sr.Project,
                Year         = sr.Year,
                WgGrade      = sr.WgGrade,
                Name         = sr.Name,
                Nohours      = sr.Nohours,
                Nodays       = sr.Nodays,
                Chargerate   = sr.Chargerate,
                Payrate      = sr.Payrate,
                Npr          = sr.Npr,
                Ohr          = sr.Ohr,
                WorkGroup    = wgg != null ? wgg.WorkGroup  : null,
                GradeCode    = wgg != null ? wgg.GradeCode  : null,
                Programme    = proj != null ? proj.Programme : null,
                EuroConvRate = proj != null ? proj.Euroconvrate : null,
                EuGrade      = eu  != null ? eu.EuGrade     : null
            };

        baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);

        List<StaffRequirementDetailView> result = await baseQuery.ToListAsync();
        return ApplyPaging(result, query.Page, query.PageSize);
    }

    public async Task<StaffRequirement> AddAsync(StaffRequirement staffRequirement)
    {
        _context.StaffRequirements.Add(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<StaffRequirement> UpdateAsync(StaffRequirement staffRequirement)
    {
        _context.StaffRequirements.Update(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<bool> DeleteAsync(int srIdentity)
    {
        var deleted = await _context.StaffRequirements
            .Where(s => s.SrIdentity == srIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static IQueryable<StaffRequirementDetailView> ApplySorting(
        IQueryable<StaffRequirementDetailView> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(c => c.WgGrade);

        return sortBy.ToLower() switch
        {
            "sridentity" => descending ? query.OrderByDescending(c => c.SrIdentity) : query.OrderBy(c => c.SrIdentity),
            "project"    => descending ? query.OrderByDescending(c => c.Project)    : query.OrderBy(c => c.Project),
            "year"       => descending ? query.OrderByDescending(c => c.Year)       : query.OrderBy(c => c.Year),
            "wggrade"    => descending ? query.OrderByDescending(c => c.WgGrade)    : query.OrderBy(c => c.WgGrade),
            "name"       => descending ? query.OrderByDescending(c => c.Name)       : query.OrderBy(c => c.Name),
            "nohours"    => descending ? query.OrderByDescending(c => c.Nohours)    : query.OrderBy(c => c.Nohours),
            "chargerate" => descending ? query.OrderByDescending(c => c.Chargerate) : query.OrderBy(c => c.Chargerate),
            _            => query.OrderBy(c => c.WgGrade)
        };
    }
}
