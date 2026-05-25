using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{

    public class WorkGroupEmployeeViewMap : IEntityTypeConfiguration<WorkGroupEmployeeView>
    {
        public void Configure(EntityTypeBuilder<WorkGroupEmployeeView> builder)
        {
            builder.HasNoKey();
            builder.ToView("vtblwgemployee", "fps");

            builder.Property(e => e.PactId).HasColumnName("pactid");
            builder.Property(e => e.SpNumber).HasColumnName("spnumber");
            builder.Property(e => e.WorkGroupGrade).HasColumnName("workgroupgrade");
            builder.Property(e => e.PersonStatus).HasColumnName("personstatus");
            builder.Property(e => e.PersonClass).HasColumnName("personclass");
            builder.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            builder.Property(e => e.Leave).HasColumnName("leave");
            builder.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            builder.Property(e => e.HrsAvail).HasColumnName("hrsavail");
            builder.Property(e => e.MakeAvailable).HasColumnName("makeavailable");
            builder.Property(e => e.TimeRecorder).HasColumnName("timerecorder");
            builder.Property(e => e.StartDate).HasColumnName("startdate");
            builder.Property(e => e.EndDate).HasColumnName("enddate");
            builder.Property(e => e.HoursPerWeek).HasColumnName("hoursperweek");
            builder.Property(e => e.FpsYear).HasColumnName("fpsyear");
            builder.Property(e => e.UserId).HasColumnName("user_id");
            builder.Property(e => e.Dt2Username).HasColumnName("dt2username");
            builder.Property(e => e.UserEmail).HasColumnName("useremail");
            builder.Ignore(e => e.Name);
        }
    }
}