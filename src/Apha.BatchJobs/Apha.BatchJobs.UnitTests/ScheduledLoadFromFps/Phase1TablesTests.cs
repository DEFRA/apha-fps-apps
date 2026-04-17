using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Apha.BatchJobs.UnitTests.ScheduledLoadFromFps;

/// <summary>
/// Phase 1 schema/mapping checks for ScheduledLoadFromFps foundation tables.
/// These tests validate EF model metadata and do not require a live DB connection.
/// </summary>
public sealed class Phase1TablesTests
{
    private static readonly DbContextOptions<BatchJobsDbContext> Options =
        new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql("Host=localhost;Database=test")
            .Options;

    private static IEntityType GetEntityByTable(BatchJobsDbContext ctx, string table) =>
        ctx.Model.GetEntityTypes().Single(e => e.GetTableName() == table);

    [Fact]
    public void DbContext_CanInstantiate_WithoutErrors()
    {
        using var context = new BatchJobsDbContext(Options);
        Assert.NotNull(context);
        Assert.NotNull(context.Model);
    }

    [Fact]
    public void ScheduledLoad_CoreTables_ArePresentInModel()
    {
        using var context = new BatchJobsDbContext(Options);
        var tableNames = context.Model.GetEntityTypes().Select(e => e.GetTableName()).ToHashSet();

        Assert.Contains("job_master", tableNames);
        Assert.Contains("job_status", tableNames);
        Assert.Contains("scheduled_load_run", tableNames);
        Assert.Contains("scheduled_load_step_run", tableNames);
        Assert.Contains("scheduled_load_validation_result", tableNames);
        Assert.Contains("fps_source_project_year", tableNames);
        Assert.Contains("fps_year_totals", tableNames);
    }

    [Fact]
    public void ScheduledLoadRun_ForeignKey_To_JobMaster_IsRestrict()
    {
        using var context = new BatchJobsDbContext(Options);
        var run = GetEntityByTable(context, "scheduled_load_run");
        var jobMaster = GetEntityByTable(context, "job_master");

        var fk = run.GetForeignKeys().Single(f => f.PrincipalEntityType == jobMaster);
        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
        Assert.Equal("fk_scheduled_load_run_jobname", fk.GetConstraintName());
    }

    [Fact]
    public void ScheduledLoadRun_Indexes_AreNamedAsExpected()
    {
        using var context = new BatchJobsDbContext(Options);
        var run = GetEntityByTable(context, "scheduled_load_run");
        var indexNames = run.GetIndexes().Select(i => i.GetDatabaseName()).ToHashSet();

        Assert.Contains("idx_scheduled_load_run_job_fps_year", indexNames);
        Assert.Contains("idx_scheduled_load_run_correlation_id", indexNames);
    }

    [Fact]
    public void FpsYearTotals_UsesCompositePrimaryKey_YearAndParentProject()
    {
        using var context = new BatchJobsDbContext(Options);
        var totals = GetEntityByTable(context, "fps_year_totals");

        var key = totals.FindPrimaryKey();
        Assert.NotNull(key);
        Assert.Equal(new[] { "Year", "ParentProject" }, key!.Properties.Select(p => p.Name));
    }
}
