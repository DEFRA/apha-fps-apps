using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestCapabilityMap : IEntityTypeConfiguration<TestCapability>
    {
        public void Configure(EntityTypeBuilder<TestCapability> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.WorkGroup, e.FpsYear })
                .HasName("pk_tlkptestcapability");

            entity.ToTable("tlkptestcapability", "fps");

            entity.Property(e => e.TestCode)
                .HasColumnType("citext")
                .HasColumnName("testcode");
            entity.Property(e => e.WorkGroup)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlanPortfolio)
                .HasColumnType("citext")
                .HasColumnName("planportfolio");
            entity.Property(e => e.PredOutturn)
                .HasDefaultValueSql("0")
                .HasColumnName("predoutturn");
            entity.Property(e => e.SmsCode)
                .HasMaxLength(50)
                .HasColumnName("smscode");
            entity.Property(e => e.Sop)
                .HasMaxLength(50)
                .HasColumnName("sop");
            entity.Property(e => e.UnitCost)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("unitcost");
        }
    }
}
