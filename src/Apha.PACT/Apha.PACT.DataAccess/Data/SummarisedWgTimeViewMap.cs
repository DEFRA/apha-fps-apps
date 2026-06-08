using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class SummarisedWgTimeViewMap : IEntityTypeConfiguration<SummarisedWgTimeView>
    {
        public void Configure(EntityTypeBuilder<SummarisedWgTimeView> entity)
        {
            entity
               .HasNoKey()
               .ToView("vwsummarisedwgtimeusage", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.MonthName)
                .HasMaxLength(50)
                .HasColumnName("monthname");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.ProjectTitle)
                .HasMaxLength(200)
                .HasColumnName("projecttitle");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.TotalTime).HasColumnName("totaltime");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
