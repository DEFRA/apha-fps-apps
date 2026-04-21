using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class StaffJobMap : IEntityTypeConfiguration<StaffJob>
    {


        public void Configure(EntityTypeBuilder<StaffJob> entity)
        {
            entity.HasKey(e => new { e.StaffId, e.JobCode }).HasName("tblstaffjob_pk__tblstaffjob__30392ede");

            entity.ToTable("tblstaffjob", "fps");

            entity.Property(e => e.StaffId)
                .HasMaxLength(50)
                .HasColumnName("staffid");
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
        }
    }
}
