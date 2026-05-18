using System.Runtime.CompilerServices;
using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal abstract class MabArchiveLoaderBase : IMabArchiveLoader
{
    public abstract int Sequence { get; }

    public abstract string Name { get; }

    public Task<int> LoadAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return ExecuteAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}

internal abstract class MabArchiveSqlLoaderBase : MabArchiveLoaderBase
{
    protected override Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return context.Database.ExecuteSqlInterpolatedAsync(BuildSql(year), cancellationToken);
    }

    protected abstract FormattableString BuildSql(int year);
}

internal abstract class MabArchiveDotNetLoaderBase : MabArchiveLoaderBase
{
    protected override Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return LoadWithDotNetAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}

internal sealed class MyTlkpProgramLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tlkpprogram (
    year, programno, programname, directorate, minim, sector_name, customer, target, manager
)
SELECT
    {0}, p.programno, p.programname, p.directorate, p.minim, p.sector_name,
    p.customer, p.target, p.manager
FROM fps.tlkpprogram p
WHERE p.fpsyear = {0}
";

    public override int Sequence => 1;

    public override string Name => "my_tlkpprogram";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class GTlkpProjectLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.g_tlkpproject (
    parentproject, projecttitle, costbookno, disease, contract, shorttitle, projectstatus
)
SELECT
    t.parentproject, t.projecttitle, t.costbookno, t.disease, t.contract,
    t.shorttitle, t.projectstatus
FROM fps.tlkpproject t
WHERE t.fpsyear = {0}
GROUP BY t.parentproject, t.projecttitle, t.costbookno, t.disease,
         t.contract, t.shorttitle, t.projectstatus
