using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PeriodProjSubContractMap : IEntityTypeConfiguration<PeriodProjSubContract>
    {
        public void Configure(EntityTypeBuilder<PeriodProjSubContract> entity)
        {
            entity.ToTable("period_proj_subcontract", "fps");

            entity.HasKey(e => new { e.Period, e.SubContCounter })
                  .HasName("pk_period_proj_subcontract");

            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.SubContCounter).HasColumnName("subcontcounter");
            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.OracleProjectCode).HasMaxLength(50).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasMaxLength(50).HasColumnName("subaccountcode");
            entity.Property(e => e.IsDefraProject).HasMaxLength(3).HasColumnName("isdefraproject");
            entity.Property(e => e.Opc).HasMaxLength(50).HasColumnName("opc");
            entity.Property(e => e.Occ).HasColumnName("occ");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Amount).HasColumnType("money").HasColumnName("amount");
            entity.Property(e => e.AcctCode).HasMaxLength(30).HasColumnName("acctcode");
        }
    }
}
