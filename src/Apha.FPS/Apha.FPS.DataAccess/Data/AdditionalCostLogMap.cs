using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AdditionalCostLogMap : IEntityTypeConfiguration<AdditionalCostLog>
    {
        public void Configure(EntityTypeBuilder<AdditionalCostLog> entity)
        {
            entity.HasKey(e => e.SequenceNo).HasName("pk_additionalcosts_log");

            entity.ToTable("additionalcosts_log", "fps");

            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.ItemCost)
                .HasColumnType("numeric(18,2)")
                .HasColumnName("itemcost");
            entity.Property(e => e.Freq)
                .HasMaxLength(10)
                .HasColumnName("freq");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
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
