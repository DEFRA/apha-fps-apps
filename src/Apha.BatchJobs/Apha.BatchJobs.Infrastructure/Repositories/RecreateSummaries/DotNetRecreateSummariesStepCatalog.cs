using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.RecreateSummaries.Steps;

namespace Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

/// <summary>
/// Builds RecreateSummaries steps from in-code SQL bodies, allowing a pure .NET
/// implementation path while keeping the external SQL scripts unchanged.
/// </summary>
internal sealed class DotNetRecreateSummariesStepCatalog : IRecreateSummariesStepCatalog
{
    public string ImplementationName => "DotNet";

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildMandatorySteps(int month, string triggeredBy) =>
    [
        Wrap(new DeleteFpsTotalsStep(StepSql.DeleteFpsTotals)),
        Wrap(new CreateFpsTotalsStep(StepSql.CreateFpsTotals)),
        Wrap(new InsertMissingProjectsStep(StepSql.InsertMissingProjects)),
        Wrap(new DeleteTimeCostCalcsStep(StepSql.DeleteTimeCostCalcs)),
        Wrap(new CreateTimeCostCalcsStep(StepSql.CreateTimeCostCalcs)),
        Wrap(new DeleteProjectMonthCaseworkStep(StepSql.DeleteProjectMonthCasework)),
        Wrap(new CreateProjectMonthCaseworkStep(StepSql.CreateProjectMonthCasework)),
        Wrap(new DeleteProjectMonthFinalStep(StepSql.DeleteProjectMonthFinal)),
        Wrap(new DeleteProjectMonth2Step(StepSql.DeleteProjectMonth2)),
        Wrap(new CreateProjectMonthSingleStep(StepSql.CreateProjectMonthSingle)),
        Wrap(new DeleteProjectMonth3Step(StepSql.DeleteProjectMonth3)),
        Wrap(new CreateProjectMonthCumulativeStep(StepSql.CreateProjectMonthCumulative)),
        Wrap(new CreateProjectMonthFinalStep(StepSql.CreateProjectMonthFinal, month)),
        Wrap(new LogRecreateSummariesStep(StepSql.LogRecreateSummaries, month, triggeredBy)),
    ];

    public IReadOnlyList<IRecreateSummariesExecutionStep> BuildRefreshSteps(int month) =>
    [
        Wrap(new RefreshPeriodMoStep(StepSql.RefreshPeriodMo, month)),
        Wrap(new RefreshPeriodPscStep(StepSql.RefreshPeriodPsc, month)),
        Wrap(new RefreshPeriodTccStep(StepSql.RefreshPeriodTcc, month)),
    ];

    private static IRecreateSummariesExecutionStep Wrap(IRecreateSummariesStep step)
        => new SqlRecreateSummariesExecutionStepAdapter(step);

    private static class StepSql
    {
        internal const string DeleteFpsTotals = "DELETE FROM fps.fpsyeartotals;";

