using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateTimeCostCalcsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateTimeCostCalcs";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Strict SQL alignment: join order, null handling, all fields, calculation order.
        var rawRows = await (
            from pc in db.RsTblkpProfitCentre.AsNoTracking()
            join pcg in db.RsProfitCentreGrade.AsNoTracking() on pc.ProfitCentre equals pcg.ProfitCentre
            join wgg in db.RsWorkGroupGrade.AsNoTracking() on pcg.PcGrade equals wgg.ProfitCentreGrade
            join vps in db.RsVpactTblStaff.AsNoTracking() on wgg.WgGrade equals vps.WorkGroupGrade
            join mt in db.RsMonthlyTime.AsNoTracking() on vps.PactId equals mt.PactStaffId
            join tcv in db.RsTimeCodeValid.AsNoTracking() on new { mt.WorkGroup, mt.TimeCode, mt.ParentProject } equals new { tcv.WorkGroup, tcv.TimeCode, tcv.ParentProject }
            join p in db.RsTlkpProject.AsNoTracking() on tcv.ParentProject equals p.ParentProject
            where p.FpsYear == context.FpsYear
            join prg in db.RsTlkpProgram.AsNoTracking() on p.Program equals prg.ProgramNo
            select new
            {
                WorkGroup = wgg.WorkGroup ?? string.Empty,
                JobCode = mt.TimeCode,
                Project = tcv.ParentProject,
                Month = mt.Month,
                StaffId = vps.PactId,
                GradeCode = wgg.GradeCode ?? string.Empty,
                Name = vps.Name ?? string.Empty,
                IsCharge = prg.SectorName == "Charge",
                Hours = mt.Hours ?? 0d,
                IsDefraProject = p.IsDefraProject,
                ChargeRate = pcg.ChargeRate,
                DefraChargeRate = pcg.DefraChargeRate,
                Division = pc.Division ?? string.Empty,
                PayRate = pcg.PayRate,
                Npr = pcg.Npr,
                Ohr = pcg.Ohr,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsTimeCostCalcsTable
        {
            WorkGroup = r.WorkGroup,
            JobCode = r.JobCode,
            Project = r.Project,
            Month = r.Month,
            StaffId = r.StaffId,
            GradeCode = r.GradeCode,
            Name = r.Name,
            ChargeRate = (r.IsDefraProject ?? 0) == 0 ? (r.ChargeRate ?? 0m) : (r.DefraChargeRate ?? 0m),
            Class = r.IsCharge ? "Charge" : "Free",
            Time = r.Hours,
            Cost = r.IsCharge ? r.Hours * (double)((r.IsDefraProject ?? 0) == 0 ? (r.ChargeRate ?? 0m) : (r.DefraChargeRate ?? 0m)) : 0d,
            Division = r.Division,
            Pay = (decimal)r.Hours * (r.PayRate ?? 0m),
            NonPay = (decimal)r.Hours * (r.Npr ?? 0m),
            Overhead = (decimal)r.Hours * (r.Ohr ?? 0m),
            FpsYear = r.FpsYear
        }).ToList();

        await db.RsTimeCostCalcs.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
