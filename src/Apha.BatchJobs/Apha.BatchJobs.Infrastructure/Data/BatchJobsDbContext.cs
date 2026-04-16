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

        // Configure BatchLock table — mirrors operational.batch_lock
        modelBuilder.Entity<BatchLock>(entity =>
        {
            entity.ToTable("batch_lock", schema: "operational");
            entity.HasKey(e => e.LockId);
            entity.Property(e => e.LockId).HasColumnName("lock_id").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.AcquiredAt).HasColumnName("acquired_at").IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired().HasMaxLength(64);
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            entity.HasIndex(e => e.JobName).HasDatabaseName("idx_batch_lock_job_name");
            entity.HasIndex(e => new { e.JobName, e.IsActive }).HasDatabaseName("idx_batch_lock_job_name_active");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_batch_lock_expires_at");
        });

        // Configure foundation job master table — mirrors operational.tbljobmaster
        modelBuilder.Entity<TblJobMaster>(entity =>
        {
            entity.ToTable("tbljobmaster", schema: "operational");
            entity.HasKey(e => e.JobId);
            entity.Property(e => e.JobId).HasColumnName("jobid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("jobname").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(50);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(250);
            entity.Property(e => e.TimeToLive).HasColumnName("timetolive").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.JobName).IsUnique().HasDatabaseName("tbljobmaster_jobname_key");
        });

        // Configure foundation job status table — mirrors operational.tbljobstatus
        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.ToTable("tbljobstatus", schema: "operational");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.StatusId).HasColumnName("statusid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.JobId, e.Status }).IsUnique().HasDatabaseName("uq_tbljobstatus_jobid_status");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_tbljobstatus_jobid")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure foundation job queue table — mirrors operational.tbljobqueue
        modelBuilder.Entity<TblJobQueue>(entity =>
        {
            entity.ToTable("tbljobqueue", schema: "operational");
            entity.HasKey(e => e.JobQueueId);
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime").IsRequired();
            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");
            entity.Property(e => e.ErrorMessage).HasColumnName("errormessage").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_tbljobqueue_jobid")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_tbljobqueue_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure foundation job queue log table — mirrors operational.tbljobqueue_log
        modelBuilder.Entity<TblJobQueueLog>(entity =>
        {
            entity.ToTable("tbljobqueue_log", schema: "operational");
            entity.HasKey(e => e.JobQueueLogId);
            entity.Property(e => e.JobQueueLogId).HasColumnName("jobqueuelogid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.PerformedBy).HasColumnName("performedby").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogTime).HasColumnName("logtime").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.HasOne<TblJobQueue>()
                  .WithMany()
                  .HasForeignKey(e => e.JobQueueId)
                  .HasConstraintName("fk_tbljobqueue_log_jobqueueid")
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_tbljobqueue_log_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