";

    public override int Sequence => 2;

    public override string Name => "g_tlkpproject";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTlkpProjectLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tlkpproject (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {0}, t.parentproject, t.program, t.customer, t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, t.projectstatus, t.datecreated, t.feccost,
    t.profit, t.budget_cvl, t.caseworksub, t.pvsincome, t.plancaseworkdebit,
    t.disease, t.contract, t.finished, t.comments, t.carryover, t.isdefraproject,
    t.costcentre, t.oracleprojectcode, t.subaccountcode, t.projectgroup, t.incomeaccountcode
FROM fps.tlkpproject t
WHERE t.fpsyear = {0}
";

    public override int Sequence => 3;

    public override string Name => "my_tlkpproject";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyFpsYearTotalsLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_fpsyeartotals (
    year, parentproject, program, totaladditionalcosts, totalanimalcosts,
    totalstaffcosts, totaltestcosts, totalcosts, custincome, transferincome,
    totalincome, budget_cvl, requiredprofit, manager, customer, projectstatus,
    pvsincome, plancaseworkdebit, totalpaycosts
)
SELECT
    {0}, f.parentproject, f.program, f.totaladditionalcosts, f.totalanimalcosts,
    f.totalstaffcosts, f.totaltestcosts, f.totalcosts, f.custincome, f.transferincome,
    f.totalincome, f.budget_cvl, f.requiredprofit, f.manager, f.customer,
    f.projectstatus, f.pvsincome, f.plancaseworkdebit, f.totalpaycosts
FROM fps.fpsyeartotals f
WHERE f.fpsyear = {0}
";

    public override int Sequence => 4;

    public override string Name => "my_fpsyeartotals";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyMonthlyOutputLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_monthlyoutput (
    year, testcode, buyer, month, workgroup, volume, wgbuyer
)
SELECT
    {0}, m.testcode, m.buyer, m.month, m.workgroup, m.volume, m.wgbuyer
FROM fps.monthlyoutput m
WHERE m.fpsyear = {0}
";

    public override int Sequence => 5;

    public override string Name => "my_monthlyoutput";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyMonthlyTimeLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_monthlytime (
    year, pactstaffid, timecode, month, parentproject, workgroup, hours
)
SELECT
    {0}, m.pactstaffid, m.timecode, m.month, m.parentproject, m.workgroup, m.hours
FROM fps.monthlytime m
WHERE m.fpsyear = {0}
";

    public override int Sequence => 6;

    public override string Name => "my_monthlytime";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyProjInvoiceLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_proj_invoice (
    year, projectparent, month, amount, costofwork, wip, profitloss, detail,
    invoicecounter, type
)
SELECT
    {0}, i.projectparent, i.month, i.amount, i.costofwork, i.wip, i.profitloss,
    i.detail, i.invoicecounter, i.type
FROM fps.proj_invoice i
WHERE i.fpsyear = {0}
";

    public override int Sequence => 7;

    public override string Name => "my_proj_invoice";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyProjSubcontractLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_proj_subcontract (
    year, subcontcounter, project, testjob, month, amount, workgroup, acctcode,
    supplier, description, suppliernumber, dailyrate, animaldays
)
SELECT
    {0}, s.subcontcounter, s.project, s.testjob, s.month, s.amount, s.workgroup,
    s.acctcode, s.supplier, s.description, s.suppliernumber, s.dailyrate, s.animaldays
FROM fps.proj_subcontract s
WHERE s.fpsyear = {0}
";

    public override int Sequence => 8;

    public override string Name => "my_proj_subcontract";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyProjectMonthFinalLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_projectmonthfinal (
    year, project, monthno, periodname, cumflag, costprofile, subcontracts, animals,
    nonanimals, timecosts, transfercosts, totalcost, invoices, coiw, portsales,
    cumcost, cumprofile, sumofcostprofile, cuminvoices, cumcoiw, cumportsales,
    mstonedue, due__done, ontime, sumofmstonedue, sumofdue__done, sumofontime,
    cwdebit, cwcredit, cumcwdebit, cumcwcredit, totalhours, cumtotalhours,
    cumsubcontracts, cumtestcosts, paycosts, cumpaycosts
)
SELECT
    {0}, p.project, p.monthno, p.periodname, p.cumflag, p.costprofile, p.subcontracts,
    p.animals, p.nonanimals, p.timecosts, p.transfercosts, p.totalcost, p.invoices,
    p.coiw, p.portsales, p.cumcost, p.cumprofile, p.sumofcostprofile, p.cuminvoices,
    p.cumcoiw, p.cumportsales, p.mstonedue, p.due__done, p.ontime, p.sumofmstonedue,
    p.sumofdue__done, p.sumofontime, p.cwdebit, p.cwcredit, p.cumcwdebit, p.cumcwcredit,
    p.totalhours, p.cumtotalhours, p.cumsubcontracts, p.cumtestcosts, p.paycosts, p.cumpaycosts
FROM fps.projectmonthfinal p
WHERE p.fpsyear = {0}
";

    public override int Sequence => 9;

    public override string Name => "my_projectmonthfinal";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblAdditionalCostsLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tbladditionalcosts (
    year, jobcode, account, description, itemcost, freq, supplier, ac_counter
)
SELECT
    {0}, a.jobcode, a.account, a.description, a.itemcost, a.freq, a.supplier,
    ROW_NUMBER() OVER (ORDER BY a.jobcode, a.account, a.description)
        + COALESCE((SELECT MAX(ac_counter) FROM mabarchive.my_tbladditionalcosts), 0)
FROM fps.tbladditionalcosts a
WHERE a.fpsyear = {0}
";

    public override int Sequence => 10;

    public override string Name => "my_tbladditionalcosts";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblAnimalReqLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tblanimalreq (
    year, jobcode, animaltype, numberofdays, numberofanimals
)
SELECT
    {0}, a.jobcode, a.animaltype, a.numberofdays, a.numberofanimals
FROM fps.tblanimalreq a
WHERE a.fpsyear = {0}
";

    public override int Sequence => 11;

    public override string Name => "my_tblanimalreq";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblContractLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tblcontract (
    year, contractno, category, manager, customer, title,
    registereddate, startdate, enddate, contractdoc, duration
)
SELECT
    {0}, c.contractno, c.category, c.manager, c.customer, c.title,
    c.registereddate, c.startdate, c.enddate, c.contractdoc, c.duration
FROM fps.tblcontract c
WHERE c.fpsyear = {0}
";

    public override int Sequence => 12;

    public override string Name => "my_tblcontract";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblStaffJobLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tblstaffjob (
    year, staffid, jobcode, plannedhours
)
SELECT
    {0}, s.staffid, s.jobcode, s.plannedhours
FROM fps.tblstaffjob s
WHERE s.fpsyear = {0}
";

    public override int Sequence => 13;

    public override string Name => "my_tblstaffjob";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTimeCostCalcsLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_timecostcalcs (
    year, workgroup, jobcode, project, month, staffid,
    gradecode, name, chargerate, class, time, cost,
    division, jobcodeold, pay, nonpay, overhead
)
SELECT
    {0}, t.workgroup, t.jobcode, t.project, t.month, t.staffid,
    t.gradecode, t.name, t.chargerate, t.class, t.time, t.cost,
    t.division, t.jobcodeold, t.pay, t.nonpay, t.overhead
FROM fps.timecostcalcs t
WHERE t.fpsyear = {0}
";

    public override int Sequence => 14;

    public override string Name => "my_timecostcalcs";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTlkpTestReqmtLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tlkptestreqmt (
    year, testcode, buyer, unitprice, norequired, projectbuyercode, testbuyercode
)
SELECT
    {0}, r.testcode, r.buyer, r.unitprice, r.norequired, r.projectbuyercode, r.testbuyercode
FROM fps.tlkptestreqmt r
WHERE r.fpsyear = {0}
";

    public override int Sequence => 15;

    public override string Name => "my_tlkptestreqmt";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class TlkpYearLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.tlkpyear (year, latestmonthreleased)
SELECT {0}, CAST(v.db_var_value AS integer)
FROM fps.tbldb_variables v
WHERE v.db_var_name = 'month'
";

    public override int Sequence => 16;

    public override string Name => "tlkpyear";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyWorkgroupGradeLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_workgroupgrade (
    year, wggrade, profitcentregrade, gradecode, workgroup
)
SELECT
    {0}, w.wggrade, w.profitcentregrade, w.gradecode, w.workgroup
FROM fps.workgroupgrade w
WHERE w.fpsyear = {0}
";

    public override int Sequence => 17;

    public override string Name => "my_workgroupgrade";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyProfitCentreGradeLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_profitcentregrade (
    year, pcgrade, divisiongrade, gradecode, profitcentre,
    chargerate, directrate, payrate, npr, ohr
)
SELECT
    {0}, p.pcgrade, p.divisiongrade, p.gradecode, p.profitcentre,
    p.chargerate, p.directrate, p.payrate, p.npr, p.ohr
FROM fps.profitcentregrade p
WHERE p.fpsyear = {0}
";

    public override int Sequence => 18;

    public override string Name => "my_profitcentregrade";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblProfitCentreLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tblprofitcentre (
    year, profitcentre, profitcentrename, division, conttarget, profitcentrehead, divisionid
)
SELECT
    {0}, p.profitcentre, p.profitcentrename, p.division,
    p.conttarget, p.profitcentrehead, p.divisionid
FROM fps.tblkpprofitcentre p
";

    public override int Sequence => 19;

    public override string Name => "my_tblprofitcentre";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTestOrProductLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_testorproduct (
    year, itemcode, itemdescription, testmanager, jobstatus,
    unitpricevla, priceahvg, owner, chargemethod, shortdescription, defraunitprice
)
SELECT
    {0}, t.itemcode, t.itemdescription, t.testmanager, t.jobstatus,
    t.unitpricevla, t.priceahvg, t.owner, t.chargemethod, t.shortdescription, t.defraunitprice
FROM fps.testorproduct t
WHERE t.fpsyear = {0}
";

    public override int Sequence => 20;

    public override string Name => "my_testorproduct";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyStaffLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_staff (
    year, staffid, name, workgroupgrade, title,
    personstatus, personclass, hrspaid, leave, sickspecial, hrsavail
)
SELECT
    {0},
    wge.pactid,
    COALESCE(e.lastname, '') || ', ' || COALESCE(e.firstname, ''),
    wge.workgroupgrade,
    e.title,
    wge.personstatus,
    wge.personclass,
    wge.hrspaid,
    wge.leave,
    wge.sickspecial,
    wge.hrsavail
FROM fps.tblwgemployee wge
JOIN fps.tblemployee e ON wge.spnumber = e.spnumber
WHERE wge.fpsyear = {0}
";

    public override int Sequence => 21;

    public override string Name => "my_staff";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyWorkgroupLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_workgroup (
    year, workgroup, profitcentre, costcentre, owner,
    description, centraloverhead, sendemail, cos90, costcentreold, email_recipient
)
SELECT
    {0}, w.workgroup, w.profitcentre, w.costcentre, w.owner,
    w.description, w.centraloverhead, w.sendemail, w.cos90, w.costcentreold, w.email_recipient
FROM fps.workgroup w
WHERE w.fpsyear = {0}
";

    public override int Sequence => 22;

    public override string Name => "my_workgroup";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTblAnimalsLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tblanimals (
    year, animaltype, species, security_level, dailyrate, planbyweek, defradailyrate
)
SELECT
    {0}, a.animaltype, a.species, a.security_level, a.dailyrate, a.planbyweek, a.defradailyrate
FROM fps.tblanimals a
WHERE a.fpsyear = {0}
";

    public override int Sequence => 23;

    public override string Name => "my_tblanimals";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}

