using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class WgSummarisedStaffTimeUsageViewMap : IEntityTypeConfiguration<WgSummarisedStaffTimeUsageView>
    {
        public void Configure(EntityTypeBuilder<WgSummarisedStaffTimeUsageView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vwsummarisedstafftimeusage", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.JobCode)
                .HasMaxLength(50)
                .HasColumnName("jobcode");
            entity.Property(e => e.JobTitle)
                .HasColumnType("character varying")
                .HasColumnName("jobtitle");
            entity.Property(e => e.MonthName)
                .HasMaxLength(50)
                .HasColumnName("monthname");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ParentProject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
            entity.Property(e => e.TotalTime).HasColumnName("totaltime");
            entity.Property(e => e.UtFlag).HasColumnName("utflag");
            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wg_grade");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}