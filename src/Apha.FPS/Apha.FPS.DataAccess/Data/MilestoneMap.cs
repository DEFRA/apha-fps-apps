using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MilestoneMap : IEntityTypeConfiguration<Milestone>
    {
        public void Configure(EntityTypeBuilder<Milestone> entity)
        {
            entity.HasKey(e => new { e.Project, e.MilestoneRef, e.ObjectiveRef })
                .HasName("pk_milestone_1__12");

            entity.ToTable("milestone", "fps");

            entity.Property(e => e.Project).HasColumnType("citext").HasColumnName("project");
            entity.Property(e => e.MilestoneRef).HasMaxLength(4).HasColumnName("milestoneref");
            entity.Property(e => e.ObjectiveRef).HasMaxLength(50).HasColumnName("objectiveref");
            entity.Property(e => e.MilestoneTitle).HasMaxLength(120).HasColumnName("milsetonetitle");
            entity.Property(e => e.PlanDate).HasColumnName("plandate");
            entity.Property(e => e.ActualDate).HasColumnName("actualdate");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.MonthNoFin).HasColumnName("monthnofin");
            entity.Property(e => e.Year).HasMaxLength(50).HasColumnName("year");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
