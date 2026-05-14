using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

internal sealed class CreateFpsTotalsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "CreateFpsTotals";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        /*
        Source: docs/database/dbscript/recreatesummaries/sp_RecreateSummaries_child_procedures.sql

        CREATE procEDURE [dbo].[sp_createFPSTotals] AS
        INSERT INTO FPSYearTotals
        SELECT DISTINCT
            tlkpProject.ParentProject,
            tlkpProject.Program,

            CASE
                WHEN qryTotalAdditionalCosts.TotalAdditionalCosts IS NULL THEN
                    0
                ELSE
                    qryTotalAdditionalCosts.TotalAdditionalCosts
                END AS TotalAdditionalCosts,
            CASE
                WHEN qryTotalAnimalCosts.TotalAnimalCosts IS NULL THEN
                    0
                ELSE
                    qryTotalAnimalCosts.TotalAnimalCosts
                END AS TotalAnimalCosts,
            CASE
                WHEN qryTotalStaffCosts.TotalStaffCosts IS NULL THEN
                    0
                ELSE
                    qryTotalStaffCosts.TotalStaffCosts
                END AS TotalStaffCosts,
            CASE
                WHEN qryTotalTestCosts.TotalTestCosts IS NULL THEN
                    0
                ELSE
                    qryTotalTestCosts.TotalTestCosts
                END AS TotalTestCosts,
            CASE
                WHEN qryTotalAdditionalCosts.TotalAdditionalCosts IS NULL THEN
                    0
                ELSE
                    qryTotalAdditionalCosts.TotalAdditionalCosts
                END +
            CASE
                WHEN qryTotalAnimalCosts.TotalAnimalCosts IS NULL THEN
                    0
                ELSE
                    qryTotalAnimalCosts.TotalAnimalCosts
                END  +
            CASE
                WHEN qryTotalStaffCosts.TotalStaffCosts IS NULL THEN
                    0
                ELSE
                    qryTotalStaffCosts.TotalStaffCosts
                END +
            CASE
                WHEN qryTotalTestCosts.TotalTestCosts IS NULL THEN
                    0
                ELSE
                    qryTotalTestCosts.TotalTestCosts
                END +
            CASE
                WHEN tlkpProject.PlanCaseworkDebit IS NULL THEN
                    0
                ELSE
                    tlkpProject.PlanCaseworkDebit
                END AS TotalCosts,
            tlkpProject.CustIncome,
            tlkpProject.TransferIncome,
            custincome + Transferincome AS TotalIncome,
            tlkpProject.Budget_CVL,
            tlkpProject.Profit as RequiredProfit,
            tlkpProject.Manager,
            tlkpProject.Customer,
            tlkpProject.ProjectStatus,
            CASE
                WHEN tlkpProject.PVSIncome IS NULL THEN
                    0
                ELSE
                    tlkpProject.PVSIncome
                END AS PVSIncome,
            CASE
                WHEN tlkpProject.PlanCaseworkDebit IS NULL THEN
                    0
                ELSE
                    tlkpProject.PlanCaseworkDebit
                END AS PlanCaseworkDebit,

            CASE
                WHEN qryTotalStaffCosts.TotalPayCosts IS NULL THEN
                    0
                ELSE
                    qryTotalStaffCosts.TotalPayCosts
                END AS TotalPayCosts

        FROM (((tlkpProject
        LEFT JOIN qryTotalAdditionalCosts ON tlkpProject.ParentProject = qryTotalAdditionalCosts.JobCode)
        LEFT JOIN qryTotalAnimalCosts ON tlkpProject.ParentProject = qryTotalAnimalCosts.JobCode)
        LEFT JOIN qryTotalStaffCosts ON tlkpProject.ParentProject = qryTotalStaffCosts.Jobcode)
        LEFT JOIN qryTotalTestCosts ON tlkpProject.ParentProject = qryTotalTestCosts.JobCode
        */

        var db = context.DbContext;

        // Two-step: fetch raw nullable values first (avoid COALESCE on PostgreSQL money columns),
        // then apply defaults in C#.
        var rawRows = await (
            from p in db.RsTlkpProject.AsNoTracking()
            join add0 in db.RsQryTotalAdditionalCosts.AsNoTracking() on p.ParentProject equals add0.JobCode into add1
            from add in add1.DefaultIfEmpty()
            join ani0 in db.RsQryTotalAnimalCosts.AsNoTracking() on p.ParentProject equals ani0.JobCode into ani1
            from ani in ani1.DefaultIfEmpty()
            join stf0 in db.RsQryTotalStaffCosts.AsNoTracking() on p.ParentProject equals stf0.JobCode into stf1
            from stf in stf1.DefaultIfEmpty()
            join tst0 in db.RsQryTotalTestCosts.AsNoTracking() on p.ParentProject equals tst0.JobCode into tst1
            from tst in tst1.DefaultIfEmpty()
            select new
            {
                ParentProject = p.ParentProject,
                Program = p.Program,
                AddCosts = add.TotalAdditionalCosts,
                AniCosts = ani.TotalAnimalCosts,
                StfCosts = stf.TotalStaffCosts,
                TstCosts = tst.TotalTestCosts,
                StfPayCosts = stf.TotalPayCosts,
                CustIncome = p.CustIncome,
                TransferIncome = p.TransferIncome,
                BudgetCvl = p.BudgetCvl,
                Profit = p.Profit,
                Manager = p.Manager,
                Customer = p.Customer,
                ProjectStatus = p.ProjectStatus,
                PvsIncome = p.PvsIncome,
                PlanCaseworkDebit = p.PlanCaseworkDebit,
                FpsYear = p.FpsYear
            })
            .ToListAsync(cancellationToken);

        var rows = rawRows.Select(r => new RsFpsYearTotalsTable
        {
            ParentProject = r.ParentProject,
            Program = r.Program,
            TotalAdditionalCosts = r.AddCosts ?? 0m,
            TotalAnimalCosts = (double?)(r.AniCosts) ?? 0d,
            TotalStaffCosts = (double?)(r.StfCosts) ?? 0d,
            TotalTestCosts = (double?)(r.TstCosts) ?? 0d,
            TotalCosts =
                ((double?)(r.AddCosts) ?? 0d) +
                ((double?)(r.AniCosts) ?? 0d) +
                ((double?)(r.StfCosts) ?? 0d) +
                ((double?)(r.TstCosts) ?? 0d) +
                ((double?)(r.PlanCaseworkDebit) ?? 0d),
            CustIncome = r.CustIncome,
            TransferIncome = r.TransferIncome,
            TotalIncome = r.CustIncome + r.TransferIncome,
            BudgetCvl = r.BudgetCvl,
            RequiredProfit = r.Profit,
            Manager = r.Manager,
            Customer = r.Customer,
            ProjectStatus = r.ProjectStatus,
            PvsIncome = r.PvsIncome ?? 0m,
            PlanCaseworkDebit = r.PlanCaseworkDebit ?? 0m,
            TotalPayCosts = (double?)(r.StfPayCosts) ?? 0d,
            FpsYear = r.FpsYear
        })
        // Enforce target table key uniqueness (parentproject + fpsyear) before tracking.
        .GroupBy(r => new { r.ParentProject, r.FpsYear })
        .Select(g => g.First())
        .ToList();

        db.ChangeTracker.Clear();
        await db.RsFpsYearTotals.AddRangeAsync(rows, cancellationToken);
        return await db.SaveChangesAsync(cancellationToken);
    }
}
