using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateTimeCostCalcsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateTimeCostCalcs";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;

        var rows = await (
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
            select new RsTimeCostCalcsTable
            {
                WorkGroup = wgg.WorkGroup ?? string.Empty,
                JobCode = mt.TimeCode,
                Project = tcv.ParentProject,
                Month = mt.Month,
                StaffId = vps.PactId,
                GradeCode = wgg.GradeCode,
                Name = vps.Name,
                ChargeRate = (p.IsDefraProject ?? 0) == 0 ? pcg.ChargeRate : pcg.DefraChargeRate,
                Class = prg.SectorName == "Charge" ? "Charge" : "Free",
                Time = mt.Hours,
                Cost = (prg.SectorName == "Charge" ? (mt.Hours ?? 0d) : 0d)
                    * ((double?)((p.IsDefraProject ?? 0) == 0 ? pcg.ChargeRate : pcg.DefraChargeRate) ?? 0d),
                Division = pc.Division,
                Pay = (mt.Hours ?? 0d) * ((double?)pcg.PayRate ?? 0d),
                NonPay = (mt.Hours ?? 0d) * ((double?)pcg.Npr ?? 0d),
                Overhead = (mt.Hours ?? 0d) * ((double?)pcg.Ohr ?? 0d),
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        await db.RsTimeCostCalcs.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
