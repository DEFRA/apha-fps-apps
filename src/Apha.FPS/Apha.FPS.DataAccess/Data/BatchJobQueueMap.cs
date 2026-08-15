using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class BatchJobQueueMap : IEntityTypeConfiguration<BatchJobQueue>
    {
        public void Configure(EntityTypeBuilder<BatchJobQueue> entity)
        {
            entity.HasKey(e => e.JobqueueId).HasName("job_queue_pkey");

            entity.ToTable("job_queue", "fps");

            entity.HasIndex(e => new { e.JobId, e.StartDateTime }, "idx_job_queue_jobid_startdatetime").IsDescending(false, true);
            entity.HasIndex(e => e.RequestedAtUtc, "idx_job_queue_requested_at_utc");

            entity.HasIndex(e => e.RequestedBy, "idx_job_queue_requestedby");

            entity.HasIndex(e => e.StatusId, "idx_job_queue_statusid");

            entity.HasIndex(e => e.JobExecutionId, "uq_job_queue_jobexecutionid").IsUnique();

            entity.Property(e => e.JobqueueId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("jobqueueid");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");

            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");

            entity.Property(e => e.ErrorMessage)
                .HasMaxLength(1000)
                .HasColumnName("errormessage");

            entity.Property(e => e.JobExecutionId).HasColumnName("jobexecutionid");
            
            entity.Property(e => e.JobId).HasColumnName("jobid");

            entity.Property(e => e.RequestedAtUtc).HasColumnName("requested_at_utc");

            entity.Property(e => e.RequestedBy)
                .HasMaxLength(256)
                .HasColumnName("requestedby");

            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime");
            
            entity.Property(e => e.StatusId).HasColumnName("statusid");

            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");

            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(256)
                .HasColumnName("approved_by");

            // Unlike every other timestamp column on this entity (requested_at_utc, startdatetime,
            // enddatetime, created_at, updated_at - all "timestamp with time zone"), these three are
            // "timestamp without time zone" in the live schema (confirmed 2026-08-15 against
            // batchjobs). Left to convention, Npgsql infers timestamptz for a plain DateTime
            // property; sending a timestamptz-typed value into a naive column triggers Postgres's
            // implicit cast using the session's TimeZone GUC, silently shifting the stored value
            // whenever that session isn't UTC - the exact corruption already confirmed for Bulk
            // Rates' approved_at_utc/triggered_at_utc (see
            // project_job_queue_approved_at_utc_timezone_bug memory). Declaring the real column
            // type here, paired with writing Kind=Unspecified UTC wall-clock values at the call
            // site (YearEndRepository), avoids the cast entirely instead of depending on the
            // session timezone happening to already be UTC.
            entity.Property(e => e.ApprovedAtUtc)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("approved_at_utc");

            entity.Property(e => e.RejectedBy)
                .HasMaxLength(256)
                .HasColumnName("rejected_by");

            entity.Property(e => e.RejectedAtUtc)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("rejected_at_utc");

            entity.Property(e => e.RejectionReason)
                .HasMaxLength(1000)
                .HasColumnName("rejection_reason");

            entity.Property(e => e.TriggeredBy)
                .HasMaxLength(256)
                .HasColumnName("triggered_by");

            entity.Property(e => e.TriggeredAtUtc)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("triggered_at_utc");
        }
    }
}
