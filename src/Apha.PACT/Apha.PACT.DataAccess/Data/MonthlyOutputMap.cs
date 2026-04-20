using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class MonthlyOutputMap : IEntityTypeConfiguration<MonthlyOutput>
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public MonthlyOutputMap(IFpsRequestContext fpsRequestContext)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public void Configure(EntityTypeBuilder<MonthlyOutput> entity)
        {
            entity.HasKey(e => new { e.TestCode, e.Buyer, e.Month, e.WorkGroup, e.FpsYear }).HasName("pk_monthlyoutput");

            entity.ToTable("monthlyoutput", "fps");

            entity.HasIndex(e => e.Month, "month");

            entity.HasIndex(e => e.WorkGroup, "monthlyoutput_workgroup");

            entity.HasIndex(e => new { e.TestCode, e.Buyer }, "reference14");

            entity.HasIndex(e => new { e.WorkGroup, e.TestCode }, "reference25");

            entity.HasIndex(e => e.TestCode, "testcode");

            entity.Property(e => e.TestCode)
                .HasColumnType("citext")
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasColumnType("citext")
                .HasColumnName("buyer");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.WorkGroup)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Volume).HasColumnName("volume");
            entity.Property(e => e.WgBuyer)
                .HasMaxLength(50)
                .HasColumnName("wgbuyer");
            entity.HasQueryFilter(e => e.FpsYear == _fpsRequestContext.FpsYear);
        }
    }
}
