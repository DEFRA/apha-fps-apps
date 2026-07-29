using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DepartmentIncomeAnimalMap : IEntityTypeConfiguration<DepartmentIncomeAnimal>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeAnimal> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_dept_income_animal", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            builder.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");

            builder.Property(e => e.DefraProject)
                .HasMaxLength(3)
                .HasColumnName("defraproject");

            builder.Property(e => e.OPC)
                .HasMaxLength(50)
                .HasColumnName("opc");

            builder.Property(e => e.OCC)
                .HasMaxLength(50)
                .HasColumnName("occ");

            builder.Property(e => e.Month)
                .HasColumnName("month");

            builder.Property(e => e.SPC)
                .HasMaxLength(50)
                .HasColumnName("spc");

            builder.Property(e => e.SCC)
                .HasMaxLength(10)
                .HasColumnName("scc");

            builder.Property(e => e.AnimalType)
                .HasMaxLength(50)
                .HasColumnName("animaltype");

            builder.Property(e => e.AnimalDays)
                .HasColumnName("animaldays");

            builder.Property(e => e.Rate)
                .HasColumnType("money")
                .HasColumnName("rate");

            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
