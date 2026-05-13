using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectInvoiceMap : IEntityTypeConfiguration<ProjectInvoice>
    {
        public void Configure(EntityTypeBuilder<ProjectInvoice> entity)
        {
            entity.HasKey(e => new { e.InvoiceCounter, e.FpsYear}).HasName("pk_proj_invoice");

            entity.ToTable("proj_invoice", "fps");

            entity.Property(e => e.InvoiceCounter)
                .ValueGeneratedOnAdd()
                .UseIdentityAlwaysColumn()
                .HasColumnName("invoicecounter");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.CostOfWork)
                .HasColumnType("money")
                .HasColumnName("costofwork");
            entity.Property(e => e.Detail)
                .HasMaxLength(100)
                .HasColumnName("detail");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ProfitLoss)
                .HasColumnType("money")
                .HasColumnName("profitloss");
            entity.Property(e => e.ProjectParent)
                .HasMaxLength(20)
                .HasColumnName("projectparent");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .HasColumnName("type");
            entity.Property(e => e.Wip)
                .HasColumnType("money")
                .HasColumnName("wip");
            entity.Property(e => e.X)
                .HasMaxLength(5)
                .HasColumnName("x");
        }
    }
}