        internal const string CreateFpsTotals = """
            INSERT INTO fps.fpsyeartotals
            (parentproject, program, totaladditionalcosts, totalanimalcosts, totalstaffcosts,
             totaltestcosts, totalcosts, custincome, transferincome, totalincome, budget_cvl,
             requiredprofit, manager, customer, projectstatus, pvsincome, plancaseworkdebit,
             totalpaycosts, fpsyear)
            SELECT DISTINCT
                tlkpproject.parentproject,
                tlkpproject.program,
                CASE
                    WHEN qrytotaladditionalcosts.totaladditionalcosts IS NULL THEN '0'::money
                    ELSE qrytotaladditionalcosts.totaladditionalcosts
                END AS totaladditionalcosts,
                CASE
                    WHEN qrytotalanimalcosts.totalanimalcosts IS NULL THEN 0::double precision
                    ELSE qrytotalanimalcosts.totalanimalcosts::numeric::double precision
                END AS totalanimalcosts,
                CASE
                    WHEN qrytotalstaffcosts.totalstaffcosts IS NULL THEN 0::double precision
                    ELSE qrytotalstaffcosts.totalstaffcosts::numeric::double precision
                END AS totalstaffcosts,
                CASE
                    WHEN qrytotaltestcosts.totaltestcosts IS NULL THEN 0::double precision
                    ELSE qrytotaltestcosts.totaltestcosts::numeric::double precision
                END AS totaltestcosts,
                CASE
                    WHEN qrytotaladditionalcosts.totaladditionalcosts IS NULL THEN 0::double precision
                    ELSE qrytotaladditionalcosts.totaladditionalcosts::numeric::double precision
                END +
                CASE
                    WHEN qrytotalanimalcosts.totalanimalcosts IS NULL THEN 0::double precision
                    ELSE qrytotalanimalcosts.totalanimalcosts::numeric::double precision
                END +
                CASE
                    WHEN qrytotalstaffcosts.totalstaffcosts IS NULL THEN 0::double precision
                    ELSE qrytotalstaffcosts.totalstaffcosts::numeric::double precision
                END +
                CASE
                    WHEN qrytotaltestcosts.totaltestcosts IS NULL THEN 0::double precision
                    ELSE qrytotaltestcosts.totaltestcosts::numeric::double precision
                END +
                CASE
                    WHEN tlkpproject.plancaseworkdebit IS NULL THEN 0::double precision
                    ELSE tlkpproject.plancaseworkdebit::numeric::double precision
                END AS totalcosts,
                tlkpproject.custincome,
                tlkpproject.transferincome,
                custincome + transferincome AS totalincome,
                tlkpproject.budget_cvl,
                tlkpproject.profit AS requiredprofit,
                tlkpproject.manager,
                tlkpproject.customer,
                tlkpproject.projectstatus,
                CASE
                    WHEN tlkpproject.pvsincome IS NULL THEN '0'::money
                    ELSE tlkpproject.pvsincome
                END AS pvsincome,
                CASE
                    WHEN tlkpproject.plancaseworkdebit IS NULL THEN '0'::money
                    ELSE tlkpproject.plancaseworkdebit
                END AS plancaseworkdebit,
                CASE
                    WHEN qrytotalstaffcosts.totalpaycosts IS NULL THEN 0::double precision
                    ELSE qrytotalstaffcosts.totalpaycosts::numeric::double precision
                END AS totalpaycosts,
                tlkpproject.fpsyear
            FROM (((fps.tlkpproject
            LEFT JOIN fps.qrytotaladditionalcosts ON tlkpproject.parentproject = qrytotaladditionalcosts.jobcode)
            LEFT JOIN fps.qrytotalanimalcosts     ON tlkpproject.parentproject = qrytotalanimalcosts.jobcode)
            LEFT JOIN fps.qrytotalstaffcosts      ON tlkpproject.parentproject = qrytotalstaffcosts.jobcode)
            LEFT JOIN fps.qrytotaltestcosts       ON tlkpproject.parentproject = qrytotaltestcosts.jobcode;
            """;

        internal const string InsertMissingProjects = """
            INSERT INTO fps.projectmonth (project, monthno)
            SELECT DISTINCT tlkpproject.parentproject,
                            @month AS monthno
            FROM fps.tlkpproject
            LEFT JOIN fps.projectmonth
                ON  tlkpproject.parentproject = projectmonth.project
                AND @month = projectmonth.monthno
            WHERE projectmonth.project IS NULL
            ORDER BY parentproject;
            """;

        internal const string DeleteTimeCostCalcs = "DELETE FROM fps.timecostcalcs;";

