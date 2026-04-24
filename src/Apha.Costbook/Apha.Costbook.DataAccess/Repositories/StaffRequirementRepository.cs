using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class StaffRequirementRepository : IStaffRequirementRepository
{
    private readonly CostbookDbContext _context;
    private readonly IFPSYearContext _fpsYearContext;

    public StaffRequirementRepository(CostbookDbContext context, IFPSYearContext fpsYearContext)
    {
        _context = context;
        _fpsYearContext = fpsYearContext;
    }

    /// <summary>
    /// LINQ equivalent of MS Access qryStaffReqGrade.
    /// WorkGroupGrades uses IgnoreQueryFilters() + explicit FpsYear in the join condition
    /// to guarantee a true LEFT JOIN — preventing EF Core from converting it to an INNER JOIN
    /// when the query filter (FpsYear == current) is applied as a WHERE clause.
    /// </summary>
    public async Task<IEnumerable<StaffRequirementDetailView>> GetByProjectYearAsync(string project, int year)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var fpsYear = _fpsYearContext.FPSYear;

        return await (
            from sr in _context.StaffRequirements.AsNoTracking()

            // LEFT JOIN WorkGroupGrade — IgnoreQueryFilters() + explicit FpsYear in ON clause
            // prevents EF Core from emitting the filter as a WHERE, which would make it INNER JOIN
            join wgg in _context.WorkGroupGrades.AsNoTracking().IgnoreQueryFilters()
                on new { sr.WgGrade, FpsYear = fpsYear }
                equals new { wgg.WgGrade, wgg.FpsYear } into wggJoin
            from wgg in wggJoin.DefaultIfEmpty()

            // LEFT JOIN Project
            join proj in _context.Projects.AsNoTracking()
                on sr.Project equals proj.ProjectId into projJoin
            from proj in projJoin.DefaultIfEmpty()

            // LEFT JOIN EuGradeConversion — DLookup equivalent on GradeCode
            join eu in _context.EuGradeConversions.AsNoTracking()
                on wgg.GradeCode equals eu.VlaGrade into euJoin
            from eu in euJoin.DefaultIfEmpty()

            where sr.Project == decodedProject && sr.Year == year
            orderby sr.WgGrade
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
                WorkGroup    = wgg != null ? wgg.WorkGroup : null,
                GradeCode    = wgg != null ? wgg.GradeCode : null,
                Programme    = proj != null ? proj.Programme : null,
                EuroConvRate = proj != null ? proj.Euroconvrate : null,
                EuGrade      = eu != null ? eu.EuGrade : null
            }
        ).ToListAsync();
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
}
