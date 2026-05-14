using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateTimeCostCalcsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateTimeCostCalcs";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        // Read nullable money/decimal values first, then compute money fields in decimal
        // to avoid provider SQL casts from money -> double precision.
        var rawRows = await (
            from pc in db.RsTblkpProfitCentre.AsNoTracking()
            join pcg in db.RsProfitCentreGrade.AsNoTracking()
                on pc.ProfitCentre equals pcg.ProfitCentre
            join wgg in db.RsWorkGroupGrade.AsNoTracking()
                on pcg.PcGrade equals wgg.ProfitCentreGrade
            join vps in db.RsVpactTblStaff.AsNoTracking()
                on wgg.WgGrade equals vps.WorkGroupGrade
            join mt in db.RsMonthlyTime.AsNoTracking()
                on vps.PactId equals mt.PactStaffId
            join tcv in db.RsTimeCodeValid.AsNoTracking()
                on new { mt.WorkGroup, mt.TimeCode, mt.ParentProject }
                equals new { tcv.WorkGroup, tcv.TimeCode, tcv.ParentProject }
            join p in db.RsTlkpProject.AsNoTracking()
                on tcv.ParentProject equals p.ParentProject
            join prg in db.RsTlkpProgram.AsNoTracking()
                on p.Program equals prg.ProgramNo
            select new
            {
                WorkGroup = wgg.WorkGroup,
                JobCode = mt.TimeCode,
                Project = tcv.ParentProject,
                Month = mt.Month,
                StaffId = vps.PactId,
                GradeCode = wgg.GradeCode,
                Name = vps.Name,
                IsCharge = prg.SectorName == "Charge",
                Hours = mt.Hours,
                ChargeRate = (p.IsDefraProject ?? 0) == 0 ? pcg.ChargeRate : pcg.DefraChargeRate,
                Division = pc.Division,
                PayRate = pcg.PayRate,
                Npr = pcg.Npr,
                Ohr = pcg.Ohr,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r =>
        {
            var hours = r.Hours ?? 0d;
            var hoursDecimal = (decimal)hours;
            var chargeRateDouble = (double)(r.ChargeRate ?? 0m);
            var chargeRate = r.ChargeRate ?? 0m;
            var payRate = r.PayRate ?? 0m;
            var npr = r.Npr ?? 0m;
            var ohr = r.Ohr ?? 0m;

            return new RsTimeCostCalcsTable
            {
                WorkGroup = r.WorkGroup ?? string.Empty,
                JobCode = r.JobCode,
                Project = r.Project,
                Month = r.Month,
                StaffId = r.StaffId,
                GradeCode = r.GradeCode,
                Name = r.Name,
                ChargeRate = r.ChargeRate,
                Class = r.IsCharge ? "Charge" : "Free",
                Time = r.Hours,
                Cost = r.IsCharge ? hours * chargeRateDouble : 0d,
                Division = r.Division,
                Pay = hoursDecimal * payRate,
                NonPay = hoursDecimal * npr,
                Overhead = hoursDecimal * ohr,
                FpsYear = r.FpsYear
            };
        }).ToList();

        await db.RsTimeCostCalcs.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