        internal const string CreateTimeCostCalcs = """
            INSERT INTO fps.timecostcalcs (
                workgroup,
                jobcode,
                project,
                month,
                staffid,
                gradecode,
                name,
                chargerate,
                class,
                time,
                cost,
                division,
                pay,
                nonpay,
                overhead,
                fpsyear
            )
            SELECT DISTINCT
                workgroupgrade.workgroup,
                monthlytime.timecode         AS jobcode,
                timecodevalid.parentproject  AS project,
                monthlytime.month,
                vpacttblstaff.pactid         AS staffid,
                workgroupgrade.gradecode,
                vpacttblstaff.name,
                CASE tlkpproject.isdefraproject
                    WHEN 0 THEN profitcentregrade.chargerate
                    ELSE        profitcentregrade.defrachargerate
                END AS chargerate,
                CASE
                    WHEN tlkpprogram.sector_name = 'Charge' THEN 'Charge'
                    ELSE 'Free'
                END AS class,
                monthlytime.hours            AS time,
                (
                    CASE
                        WHEN tlkpprogram.sector_name = 'Charge' THEN hours
                        ELSE 0
                    END *
                    CASE tlkpproject.isdefraproject
                        WHEN 0 THEN profitcentregrade.chargerate
                        ELSE        profitcentregrade.defrachargerate
                    END
                )::numeric::double precision AS cost,
                fps.tblkpprofitcentre.division,
                monthlytime.hours * profitcentregrade.payrate AS pay,
                monthlytime.hours * profitcentregrade.npr     AS nonpay,
                monthlytime.hours * profitcentregrade.ohr     AS overhead,
                tlkpproject.fpsyear
            FROM (((fps.tblkpprofitcentre
                INNER JOIN fps.profitcentregrade
                    ON fps.tblkpprofitcentre.profitcentre = profitcentregrade.profitcentre)
                INNER JOIN fps.workgroupgrade
                    ON profitcentregrade.pcgrade = workgroupgrade.profitcentregrade)
                INNER JOIN (fps.timecodevalid
                INNER JOIN (fps.vpacttblstaff
                INNER JOIN fps.monthlytime
                    ON vpacttblstaff.pactid = monthlytime.pactstaffid)
                    ON (timecodevalid.workgroup      = monthlytime.workgroup)
                    AND (timecodevalid.timecode      = monthlytime.timecode)
                    AND (timecodevalid.parentproject = monthlytime.parentproject))
                    ON workgroupgrade.wggrade = vpacttblstaff.workgroupgrade)
                INNER JOIN fps.tlkpproject  ON timecodevalid.parentproject = tlkpproject.parentproject
                INNER JOIN fps.tlkpprogram  ON tlkpprogram.programno       = tlkpproject.program;
            """;

        internal const string DeleteProjectMonthCasework = "DELETE FROM fps.projectmonthcasework;";

        internal const string CreateProjectMonthCasework = """
            INSERT INTO fps.projectmonthcasework
            SELECT DISTINCT
                qryprojectmonthcw.project,
                qryprojectmonthcw.monthno,
                qryprojectmonthcw.cwdebit::numeric::double precision,
                qryprojectmonthcw.cwcredit::numeric::double precision
            FROM fps.qryprojectmonthcw;
            """;

        internal const string DeleteProjectMonthFinal = "DELETE FROM fps.projectmonthfinal;";
        internal const string DeleteProjectMonth2 = "DELETE FROM fps.projectmonth2;";

