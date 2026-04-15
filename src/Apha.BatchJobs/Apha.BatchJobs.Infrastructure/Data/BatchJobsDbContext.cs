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
    /// Gets or sets the foundation job master table.
    /// </summary>
    internal DbSet<TblJobMaster> TblJobMaster { get; set; }

    /// <summary>
    /// Gets or sets the foundation job status table.
    /// </summary>
    internal DbSet<TblJobStatus> TblJobStatus { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue table.
    /// </summary>
    internal DbSet<TblJobQueue> TblJobQueue { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue log table.
    /// </summary>
    internal DbSet<TblJobQueueLog> TblJobQueueLog { get; set; }

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

        // Configure foundation job master table
        modelBuilder.Entity<TblJobMaster>(entity =>
        {
            entity.ToTable("tbljobmaster", schema: "operational");
            entity.HasKey(e => e.JobId);
            entity.Property(e => e.JobId).HasColumnName("jobid");
            entity.Property(e => e.JobName).HasColumnName("jobname").IsRequired();
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.TimeToLive).HasColumnName("timetolive");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(e => e.JobName).IsUnique(true);
        });

        // Configure foundation job status table
        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.ToTable("tbljobstatus", schema: "operational");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.StatusId).HasColumnName("statusid");
            entity.Property(e => e.JobId).HasColumnName("jobid");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(e => new { e.JobId, e.Status }).IsUnique(true);
        });

        // Configure foundation job queue table
        modelBuilder.Entity<TblJobQueue>(entity =>
        {
            entity.ToTable("tbljobqueue", schema: "operational");
            entity.HasKey(e => e.JobQueueId);
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid");
            entity.Property(e => e.JobId).HasColumnName("jobid");
            entity.Property(e => e.StatusId).HasColumnName("statusid");
            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime");
            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");
            entity.Property(e => e.ErrorMessage).HasColumnName("errormessage");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // Configure foundation job queue log table
        modelBuilder.Entity<TblJobQueueLog>(entity =>
        {
            entity.ToTable("tbljobqueue_log", schema: "operational");
            entity.HasKey(e => e.JobQueueLogId);
            entity.Property(e => e.JobQueueLogId).HasColumnName("jobqueuelogid");
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid");
            entity.Property(e => e.StatusId).HasColumnName("statusid");
            entity.Property(e => e.PerformedBy).HasColumnName("performedby").IsRequired();
            entity.Property(e => e.LogTime).HasColumnName("logtime");
            entity.Property(e => e.Note).HasColumnName("note");
        });
    }
}
