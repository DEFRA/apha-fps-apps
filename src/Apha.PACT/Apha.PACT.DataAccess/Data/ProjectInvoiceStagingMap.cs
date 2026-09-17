using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ProjectInvoiceStagingMap : IEntityTypeConfiguration<ProjectInvoiceStaging>
    {
        public void Configure(EntityTypeBuilder<ProjectInvoiceStaging> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_proj_invoice_staging");

            entity.ToTable("proj_invoice_staging", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectParent)
                .HasColumnName("projectparent");
            entity.Property(e => e.Month)
                .HasColumnName("month");
            entity.Property(e => e.Amount)
                .HasColumnName("amount");
            entity.Property(e => e.CostOfWork)
                .HasColumnName("costofwork");
            entity.Property(e => e.Wip)
                .HasColumnName("wip");
            entity.Property(e => e.ProfitLoss)
                .HasColumnName("profitloss");
            entity.Property(e => e.Detail)
                .HasColumnName("detail");
            entity.Property(e => e.Type)
                .HasColumnName("type");
            entity.Property(e => e.Filename)
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.ImportedBy)
                .HasMaxLength(255)
                .HasColumnName("importedby");
            entity.Property(e => e.ImportedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("importeddate");
            entity.Property(e => e.ValidationFailure).HasColumnName("validationfailure");
            entity.Property(e => e.IsExported)
                .HasDefaultValue(false)
                .HasColumnName("isexported");
            entity.Property(e => e.IsPassed)
                .HasDefaultValue(false)
                .HasColumnName("ispassed");
        }
    }
}
