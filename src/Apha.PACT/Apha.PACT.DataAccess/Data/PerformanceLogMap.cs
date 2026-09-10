using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class PerformanceLogMap : IEntityTypeConfiguration<PerformanceLog>
    {
        public void Configure(EntityTypeBuilder<PerformanceLog> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_performance_log");

            entity.ToTable("performance_log", "fps");

            entity.HasIndex(e => e.StartTimeUtc, "ix_performance_log_start_time_utc");
            entity.HasIndex(e => e.LogType, "ix_performance_log_log_type");
            entity.HasIndex(e => e.DurationMs, "ix_performance_log_duration_ms");

            entity.Property(e => e.Id)
                .UseIdentityByDefaultColumn()
                .HasColumnName("id");
            entity.Property(e => e.LogType)
                .HasMaxLength(20)
                .HasColumnName("log_type");
            entity.Property(e => e.StartTimeUtc)
                .HasColumnName("start_time_utc");
            entity.Property(e => e.EndTimeUtc)
                .HasColumnName("end_time_utc");
            entity.Property(e => e.DurationMs)
                .HasColumnName("duration_ms");
            entity.Property(e => e.CommandKind)
                .HasMaxLength(20)
                .HasColumnName("command_kind");
            entity.Property(e => e.CommandId)
                .HasMaxLength(50)
                .HasColumnName("command_id");
            entity.Property(e => e.CommandText)
                .HasColumnName("command_text");
            entity.Property(e => e.Parameters)
                .HasColumnName("parameters");
            entity.Property(e => e.ApiName)
                .HasMaxLength(50)
                .HasColumnName("api_name");
            entity.Property(e => e.HttpMethod)
                .HasMaxLength(10)
                .HasColumnName("http_method");
            entity.Property(e => e.StatusCode)
                .HasMaxLength(10)
                .HasColumnName("status_code");
            entity.Property(e => e.Outcome)
                .HasMaxLength(100)
                .HasColumnName("outcome");
            entity.Property(e => e.Url)
                .HasColumnName("url");
            entity.Property(e => e.CorrelationId)
                .HasMaxLength(100)
                .HasColumnName("correlation_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(now() at time zone 'utc')")
                .HasColumnName("created_at");
        }
    }
}
