using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectMonthFinalMap : IEntityTypeConfiguration<ProjectMonthFinal>
    {
        public void Configure(EntityTypeBuilder<ProjectMonthFinal> entity)
        {
            entity.HasKey(e => new { e.Year, e.Project, e.Monthno }).HasName("pk_my_projectmonthfinal");

            entity.ToTable("my_projectmonthfinal", "mabarchive");

            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Monthno).HasColumnName("monthno");
            entity.Property(e => e.Periodname)
                .HasMaxLength(50)
                .HasColumnName("periodname");
            entity.Property(e => e.Subcontracts)
                .HasColumnType("money")
                .HasColumnName("subcontracts");
            entity.Property(e => e.Nonanimals)
                .HasColumnType("money")
                .HasColumnName("nonanimals");
            entity.Property(e => e.Animals)
                .HasColumnType("money")
                .HasColumnName("animals");
            entity.Property(e => e.Timecosts)
                .HasColumnType("money")
                .HasColumnName("timecosts");
            entity.Property(e => e.Transfercosts)
                .HasColumnType("money")
                .HasColumnName("transfercosts");
            entity.Property(e => e.Totalcost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
            entity.Property(e => e.Totalhours).HasColumnName("totalhours");
            entity.Property(e => e.Invoices)
                .HasColumnType("money")
                .HasColumnName("invoices");
            entity.Property(e => e.Coiw)
                .HasColumnType("money")
                .HasColumnName("coiw");
        }
    }
}
