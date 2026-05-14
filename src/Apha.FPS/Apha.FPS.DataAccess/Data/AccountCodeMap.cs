using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AccountCodeMap : IEntityTypeConfiguration<AccountCode>
    {
        public void Configure(EntityTypeBuilder<AccountCode> entity)
        {
            entity.HasKey(e => e.Code).HasName("pk_tlkpaccountcode");

            entity.ToTable("tlkpaccountcode", "fps");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .HasColumnName("description");
        }
    }
}
