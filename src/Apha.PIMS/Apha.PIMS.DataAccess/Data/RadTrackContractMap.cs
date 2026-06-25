using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class RadTrackContractMap : IEntityTypeConfiguration<RadTrackContract>
    {
        public void Configure(EntityTypeBuilder<RadTrackContract> entity)
        {
            entity.HasKey(e => e.Contract).HasName("pk_tblradtrackcontract");

            entity.ToTable("tblradtrackcontract", "mabarchive");

            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasColumnName("contract");
        }
    }
}