        internal const string CreateProjectMonthSingle = """
            INSERT INTO fps.projectmonth2 (
                project,
                monthno,
                costprofile,
                subcontracts,
                animals,
                nonanimal,
                timecosts,
                transfercosts,
                totalcost,
                invoices,
                coiw,
                sumofcostprofile,
                portsales,
                mstonedue,
                due__done,
                ontime,
                totalhours,
                paycosts
            )
            SELECT
                projectmonth.project,
                projectmonth.monthno,
                projectmonth.costprofile,
                CASE WHEN total     IS NULL THEN '0'::money ELSE total::money END AS subcontracts,
                CASE WHEN animals   IS NULL THEN '0'::money ELSE animals::money END AS animals,
                CASE WHEN other     IS NULL THEN '0'::money ELSE other::money END AS nonanimal,
                CASE WHEN sumofcost IS NULL THEN 0::double precision ELSE sumofcost END AS timecosts,
                CASE
                    WHEN sumoftransfercost IS NULL THEN 0::double precision
                    ELSE sumoftransfercost::numeric::double precision
                END AS transfercosts,
                (
                    COALESCE(total, 0::numeric)
                    + COALESCE(sumofcost, 0::double precision)::numeric
                    + COALESCE(sumoftransfercost, '0'::money)::numeric
                )::money AS totalcost,
                CASE WHEN sumofamount1 IS NULL THEN '0'::money ELSE sumofamount1 END AS invoices,
                CASE WHEN workcost     IS NULL THEN '0'::money ELSE workcost     END AS coiw,
                qryjobmonth_totprofile.sumofcostprofile,
                CASE
                    WHEN fee IS NULL THEN 0::double precision
                    ELSE fee::numeric::double precision
                END AS portsales,
                qryjobmonthmilestone.mstonedue,
                qryjobmonthmilestone.due__done,
                qryjobmonthmilestone.ontime,
                CASE WHEN sumofhours   IS NULL THEN 0::double precision ELSE sumofhours END AS totalhours,
                CASE
                    WHEN sumofpayrate IS NULL THEN 0::double precision
                    ELSE sumofpayrate::numeric::double precision
                END AS paycosts
            FROM ((((((fps.projectmonth
            LEFT JOIN fps.qryjobmonth_subcontracts
                ON  projectmonth.monthno  = qryjobmonth_subcontracts.month
                AND projectmonth.project  = qryjobmonth_subcontracts.project)
            LEFT JOIN fps.qryjobmonth_time
                ON  projectmonth.monthno  = qryjobmonth_time.month
                AND projectmonth.project  = qryjobmonth_time.project)
            LEFT JOIN fps.qryjobmonthmilestone
                ON  projectmonth.monthno  = qryjobmonthmilestone.duemonth
                AND projectmonth.project  = qryjobmonthmilestone.project)
            LEFT JOIN fps.qryjobmonth_transferstotal
                ON  projectmonth.monthno  = qryjobmonth_transferstotal.month
                AND projectmonth.project  = qryjobmonth_transferstotal.project)
            LEFT JOIN fps.qryjobmonth_invoices
                ON  projectmonth.monthno  = qryjobmonth_invoices.month
                AND projectmonth.project  = qryjobmonth_invoices.projectparent)
            LEFT JOIN fps.qryjobmonthportfoliosales
                ON  projectmonth.monthno  = qryjobmonthportfoliosales.month
                AND projectmonth.project  = qryjobmonthportfoliosales.planportfolio)
            LEFT JOIN fps.qryjobmonth_totprofile
                ON  projectmonth.project  = qryjobmonth_totprofile.project;
            """;

        internal const string DeleteProjectMonth3 = "DELETE FROM fps.projectmonth3;";

        internal const string CreateProjectMonthCumulative = """
            INSERT INTO fps.projectmonth3 (
                endperiod,
                periodname,
                project,
                cumcost,
                cuminvoices,
                cumcoiw,
                cumportsales,
                cumprofile,
                sumofcostprofile,
                sumofmstonedue,
                sumofdue__done,
                sumofontime,
                cumcwdebit,
                cumcwcredit,
                cumtotalhours,
                cumsubcontracts,
                cumtestcosts,
                cumpaycosts
            )
            SELECT DISTINCT
                tblperiod.endperiod,
                tblperiod.periodname,
                projectmonth2.project,
                SUM(projectmonth2.totalcost)           AS cumcost,
                SUM(projectmonth2.invoices)            AS cuminvoices,
                SUM(projectmonth2.coiw)                AS cumcoiw,
                SUM(projectmonth2.portsales)           AS cumportsales,
                SUM(projectmonth2.costprofile)         AS cumprofile,
                projectmonth2.sumofcostprofile,
                SUM(projectmonth2.mstonedue)           AS sumofmstonedue,
                SUM(projectmonth2.due__done)           AS sumofdue__done,
                SUM(projectmonth2.ontime)              AS sumofontime,
                SUM(projectmonthcasework.cwdebit)::numeric::money   AS cumcwdebit,
                SUM(projectmonthcasework.cwcredit)::numeric::money  AS cumcwcredit,
                SUM(projectmonth2.totalhours)          AS cumtotalhours,
                SUM(projectmonth2.subcontracts::numeric::double precision) AS cumsubcontracts,
                SUM(projectmonth2.transfercosts)       AS cumtestcosts,
                SUM(projectmonth2.paycosts)            AS cumpaycosts
            FROM (fps.tblperiod
                INNER JOIN fps.tblkperiodmonth
                    ON tblperiod.periodname = tblkperiodmonth.periodname)
                INNER JOIN fps.projectmonth2
                    ON tblkperiodmonth.monthno = projectmonth2.monthno
                INNER JOIN fps.projectmonthcasework
                    ON  projectmonth2.monthno = projectmonthcasework.monthno
                    AND projectmonth2.project = projectmonthcasework.project
            GROUP BY
                tblperiod.endperiod,
                tblperiod.periodname,
                projectmonth2.project,
                projectmonth2.sumofcostprofile;
            """;

