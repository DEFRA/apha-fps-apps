using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TimeCostCalcMap : IEntityTypeConfiguration<TimeCostCalc>
    {
        public void Configure(EntityTypeBuilder<TimeCostCalc> entity)
        {
            entity.HasKey(e => new { e.WorkGroup, e.JobCode, e.Project, e.Month, e.StaffId })
                .HasName("aaaaatimecostcalcs_pk");

            entity.ToTable("timecostcalcs", "fps");

            entity.Property(e => e.WorkGroup).HasMaxLength(50).HasColumnName("workgroup");
            entity.Property(e => e.JobCode).HasMaxLength(50).HasColumnName("jobcode");
            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.StaffId).HasMaxLength(50).HasColumnName("staffid");
            entity.Property(e => e.GradeCode).HasMaxLength(10).HasColumnName("gradecode");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
            entity.Property(e => e.ChargeRate).HasColumnType("money").HasColumnName("chargerate");
            entity.Property(e => e.Class).HasMaxLength(255).HasColumnName("class");
            entity.Property(e => e.Time).HasColumnName("time");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division).HasMaxLength(10).HasColumnName("division");
            entity.Property(e => e.JobCodeOld).HasMaxLength(14).HasColumnName("jobcodeold");
            entity.Property(e => e.Pay).HasColumnType("money").HasColumnName("pay");
            entity.Property(e => e.NonPay).HasColumnType("money").HasColumnName("nonpay");
            entity.Property(e => e.Overhead).HasColumnType("money").HasColumnName("overhead");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
