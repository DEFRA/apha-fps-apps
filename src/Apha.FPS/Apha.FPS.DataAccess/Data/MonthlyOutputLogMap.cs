using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class MonthlyOutputLogMap : IEntityTypeConfiguration<MonthlyOutputLog>
    {
        public void Configure(EntityTypeBuilder<MonthlyOutputLog> entity)
        {
            entity.HasKey(e => e.SequenceNo).HasName("pk_mo_log");

            entity.ToTable("mo_log", "fps");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.WorkGroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.WgBuyer)
                .HasMaxLength(70)
                .HasColumnName("wgbuyer");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("user_id");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
