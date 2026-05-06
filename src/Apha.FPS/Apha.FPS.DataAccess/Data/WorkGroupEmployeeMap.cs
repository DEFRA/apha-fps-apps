using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WgEmployeeMap : IEntityTypeConfiguration<WorkGroupEmployee>
    {


        public void Configure(EntityTypeBuilder<WorkGroupEmployee> entity)
        {
            entity.HasKey(e => e.PactId).HasName("tblwgemployee_pk_tblwgemployee_1__10");

            entity.ToTable("tblwgemployee", "fps");

            entity.Property(e => e.PactId)
                .HasMaxLength(50)
                .HasColumnName("pactid");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.HoursPerWeek).HasColumnName("hoursperweek");
            entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.Leave).HasColumnName("leave");
            entity.Property(e => e.MakeAvailable)
                .HasDefaultValueSql("'-1'::integer")
                .HasColumnName("makeavailable");
            entity.Property(e => e.PersonClass)
                .HasMaxLength(10)
                .HasColumnName("personclass");
            entity.Property(e => e.PersonStatus)
                .HasMaxLength(10)
                .HasDefaultValueSql("'A'::character varying")
                .HasColumnName("personstatus");
            entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            entity.Property(e => e.SpNumber)
                .HasMaxLength(10)
                .HasColumnName("spnumber");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.TimeRecorder)
                .HasDefaultValue(0)
                .HasColumnName("timerecorder");
            entity.Property(e => e.WorkGroupGrade)
                .HasMaxLength(50)
                .HasColumnName("workgroupgrade");
        }
    }
}
