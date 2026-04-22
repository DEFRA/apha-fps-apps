using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StatusMap : IEntityTypeConfiguration<Status>
    {
        public void Configure(EntityTypeBuilder<Status> entity)
        {
            entity.HasKey(e => e.StatusValue).HasName("tblstatus_pk___3__10");

            entity.ToTable("tblstatus", "fps");

            entity.Property(e => e.StatusValue)
                .HasMaxLength(50)
                .HasColumnName("status");
        }
    }
}
