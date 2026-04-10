using Apha.BatchJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// Database context for batch jobs operational schema.
/// </summary>
public class BatchJobsDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BatchJobsDbContext.
    /// </summary>
    /// <param name="options">DbContext configuration options.</param>
    public BatchJobsDbContext(DbContextOptions<BatchJobsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the batch locks table.
    /// </summary>
    public DbSet<BatchLock> BatchLocks { get; set; }

    /// <summary>
    /// Gets or sets the job execution records table.
    /// </summary>
    public DbSet<JobExecutionRecord> JobExecutionRecords { get; set; }

    /// <summary>
    /// Configures the model for the database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BatchLock table
        modelBuilder.Entity<BatchLock>(entity =>
        {
            entity.ToTable("batch_lock", schema: "operational");
            entity.HasKey(e => e.LockId);
            entity.Property(e => e.LockId).HasColumnName("lock_id");
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired();
            entity.Property(e => e.AcquiredAt).HasColumnName("acquired_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.HasIndex(e => e.JobName).IsUnique(false);
        });

        // Configure JobExecutionRecord table
        modelBuilder.Entity<JobExecutionRecord>(entity =>
        {
            entity.ToTable("job_execution_record", schema: "operational");
            entity.HasKey(e => e.ExecutionId);
            entity.Property(e => e.ExecutionId).HasColumnName("execution_id");
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired();
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.JobType).HasColumnName("job_type");
            entity.Property(e => e.RunMode).HasColumnName("run_mode");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.DurationSeconds).HasColumnName("duration_seconds");
            entity.Property(e => e.RecordsProcessed).HasColumnName("records_processed");
            entity.Property(e => e.RecordsFailed).HasColumnName("records_failed");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.StackTrace).HasColumnName("stack_trace");
            entity.Property(e => e.RetryAttempts).HasColumnName("retry_attempts");
            entity.HasIndex(e => e.RunId).IsUnique(true);
            entity.HasIndex(e => e.JobName).IsUnique(false);
        });
    }
}
