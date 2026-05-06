using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class EmployeeMap : IEntityTypeConfiguration<Employee>
    {


        public void Configure(EntityTypeBuilder<Employee> entity)
        {
            entity.HasKey(e => new { e.SPNumber, e.FpsYear }).HasName("pk_tblemployee");

            entity.ToTable("tblemployee", "fps");

            entity.Property(e => e.SPNumber)
                .HasMaxLength(10)
                .HasColumnName("spnumber");
            entity.Property(e => e.FirstName)
                .HasMaxLength(20)
                .HasColumnName("firstname");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.LastName)
                .HasMaxLength(20)
                .HasColumnName("lastname");
            entity.Property(e => e.Title)
                .HasMaxLength(4)
                .HasColumnName("title");
        }
    }
}
