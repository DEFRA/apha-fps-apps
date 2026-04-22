using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRequirementMap : IEntityTypeConfiguration<TestRequirement>
    {
        public void Configure(EntityTypeBuilder<TestRequirement> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.FpsYear });

            entity.ToTable("tlkptestreqmt", "fps");

            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasColumnType("citext")
                .HasColumnName("buyer");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("money")
                .HasColumnName("unitprice");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode)
                .HasColumnType("citext")
                .HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("testbuyercode");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
