using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class RecreateSummariesLogMap : IEntityTypeConfiguration<RecreateSummariesLog>
    {
        public void Configure(EntityTypeBuilder<RecreateSummariesLog> entity)
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("recreatesummaries_log", "fps");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnName("userid");

            entity.Property(e => e.Period)
                .HasColumnName("period");

            entity.Property(e => e.DateDone)
                .HasColumnName("datedone");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");

            entity.HasOne(e => e.User)
                .WithMany(e => e.Logs)
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(e => e.UserName)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
