using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class SubAccountMap : IEntityTypeConfiguration<SubAccount>
    {
        public void Configure(EntityTypeBuilder<SubAccount> entity)
        {
            entity.HasKey(e => e.SubAccountCode).HasName("tlkpsubaccount_pk_tlkpsubaccount");

            entity.ToTable("tlkpsubaccount", "fps");

            entity.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");
            entity.Property(e => e.SubAccountName)
                .HasMaxLength(50)
                .HasColumnName("subaccount");
        }
    }
}
