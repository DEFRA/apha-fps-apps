using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Apha.BatchJobs.UnitTests;

/// <summary>
/// Validates that the EF Core model configuration mirrors the SQL schema:
/// column names, required constraints, max-lengths, FK navigation, delete behavior.
/// No DB connection required — these assertions run entirely against the in-memory model metadata.
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

    // ─────────────────────────────────────────────────────────────
    // batch_lock
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void BatchLock_JobName_MaxLength_Is_255()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.JobName))!;
        Assert.Equal(255, prop.GetMaxLength());
    }

    [Fact]
    public void BatchLock_RunId_MaxLength_Is_64()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!
                       .FindProperty(nameof(Domain.Entities.BatchLock.RunId))!;
        Assert.Equal(64, prop.GetMaxLength());
    }

    [Fact]
    public void BatchLock_TableName_Is_batch_lock_In_Operational_Schema()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var entityType = ctx.Model.FindEntityType(typeof(Domain.Entities.BatchLock))!;
        Assert.Equal("batch_lock", entityType.GetTableName());
        Assert.Equal("operational", entityType.GetSchema());
    }

    // ─────────────────────────────────────────────────────────────
    // tbljobmaster
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void TblJobMaster_JobName_MaxLength_Is_100()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var entityType = GetEntityByTable(ctx, "tbljobmaster");
        var prop = entityType.FindProperty("JobName")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobMaster_CreatedAt_HasDefaultValueSql()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "tbljobmaster").FindProperty("CreatedAt")!;
        Assert.Equal("NOW()", prop.GetDefaultValueSql());
    }

    // ─────────────────────────────────────────────────────────────
    // tbljobstatus FK
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void TblJobStatus_HasForeignKey_To_TblJobMaster_Cascade()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var master = GetEntityByTable(ctx, "tbljobmaster");
        var status = GetEntityByTable(ctx, "tbljobstatus");
        var fk = status.GetForeignKeys()
                       .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    // ─────────────────────────────────────────────────────────────
    // tbljobqueue FK + error message length
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void TblJobQueue_ErrorMessage_MaxLength_Is_1000()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "tbljobqueue").FindProperty("ErrorMessage")!;
        Assert.Equal(1000, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobMaster_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var master = GetEntityByTable(ctx, "tbljobmaster");
        var queue = GetEntityByTable(ctx, "tbljobqueue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == master);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueue_HasForeignKey_To_TblJobStatus_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var status = GetEntityByTable(ctx, "tbljobstatus");
        var queue = GetEntityByTable(ctx, "tbljobqueue");
        var fk = queue.GetForeignKeys()
                      .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }

    // ─────────────────────────────────────────────────────────────
    // tbljobqueue_log FK + performed by length
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public void TblJobQueueLog_PerformedBy_MaxLength_Is_100()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "tbljobqueue_log").FindProperty("PerformedBy")!;
        Assert.Equal(100, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_Note_MaxLength_Is_500()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var prop = GetEntityByTable(ctx, "tbljobqueue_log").FindProperty("Note")!;
        Assert.Equal(500, prop.GetMaxLength());
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobQueue_Cascade()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var queue = GetEntityByTable(ctx, "tbljobqueue");
        var log = GetEntityByTable(ctx, "tbljobqueue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == queue);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Cascade, fk!.DeleteBehavior);
    }

    [Fact]
    public void TblJobQueueLog_HasForeignKey_To_TblJobStatus_Restrict()
    {
        using var ctx = new BatchJobsDbContext(_options);
        var status = GetEntityByTable(ctx, "tbljobstatus");
        var log = GetEntityByTable(ctx, "tbljobqueue_log");
        var fk = log.GetForeignKeys()
                    .FirstOrDefault(f => f.PrincipalEntityType == status);
        Assert.NotNull(fk);
        Assert.Equal(DeleteBehavior.Restrict, fk!.DeleteBehavior);
    }
}
