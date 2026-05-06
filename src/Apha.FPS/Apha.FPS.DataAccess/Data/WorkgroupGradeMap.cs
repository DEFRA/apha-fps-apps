using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WorkgroupGradeMap : IEntityTypeConfiguration<WorkgroupGrade>
    {


        public void Configure(EntityTypeBuilder<WorkgroupGrade> entity)
        {
            entity.HasKey(e => new { e.WgGrade, e.FpsYear }).HasName("pk_workgroupgrade");

            entity.ToTable("workgroupgrade", "fps");

            entity.Property(e => e.WgGrade)
                .HasMaxLength(50)
                .HasColumnName("wggrade");
            entity.Property(e => e.AvSalary)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasColumnType("money")
                .HasColumnName("chargeratewg");
            entity.Property(e => e.DirectRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("directratewg");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("nprwg");
            entity.Property(e => e.OhrWg)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("ohrwg");
            entity.Property(e => e.PayRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("payratewg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasMaxLength(20)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.Workgroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
        }
    }
}
