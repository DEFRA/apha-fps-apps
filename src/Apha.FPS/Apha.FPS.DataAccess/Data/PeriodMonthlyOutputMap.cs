using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PeriodMonthlyOutputMap : IEntityTypeConfiguration<PeriodMonthlyOutput>
    {
        public void Configure(EntityTypeBuilder<PeriodMonthlyOutput> entity)
        {
            entity.ToTable("period_monthlyoutput", "fps");

            entity.HasKey(e => e.Id).HasName("pk_period_monthlyoutput_1");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasMaxLength(50).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasMaxLength(50).HasColumnName("subaccountcode");
            entity.Property(e => e.IsDefraProject).HasMaxLength(3).HasColumnName("isdefraproject");
            entity.Property(e => e.Opc).HasMaxLength(50).HasColumnName("opc");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Spc).HasMaxLength(50).HasColumnName("spc");
            entity.Property(e => e.WorkGroup).HasMaxLength(50).HasColumnName("workgroup");
            entity.Property(e => e.Scc).HasColumnName("scc");
            entity.Property(e => e.TestCode).HasMaxLength(20).HasColumnName("testcode");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.TestPrice).HasColumnType("money").HasColumnName("testprice");
            entity.Property(e => e.TotalCost).HasColumnType("money").HasColumnName("totalcost");
        }
    }
}
