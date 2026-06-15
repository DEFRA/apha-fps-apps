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

public sealed class RefreshPeriodMoStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        await using var harness = await RecreateSummariesPostgresTestHarness.CreateAsync();
        var db = harness.DbContext;

        var period = 91;
        var project = harness.Id("PRJMO");
        var program = "PRGMO";
        var costCentre = Random.Shared.Next(700001, 799999);
        var workGroup = harness.Id("WG1");
        var testCode = harness.Id("T1");

        await harness.ExecuteSqlAsync($@"
            INSERT INTO fps.tlkpprogram (programno, fpsyear, sector_name)
            VALUES ('{program}', {harness.FpsYear}, 'charge');

            INSERT INTO fps.tlkpproject
                (parentproject, projecttitle, program, customer, transferincome, custincome, projectstatus, disease,
                 contract, isdefraproject, costcentre, oracleprojectcode, subaccountcode, incomeaccountcode, fpsyear)
            VALUES
                ('{project}', 'Project MO', '{program}', 'Cust', 0::money, 0::money, 'Active', 'General',
                 'Contract', 0, {costCentre}, 'OPC1', 'SAC1', 'IA1', {harness.FpsYear});

            INSERT INTO fps.tblkpprofitcentre (profitcentre, profitcentrename, division)
            VALUES ('{harness.Id("PC1")}', 'Profit Centre', (SELECT divname FROM fps.tlkpdivision LIMIT 1));

            INSERT INTO fps.costcentre (costcentre, profitcentre, fpsyear)
            VALUES ({costCentre}, '{harness.Id("PC1")}', {harness.FpsYear});

            INSERT INTO fps.workgroup (workgroup, profitcentre, costcentre, fpsyear)
            VALUES ('{workGroup}', '{harness.Id("PC1")}', {costCentre}, {harness.FpsYear});

            INSERT INTO fps.tlkptestreqmt (testcode, buyer, unitprice, norequired, projectbuyercode, active, fpsyear)
            VALUES ('{testCode}', '{project}', 10::money, 1, '{project}', 1, {harness.FpsYear});

            INSERT INTO fps.monthlyoutput (buyer, workgroup, testcode, month, volume, fpsyear)
            VALUES ('{project}', '{workGroup}', '{testCode}', 1, 5, {harness.FpsYear});
        ");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection());
        var step = new RefreshPeriodMoStep(period);
        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);
        // Assert
        Assert.Equal("RefreshPeriodMo", result.StepName);
        Assert.Equal(StepStatus.Success, result.Status);

        // Validate output in RsPeriodMonthlyOutput
        var row = await db.RsPeriodMonthlyOutput.AsNoTracking()
            .SingleAsync(x => x.Period == period && x.Project == project && x.Month == 1);
        Assert.Equal(period, row.Period);
        Assert.Equal(project, row.Project);
        Assert.Equal("OPC1", row.OracleProjectCode);
        Assert.Equal("SAC1", row.SubAccountCode);
        Assert.Equal("No", row.IsDefraProject);
        Assert.Equal(harness.Id("PC1"), row.Opc);
        Assert.Equal((double)costCentre, row.Occ ?? 0d);
        Assert.Equal(1, row.Month);
        Assert.Equal(harness.Id("PC1"), row.Spc);
        Assert.Equal(workGroup, row.WorkGroup);
        Assert.Equal((double)costCentre, row.Scc ?? 0d);
        Assert.Equal(testCode, row.TestCode);
        Assert.Equal(5, row.Volume);
        Assert.Equal(10m, row.TestPrice);
        Assert.Equal(50m, row.TotalCost);
    }
}