        internal const string CreateProjectMonthFinal = """
            INSERT INTO fps.projectmonthfinal (
                project,
                monthno,
                costprofile,
                subcontracts,
                animals,
                nonanimals,
                timecosts,
                transfercosts,
                totalcost,
                invoices,
                coiw,
                portsales,
                cumcost,
                cumprofile,
                periodname,
                sumofcostprofile,
                cuminvoices,
                cumcoiw,
                cumportsales,
                mstonedue,
                due__done,
                ontime,
                sumofmstonedue,
                sumofdue__done,
                sumofontime,
                cumflag,
                cwdebit,
                cwcredit,
                cumcwdebit,
                cumcwcredit,
                totalhours,
                cumtotalhours,
                cumsubcontracts,
                cumtestcosts,
                paycosts,
                cumpaycosts
            )
            SELECT DISTINCT
                projectmonth2.project,
                projectmonth2.monthno,
                projectmonth2.costprofile,
                projectmonth2.subcontracts,
                projectmonth2.animals,
                projectmonth2.nonanimal,
                projectmonth2.timecosts::numeric::money,
                projectmonth2.transfercosts::numeric::money,
                projectmonth2.totalcost,
                projectmonth2.invoices,
                projectmonth2.coiw,
                projectmonth2.portsales::numeric::money,
                CASE WHEN projectmonth2.monthno <= :month THEN cumcost * 1     ELSE NULL END AS cumcost,
                projectmonth3.cumprofile,
                projectmonth3.periodname,
                projectmonth3.sumofcostprofile,
                CASE WHEN projectmonth2.monthno <= :month THEN cuminvoices * 1 ELSE NULL END AS cuminvoices,
                CASE WHEN projectmonth2.monthno <= :month THEN cumcoiw * 1     ELSE NULL END AS cumcoiw,
                CASE
                    WHEN projectmonth2.monthno <= :month THEN cumportsales::numeric::money
                    ELSE NULL
                END AS cumportsales,
                projectmonth2.mstonedue,
                projectmonth2.due__done,
                projectmonth2.ontime,
                projectmonth3.sumofmstonedue,
                CASE WHEN projectmonth2.monthno <= :month THEN sumofdue__done * 1 ELSE NULL END AS sumofdue__done,
                CASE WHEN projectmonth2.monthno <= :month THEN sumofontime * 1    ELSE NULL END AS sumofontime,
                CASE WHEN projectmonth2.monthno <= :month THEN 1                  ELSE NULL END AS cumflag,
                CASE
                    WHEN projectmonth2.monthno <= :month THEN projectmonthcasework.cwdebit::numeric::money
                    ELSE NULL
                END,
                CASE
                    WHEN projectmonth2.monthno <= :month THEN projectmonthcasework.cwcredit::numeric::money
                    ELSE NULL
                END,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumcwdebit      ELSE NULL END,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumcwcredit     ELSE NULL END,
                projectmonth2.totalhours,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumtotalhours   ELSE NULL END,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumsubcontracts ELSE NULL END,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumtestcosts    ELSE NULL END,
                projectmonth2.paycosts,
                CASE WHEN projectmonth2.monthno <= :month THEN 1 * projectmonth3.cumpaycosts     ELSE NULL END
            FROM fps.projectmonth2
                INNER JOIN fps.projectmonth3
                    ON  projectmonth2.project = projectmonth3.project
                    AND projectmonth2.monthno = projectmonth3.endperiod
                INNER JOIN fps.projectmonthcasework
                    ON  projectmonth2.project = projectmonthcasework.project
                    AND projectmonth2.monthno = projectmonthcasework.monthno;
            """;

        internal const string LogRecreateSummaries = """
            INSERT INTO fps.recreatesummaries_log (userid, period, datedone)
            VALUES (:userId, :month, CURRENT_TIMESTAMP);
            """;

