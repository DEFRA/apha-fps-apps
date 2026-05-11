using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.Costbook.DataAccess.Data
{
    public class ProjectYearMap : IEntityTypeConfiguration<ProjectYear>
    {
        public void Configure(EntityTypeBuilder<ProjectYear> entity)
        {
            entity.HasKey(e => new { e.Project, e.YearValue }).HasName("aaaaatblprojectyear_pk");

            entity.ToTable("tblprojectyear", DbConstants.MabArchiveSchemaName);

            entity.HasIndex(e => e.Project, "tblprojecttblprojectyear");

            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.YearValue).HasColumnName("yearno");
            entity.Property(e => e.MarkupAdditional).HasColumnName("markup_additional");
            entity.Property(e => e.MarkupAnimals).HasColumnName("markup_animals");
            entity.Property(e => e.MarkupTests).HasColumnName("markup_tests");
            entity.Property(e => e.MarkupTime).HasColumnName("markup_time");
            entity.Property(e => e.ProfitAdditional).HasColumnName("profit_additional");
            entity.Property(e => e.ProfitAnimals).HasColumnName("profit_animals");
            entity.Property(e => e.ProfitTests).HasColumnName("profit_tests");
            entity.Property(e => e.ProfitTime).HasColumnName("profit_time");

        }
    }
}
