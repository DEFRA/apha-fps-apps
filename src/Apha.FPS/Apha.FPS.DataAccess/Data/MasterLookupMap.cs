using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MasterLookupMap : IEntityTypeConfiguration<MasterLookup>
    {
        public void Configure(EntityTypeBuilder<MasterLookup> entity)
        {
            entity.HasKey(e => e.MasterTableName).HasName("pk_tblmasterlookup");

            entity.ToTable("tblmasterlookup", "fps");

            entity.Property(e => e.MasterTableName)
                .HasMaxLength(100)
                .HasColumnName("mastertablename");
        }
    }
}