internal sealed class MyTlkpProjectAllLoader : MabArchiveSqlLoaderBase
{
    private const string SqlTemplate = @"
INSERT INTO mabarchive.my_tlkpproject_all (
    year, parentproject, program, customer, manager, transferincome, custincome,
    wip_eoy, wip_limit, wip_current, projectstatus, datecreated, feccost,
    profit, budget_cvl, caseworksub, pvsincome, plancaseworkdebit,
    disease, contract, finished, comments, carryover, isdefraproject,
    costcentre, oracleprojectcode, subaccountcode, projectgroup, incomeaccountcode
)
SELECT
    {0}, t.parentproject, t.program, t.customer, t.manager, t.transferincome, t.custincome,
    t.wip_eoy, t.wip_limit, t.wip_current, t.projectstatus, t.datecreated, t.feccost,
    t.profit, t.budget_cvl, t.caseworksub, t.pvsincome, t.plancaseworkdebit,
    t.disease, t.contract, t.finished, t.comments, t.carryover, t.isdefraproject,
    t.costcentre, t.oracleprojectcode, t.subaccountcode, t.projectgroup, t.incomeaccountcode
FROM fps.tlkpproject t
WHERE t.fpsyear = {0}
";

    public override int Sequence => 24;

    public override string Name => "my_tlkpproject_all";

    protected override FormattableString BuildSql(int year) => FormattableStringFactory.Create(SqlTemplate, year);
}
