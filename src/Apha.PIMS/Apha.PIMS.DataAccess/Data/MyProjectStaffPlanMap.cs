using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class MyProjectStaffPlanMap : IEntityTypeConfiguration<MyProjectStaffPlan>
    {
        public void Configure(EntityTypeBuilder<MyProjectStaffPlan> entity)
        {
            entity
                .HasNoKey()
                .ToView("vmy_projectstaffplan", "mabarchive");

            entity.Property(e => e.Cost)
                .HasColumnType("money")
                .HasColumnName("cost");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Pcgrade)
                .HasMaxLength(20)
                .HasColumnName("pcgrade");
            entity.Property(e => e.Plannedhours).HasColumnName("plannedhours");
            entity.Property(e => e.Rate)
                .HasColumnType("money")
                .HasColumnName("rate");
            entity.Property(e => e.Workgroupgrade)
                .HasMaxLength(50)
                .HasColumnName("workgroupgrade");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}
