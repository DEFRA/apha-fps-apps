using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Domain.Enums;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateFpsTotalsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var project = harness.Id("PRJ1");
        var program = $"P{harness.Prefix[2..6]}A";
        var profitCentre = harness.Id("PC1");
        var pcGrade = harness.Id("PCG1");
        var wgGrade = harness.Id("WG1");
        var staffId = harness.Id("S1");
        var spNumber = harness.Id("SP1");
        var animalType = harness.Id("AN1");
        var testCode = harness.Id("T1");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, plancaseworkdebit, pvsincome, budget_cvl, profit, isdefraproject, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project', '{program}', 'Cust', 200::money, 100::money, 'Active', 'General',
                 'Contract', 10::money, 5::money, 50::money, 20::money, 0, 'IA1', {harness.FpsYear});

            INSERT INTO fps.tbladditionalcosts (jobcode, account, description, itemcost, fpsyear)
            VALUES ('{project}', 'A1', 'Additional', 1::money, {harness.FpsYear});

            INSERT INTO fps.tblanimals (animaltype, planbyweek, dailyrate, defradailyrate, fpsyear)
            VALUES ('{animalType}', false, 1::money, 1::money, {harness.FpsYear});

            INSERT INTO fps.tblanimalreq (jobcode, animaltype, numberofdays, numberofanimals, indcounter, fpsyear)
            VALUES ('{project}', '{animalType}', 1, 2, 990001, {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{profitCentre}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.profitcentregrade
                (pcgrade, divisiongrade, gradecode, profitcentre, chargerate, payrate, defrachargerate, fpsyear)
            VALUES
                ('{pcGrade}', 'D1', 'GC1', '{profitCentre}', 3::money, 7::money, 3::money, {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{harness.Id("WG")}', '{profitCentre}', 0, {harness.FpsYear});

            INSERT INTO fps.workgroupgrade (wggrade, profitcentregrade, gradecode, workgroup, fpsyear)
            VALUES ('{wgGrade}', '{pcGrade}', 'GC1', '{harness.Id("WG")}', {harness.FpsYear});

            INSERT INTO fps.tblemployee (spnumber, firstname, lastname, fpsyear)
            VALUES ('{spNumber}', 'Staff', 'One', {harness.FpsYear});

            INSERT INTO fps.tblwgemployee
                (pactid, spnumber, workgroupgrade, personstatus, personclass, hrspaid, leave, sickspecial, hrsavail,
                 makeavailable, timerecorder, fpsyear)
            VALUES
                ('{staffId}', '{spNumber}', '{wgGrade}', 'A', 'P', 37, 0, 0, 37, 1, 0, {harness.FpsYear});

            INSERT INTO fps.tblstaffjob (staffid, jobcode, plannedhours, fpsyear)
            VALUES ('{staffId}', '{project}', 1, {harness.FpsYear});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES ('{testCode}', '{project}', 4::money, 1, '{project}', 1, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var deleteStep = new DeleteFpsTotalsStep();
        var deleteResult = await deleteStep.ExecuteAsync(context, CancellationToken.None);
        Assert.True(deleteResult.Status == StepStatus.Success, deleteResult.ErrorMessage);

        var step = new CreateFpsTotalsStep();

        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal("CreateFpsTotals", result.StepName);
        Assert.True(result.Status == StepStatus.Success, result.ErrorMessage);

        var row = await db.RsFpsYearTotals.AsNoTracking()
            .SingleAsync(x => x.ParentProject == project && x.FpsYear == harness.FpsYear);
        Assert.Equal(project, row.ParentProject);
        Assert.Equal(program, row.Program);
        Assert.Equal(1m, row.TotalAdditionalCosts);
        Assert.Equal(2d, row.TotalAnimalCosts);
        Assert.Equal(3d, row.TotalStaffCosts);
        Assert.Equal(4d, row.TotalTestCosts);
        Assert.Equal(20d, row.TotalCosts);
        Assert.Equal(300m, row.TotalIncome);
        Assert.Equal(7d, row.TotalPayCosts);
        Assert.Equal(harness.FpsYear, row.FpsYear);
    }
}
