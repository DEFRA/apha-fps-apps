using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DepartmentIncomeTotalsMap : IEntityTypeConfiguration<DepartmentIncomeTotals>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeTotals> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_dept_income_totals", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            builder.Property(e => e.TotalCosts)
                .HasColumnType("money")
                .HasColumnName("totalcosts");

            builder.Property(e => e.TimeCost)
                .HasColumnType("money")
                .HasColumnName("timecost");

            builder.Property(e => e.TestsCost)
                .HasColumnType("money")
                .HasColumnName("testscost");

            builder.Property(e => e.AnimalsCost)
                .HasColumnType("money")
                .HasColumnName("animalscost");

            builder.Property(e => e.ProjectSpecificsCost)
                .HasColumnType("money")
                .HasColumnName("projectspecificscost");
        }
    }
}
