using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class TimeCostCalcsMap : IEntityTypeConfiguration<TimeCostCalcs>
    {
        public void Configure(EntityTypeBuilder<TimeCostCalcs> entity)
        {
            entity.HasKey(e => new { e.Year, e.Workgroup, e.Jobcode, e.Project, e.Month, e.Staffid }).HasName("pk_my_timecostcalcs");

            entity.ToTable("my_timecostcalcs", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Workgroup)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.Jobcode)
                .HasMaxLength(50)
                .HasColumnName("jobcode");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Staffid)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.Chargerate)
                .HasColumnType("money")
                .HasColumnName("chargerate");
            entity.Property(e => e.Class)
                .HasMaxLength(255)
                .HasColumnName("class");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Division)
                .HasMaxLength(10)
                .HasColumnName("division");
            entity.Property(e => e.Gradecode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");
            entity.Property(e => e.Jobcodeold)
                .HasMaxLength(14)
                .HasColumnName("jobcodeold");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Nonpay)
                .HasColumnType("money")
                .HasColumnName("nonpay");
            entity.Property(e => e.Overhead)
                .HasColumnType("money")
                .HasColumnName("overhead");
            entity.Property(e => e.Pay)
                .HasColumnType("money")
                .HasColumnName("pay");
            entity.Property(e => e.Time).HasColumnName("time");
        }
    }
}
