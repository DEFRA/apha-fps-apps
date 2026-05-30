using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.Costbook.DataAccess.Data
{
    public class AdditionalCostMap: IEntityTypeConfiguration<AdditionalCost>
    {
        public void Configure(EntityTypeBuilder<AdditionalCost> entity)
        {
            entity.HasKey(e => e.AcIdentity).HasName("pk_tbladditionalcosts");

            entity.ToTable("tbladditionalcosts", DbConstants.MabArchiveSchemaName);

            entity.HasIndex(e => e.Project, "idx_tbladditionalcosts_project");

            entity.HasIndex(e => new { e.Project, e.Year }, "idx_tbladditionalcosts_project_year");

            entity.Property(e => e.AcIdentity).HasColumnName("ac_identity");
            entity.Property(e => e.AccountCat)
                .HasMaxLength(50)
                .HasColumnName("accountcat");
            entity.Property(e => e.CostEntered).HasColumnName("costentered");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.Freq)
                .HasMaxLength(5)
                .HasColumnName("freq");
            entity.Property(e => e.ItemCost)
                .HasDefaultValueSql("0")
                .HasColumnName("itemcost");
            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.Year)
                .HasDefaultValue(0)
                .HasColumnName("year");
        }
    }
}
