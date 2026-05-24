using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PactStaffMap : IEntityTypeConfiguration<PactStaff>
    {
        public void Configure(EntityTypeBuilder<PactStaff> entity)
        {
            entity
               .HasNoKey()
               .ToView("vpacttblstaff", "fps");

            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
            entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
            entity.Property(e => e.Leave).HasColumnName("leave");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.PactId)
                .HasMaxLength(50)
                .HasColumnName("pactid");
            entity.Property(e => e.PersonClass)
                .HasMaxLength(10)
                .HasColumnName("personclass");
            entity.Property(e => e.PersonStatus)
                .HasMaxLength(10)
                .HasColumnName("personstatus");
            entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
            entity.Property(e => e.SpNumber)
                .HasMaxLength(10)
                .HasColumnName("spnumber");
            entity.Property(e => e.Title)
                .HasMaxLength(4)
                .HasColumnName("title");
            entity.Property(e => e.WorkGroupGrade)
                .HasMaxLength(50)
                .HasColumnName("workgroupgrade");
        }
    }
}
