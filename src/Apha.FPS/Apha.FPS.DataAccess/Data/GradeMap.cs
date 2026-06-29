using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    /// <summary>
    /// EF Core entity type configuration for the Grade entity.
    /// Maps to fps.grade (partitioned by fpsyear). Composite PK: (gradecode, fpsyear).
    /// </summary>
    public class GradeMap : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> entity)
        {
            // TRANSFORMENGINE: Fixed composite PK — DDL defines PRIMARY KEY (gradecode, fpsyear)
            entity.HasKey(e => new { e.GradeCode, e.FpsYear }).HasName("pk_grade");

            entity.ToTable("grade", "fps");

            entity.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            // TRANSFORMENGINE: Added DescLong mapping — fps.grade.desc_long varchar(30)
            entity.Property(e => e.DescLong)
                .HasMaxLength(30)
                .HasColumnName("desc_long");

            // TRANSFORMENGINE: Added AvSalary mapping — fps.grade.avsalary money DEFAULT 0
            entity.Property(e => e.AvSalary)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("avsalary");

            // TRANSFORMENGINE: Added PactCode mapping — fps.grade.pactcode varchar(50)
            entity.Property(e => e.PactCode)
                .HasMaxLength(50)
                .HasColumnName("pactcode");

            // TRANSFORMENGINE: Added AvLeaveHrs mapping — fps.grade.avleavehrs double precision DEFAULT 0
            entity.Property(e => e.AvLeaveHrs)
                .HasDefaultValueSql("0")
                .HasColumnName("avleavehrs");

            // TRANSFORMENGINE: Added AvSickHrs mapping — fps.grade.avsickhrs double precision DEFAULT 0
            entity.Property(e => e.AvSickHrs)
                .HasDefaultValueSql("0")
                .HasColumnName("avsickhrs");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
