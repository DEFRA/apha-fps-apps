using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ReleaseSummaryMonthClosureMap : IEntityTypeConfiguration<ReleaseSummaryMonthClosure>
    {
        public void Configure(EntityTypeBuilder<ReleaseSummaryMonthClosure> entity)
        {
            entity.HasKey(e => e.Key).HasName("pk_tbldb_variables");

            entity.ToTable("tbldb_variables", "fps");

            entity.Property(e => e.Key)
                .HasMaxLength(20)
                .HasColumnName("db_var_name");
            entity.Property(e => e.Value)
                .HasMaxLength(20)
                .HasColumnName("db_var_value");
        }
    }
}
