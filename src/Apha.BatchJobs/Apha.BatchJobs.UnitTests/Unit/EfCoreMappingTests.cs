using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Validates that the EF Core model configuration mirrors the SQL schema:
/// column names, required constraints, max-lengths, FK navigation, delete behavior.
/// No DB connection required ΓÇö these assertions run entirely against the in-memory model metadata.
/// Internal entity types are accessed by table-name lookup to avoid visibility constraints.
/// </summary>
public sealed class EfCoreMappingTests
{
    private static readonly DbContextOptions<BatchJobsDbContext> _options =
        new DbContextOptionsBuilder<BatchJobsDbContext>()
            .UseNpgsql("Host=localhost;Database=test") // metadata-only; no connection made
            .Options;

    private static IEntityType GetEntityByTable(BatchJobsDbContext ctx, string table) =>
        ctx.Model.GetEntityTypes()
                 .Single(e => e.GetTableName() == table);

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // job_lock
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void BatchLock_JobName_MaxLength_Is_255()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.JobName))!;
        Assert.Equal(255, prop.GetMaxLength());
    }

    [Fact]
    public void BatchLock_JobQueueId_Is_Mapped()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.JobQueueId));
        Assert.NotNull(prop);
    }

    [Fact]
    public void BatchLock_TableName_Is_job_lock_In_Fps_Schema()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var entityType = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!;
        Assert.Equal("job_lock", entityType.GetTableName());
        Assert.Equal("fps", entityType.GetSchema());
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // job_master
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void TblJobMaster_JobName_MaxLength_Is_100()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var entityType = GetEntityByTable(ctx, "job_master");
        var prop = entityType.FindProperty("JobName")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobMaster_CreatedAt_HasDefaultValueSql()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "job_master").FindProperty("CreatedAt")!;
        Assert.Equal("NOW()", prop.GetDefaultValueSql());
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // job_status FK
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void TblJobStatus_HasForeignKey_To_TblJobMaster_Cascade()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var master = GetEntityByTable(ctx, "job_master");
        var status = GetEntityByTable(ctx, "job_status");
        var fk = status.GetForeignKeys()
                       .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // job_queue FK + error message length
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void TblJobQueue_ErrorMessage_MaxLength_Is_1000()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "job_queue").FindProperty("ErrorMessage")!;
        Assert.Equal(1000, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueue_RequestedAtUtc_ColumnName_Is_requested_at_utc()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "job_queue").FindProperty("RequestedAtUtc")!;
        Assert.Equal("requested_at_utc", prop.GetColumnName(StoreObjectIdentifier.Table("job_queue", "fps")));
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobMaster_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var master = GetEntityByTable(ctx, "job_master");
        var queue = GetEntityByTable(ctx, "job_queue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobStatus_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var status = GetEntityByTable(ctx, "job_status");
        var queue = GetEntityByTable(ctx, "job_queue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // job_queue_log FK + performed by length
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void TblJobQueueLog_PerformedBy_MaxLength_Is_100()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "job_queue_log").FindProperty("PerformedBy")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_Note_MaxLength_Is_500()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "job_queue_log").FindProperty("Note")!;
        Assert.Equal(500, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobQueue_Cascade()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var queue = GetEntityByTable(ctx, "job_queue");
        var log = GetEntityByTable(ctx, "job_queue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == queue);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobStatus_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var status = GetEntityByTable(ctx, "job_status");
        var log = GetEntityByTable(ctx, "job_queue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // scheduled_load_run
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void ScheduledLoadRun_Table_Exists_In_Fps_Schema()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var entity = GetEntityByTable(ctx, "scheduled_load_run");
        Assert.Equal("fps", entity.GetSchema());
    }

    [Fact]
    public void ScheduledLoadRun_JobName_MaxLength_Is_100()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "scheduled_load_run").FindProperty("JobName")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void ScheduledLoadRun_HasForeignKey_To_TblJobMaster_Restrict_Using_JobName()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var run = GetEntityByTable(ctx, "scheduled_load_run");
        var master = GetEntityByTable(ctx, "job_master");

        var fk = run.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
        Assert.Equal("JobName", fk.Properties.Single().Name);
        Assert.Equal("JobName", fk.PrincipalKey.Properties.Single().Name);
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // scheduled_load_step_run + scheduled_load_validation_result
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void ScheduledLoadStepRun_HasForeignKey_To_ScheduledLoadRun_Cascade()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var run = GetEntityByTable(ctx, "scheduled_load_run");
        var step = GetEntityByTable(ctx, "scheduled_load_step_run");
        var fk = step.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType == run);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void ScheduledLoadValidationResult_Has_Unique_Index_On_RunId_And_AssertionCode()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var validation = GetEntityByTable(ctx, "scheduled_load_validation_result");

        var uniqueIndex = validation
            .GetIndexes()
            .FirstOrDefault(i => i.IsUnique &&
                                 i.Properties.Select(p => p.Name).SequenceEqual(new[] { "RunId", "AssertionCode" }));

        Assert.NotNull(uniqueIndex);
        Assert.Equal("uq_scheduled_load_validation_run_assertion", uniqueIndex!.GetDatabaseName());
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // fps_source_project_year
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void FpsSourceProjectYear_Has_CompositePrimaryKey_Year_ParentProject()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var source = GetEntityByTable(ctx, "fps_source_project_year");
        Assert.Equal("fps", source.GetSchema());
        var keyProps = source.FindPrimaryKey()!.Properties.Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "Year", "ParentProject" }, keyProps);
    }

    [Fact]
    public void FpsSourceProjectYear_Program_MaxLength_Is_10_And_TotalAdditionalCosts_Is_Money()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var source = GetEntityByTable(ctx, "fps_source_project_year");

        var program = source.FindProperty("Program")!;
        Assert.Equal(10, program.GetMaxLength());

        var totalAdditional = source.FindProperty("TotalAdditionalCosts")!;
        Assert.Equal("money", totalAdditional.GetColumnType());
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // fps_year_totals + fps_year_archive
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void FpsYearTotals_ProjectStatus_Is_Required_And_CustIncome_Is_Money()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var totals = GetEntityByTable(ctx, "fps_year_totals");
        Assert.Equal("fps", totals.GetSchema());

        var projectStatus = totals.FindProperty("ProjectStatus")!;
        Assert.False(projectStatus.IsNullable);

        var custIncome = totals.FindProperty("CustIncome")!;
        Assert.Equal("money", custIncome.GetColumnType());
    }

    [Fact]
    public void FpsYearArchive_ArchiveReason_MaxLength_Is_100_And_Has_Default()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var archive = GetEntityByTable(ctx, "fps_year_archive");
        Assert.Equal("fps", archive.GetSchema());
        var reason = archive.FindProperty("ArchiveReason")!;

        Assert.Equal(100, reason.GetMaxLength());
        Assert.Equal("Before deletion", reason.GetDefaultValue());
    }

    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ
    // fps_project_all_current_year
    // ΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇΓöÇ

    [Fact]
    public void FpsProjectAllCurrentYear_Has_CompositePrimaryKey_Year_ParentProject()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var projectAll = GetEntityByTable(ctx, "fps_project_all_current_year");
        Assert.Equal("fps", projectAll.GetSchema());
        var keyProps = projectAll.FindPrimaryKey()!.Properties.Select(p => p.Name).ToArray();
        Assert.Equal(new[] { "Year", "ParentProject" }, keyProps);
    }

    [Fact]
    public void FpsProjectAllCurrentYear_DateCreated_Uses_Date_Column_Type()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var projectAll = GetEntityByTable(ctx, "fps_project_all_current_year");
        var dateCreated = projectAll.FindProperty("DateCreated")!;
        Assert.Equal("date", dateCreated.GetColumnType());
    }
}
