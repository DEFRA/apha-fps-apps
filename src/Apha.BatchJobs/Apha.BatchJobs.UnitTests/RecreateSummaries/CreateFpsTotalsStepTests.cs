using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class CreateFpsTotalsStepTests
{
    [Fact]
    public async Task ExecuteCoreAsync_SuccessPath()
    {
        // Arrange: PostgreSQL EF Core context
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__BatchJobsConnectionString")
            ?? "Host=localhost;Port=5432;Database=batch_jobs_foundation_db;Username=postgres;Password=admin123;Timeout=30";
        var options = new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        using var db = new BatchJobsDbContext(options);

        // Clean up and seed as needed for test
        await db.Database.EnsureCreatedAsync();
        // Clean up tables
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RsFpsYearTotals\" RESTART IDENTITY CASCADE;");
        await db.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RsTlkpProject\" RESTART IDENTITY CASCADE;");
        // Insert required data for views using raw SQL
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsTlkpProject\" (\"ParentProject\", \"Program\", \"PlanCaseworkDebit\", \"CustIncome\", \"TransferIncome\", \"BudgetCvl\", \"Profit\", \"Manager\", \"Customer\", \"ProjectStatus\", \"PvsIncome\", \"FpsYear\") VALUES ('PRJ1', 'PRG1', 10, 100, 200, 50, 20, 'Mgr', 'Cust', 'Active', 5, 2026);");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsQryTotalAdditionalCostsView\" (\"JobCode\", \"TotalAdditionalCosts\", \"FpsYear\") VALUES ('PRJ1', 1, 2026);");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsQryTotalAnimalCostsView\" (\"JobCode\", \"TotalAnimalCosts\", \"FpsYear\") VALUES ('PRJ1', 2, 2026);");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsQryTotalStaffCostsView\" (\"JobCode\", \"TotalStaffCosts\", \"TotalPayCosts\", \"FpsYear\") VALUES ('PRJ1', 3, 7, 2026);");
        await db.Database.ExecuteSqlRawAsync("INSERT INTO \"RsQryTotalTestCostsView\" (\"JobCode\", \"TotalTestCosts\", \"FpsYear\") VALUES ('PRJ1', 4, 2026);");

        var context = new RecreateSummariesExecutionContext(db, new NpgsqlConnection(connectionString));
        var step = new CreateFpsTotalsStep();

        // Act
        var result = await step.ExecuteAsync(context, CancellationToken.None);

        // Assert
        Assert.Equal("CreateFpsTotals", result.StepName);
        // Additional asserts as needed, or adapt to your real DB state
    }
}
