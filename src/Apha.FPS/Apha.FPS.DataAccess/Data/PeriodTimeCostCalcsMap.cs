using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PeriodTimeCostCalcsMap : IEntityTypeConfiguration<PeriodTimeCostCalcs>
    {
        public void Configure(EntityTypeBuilder<PeriodTimeCostCalcs> entity)
        {
            entity.ToTable("period_timecostcalcs", "fps");

            entity.HasKey(e => e.Id).HasName("pk_periotimecostcalcs_1");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasMaxLength(50).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasMaxLength(50).HasColumnName("subaccountcode");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.DefraProject).HasMaxLength(3).HasColumnName("defraproject");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Opc).HasMaxLength(50).HasColumnName("opc");
            entity.Property(e => e.Spc).HasMaxLength(50).HasColumnName("spc");
            entity.Property(e => e.Scc).HasColumnName("scc");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
            entity.Property(e => e.GradeCode).HasMaxLength(10).HasColumnName("gradecode");
            entity.Property(e => e.SpNumber).HasMaxLength(10).HasColumnName("spnumber");
            entity.Property(e => e.ChargeRate).HasColumnType("numeric(19,4)").HasColumnName("chargerate");
            entity.Property(e => e.Pay).HasColumnType("numeric(19,4)").HasColumnName("pay");
            entity.Property(e => e.NonPay).HasColumnType("numeric(19,4)").HasColumnName("nonpay");
            entity.Property(e => e.Overhead).HasColumnType("numeric(19,4)").HasColumnName("overhead");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.TotalCost).HasColumnType("numeric(19,4)").HasColumnName("totalcost");
        }
    }
}
