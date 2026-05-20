using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MilestoneMap : IEntityTypeConfiguration<Milestone>
    {
        public void Configure(EntityTypeBuilder<Milestone> entity)
        {
            entity.HasKey(e => new { e.Project, e.MilestoneRef, e.ObjectiveRef, e.FpsYear }).HasName("pk_milestone");

            entity.ToTable("milestone", "fps", tb => tb.HasComment("Milestone information"));

            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasComment("Project identifier")
                .HasColumnName("project");
            entity.Property(e => e.MilestoneRef)
                .HasMaxLength(4)
                .HasComment("Milestone reference")
                .HasColumnName("milestoneref");
            entity.Property(e => e.ObjectiveRef)
                .HasMaxLength(50)
                .HasComment("Objective reference")
                .HasColumnName("objectiveref");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ActualDate)
                .HasComment("Actual date")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("actualdate");
            entity.Property(e => e.Comment)
                .HasComment("Additional comments")
                .HasColumnName("comment");
            entity.Property(e => e.MilestoneTitle)
                .HasMaxLength(120)
                .HasComment("Milestone title")
                .HasColumnName("milestonetitle");
            entity.Property(e => e.MonthNoFin)
                .HasComment("Month number (financial)")
                .HasColumnName("monthnofin");
            entity.Property(e => e.PlanDate)
                .HasComment("Planned date")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("plandate");
            entity.Property(e => e.Year)
                .HasMaxLength(50)
                .HasComment("Year")
                .HasColumnName("year");

        }
    }
}