        internal const string RefreshPeriodMo = """
            DELETE FROM fps.period_monthlyoutput
            WHERE period = :period;

            INSERT INTO fps.period_monthlyoutput (
                period,
                project,
                oracleprojectcode,
                subaccountcode,
                isdefraproject,
                opc,
                occ,
                month,
                spc,
                workgroup,
                scc,
                testcode,
                volume,
                testprice,
                totalcost
            )
            SELECT
                :period,
                tlkpproject.parentproject    AS project,
                tlkpproject.oracleprojectcode,
                tlkpproject.subaccountcode,
                CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS isdefraproject,
                costcentre.profitcentre      AS opc,
                costcentre.costcentre        AS occ,
                monthlyoutput.month,
                workgroup.profitcentre       AS spc,
                workgroup.workgroup,
                workgroup.costcentre         AS scc,
                monthlyoutput.testcode,
                monthlyoutput.volume,
                tlkptestreqmt.unitprice      AS testprice,
                CAST(unitprice * volume AS numeric) AS totalcost
            FROM ((fps.tlkpproject
            LEFT JOIN fps.costcentre
                ON tlkpproject.costcentre = costcentre.costcentre)
                INNER JOIN (fps.monthlyoutput
                INNER JOIN fps.workgroup
                    ON monthlyoutput.workgroup = workgroup.workgroup)
                ON tlkpproject.parentproject = monthlyoutput.buyer)
                INNER JOIN fps.tlkptestreqmt
                    ON  monthlyoutput.buyer     = tlkptestreqmt.projectbuyercode
                    AND monthlyoutput.testcode  = tlkptestreqmt.testcode;
            """;

        internal const string RefreshPeriodPsc = """
            DELETE FROM fps.period_proj_subcontract
            WHERE period = :period;

            INSERT INTO fps.period_proj_subcontract (
                period,
                subcontcounter,
                project,
                oracleprojectcode,
                subaccountcode,
                isdefraproject,
                opc,
                occ,
                month,
                amount,
                acctcode
            )
            SELECT
                :period,
                proj_subcontract.subcontcounter,
                proj_subcontract.project,
                tlkpproject.oracleprojectcode,
                tlkpproject.subaccountcode,
                CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS isdefraproject,
                costcentre.profitcentre  AS opc,
                costcentre.costcentre    AS occ,
                proj_subcontract.month,
                proj_subcontract.amount,
                proj_subcontract.acctcode
            FROM fps.costcentre
            RIGHT OUTER JOIN fps.tlkpproject
                ON fps.costcentre.costcentre = fps.tlkpproject.costcentre
            INNER JOIN fps.proj_subcontract
                ON fps.tlkpproject.parentproject = fps.proj_subcontract.project;
            """;

        internal const string RefreshPeriodTcc = """
            DELETE FROM fps.period_timecostcalcs
            WHERE period = :period;

            INSERT INTO fps.period_timecostcalcs (
                period,
                project,
                oracleprojectcode,
                subaccountcode,
                month,
                defraproject,
                occ,
                opc,
                spc,
                scc,
                name,
                gradecode,
                spnumber,
                chargerate,
                pay,
                nonpay,
                overhead,
                time,
                totalcost
            )
            SELECT
                :period,
                tlkpproject.parentproject    AS project,
                tlkpproject.oracleprojectcode,
                tlkpproject.subaccountcode,
                timecostcalcs.month,
                CASE tlkpproject.isdefraproject WHEN 0 THEN 'No' ELSE 'Yes' END AS defraproject,
                costcentre.costcentre        AS occ,
                costcentre.profitcentre      AS opc,
                workgroup.profitcentre       AS spc,
                workgroup.costcentre         AS scc,
                timecostcalcs.name,
                timecostcalcs.gradecode,
                tblwgemployee.spnumber,
                timecostcalcs.chargerate,
                timecostcalcs.pay,
                timecostcalcs.nonpay,
                timecostcalcs.overhead,
                timecostcalcs.time,
                timecostcalcs.cost           AS totalcost
            FROM fps.tblwgemployee
            INNER JOIN (
                (fps.tlkpproject
                LEFT JOIN fps.costcentre
                    ON tlkpproject.costcentre = costcentre.costcentre)
                INNER JOIN (fps.timecostcalcs
                INNER JOIN fps.workgroup
                    ON timecostcalcs.workgroup = workgroup.workgroup)
                ON tlkpproject.parentproject = timecostcalcs.project)
            ON tblwgemployee.pactid = timecostcalcs.staffid;
            """;
    }
}
