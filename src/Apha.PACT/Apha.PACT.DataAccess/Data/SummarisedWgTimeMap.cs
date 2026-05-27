using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class SummarisedWgTimeMap : IEntityTypeConfiguration<SummarisedWgTimeView>
    {
        public void Configure(EntityTypeBuilder<SummarisedWgTimeView> entity)
        {
            entity.ToTable("vwsummarisedstafftimeusage", "fps");
            entity.HasNoKey();

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.MonthName).HasColumnName("monthname");
            entity.Property(e => e.ProfitCentre).HasColumnName("profitcentre");
            entity.Property(e => e.WorkGroup).HasColumnName("workgroup");
            entity.Property(e => e.WgGrade).HasColumnName("wg_grade");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.ParentProject).HasColumnName("parentproject");
            entity.Property(e => e.JobCode).HasColumnName("jobcode");
            entity.Property(e => e.JobTitle).HasColumnName("jobtitle");
            entity.Property(e => e.UtFlag).HasColumnName("utflag");
            entity.Property(e => e.TotalTime).HasColumnName("totaltime");
            entity.Property(e => e.TotalCost).HasColumnName("totalcost");
        }
    }
}
