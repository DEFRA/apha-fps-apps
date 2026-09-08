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
                .HasMaxLength(20)
                .HasColumnName("projectparent");
            entity.Property(e => e.Month)
                .HasMaxLength(10)
                .HasColumnName("month");
            entity.Property(e => e.Amount)
                .HasMaxLength(10)
                .HasColumnName("amount");
            entity.Property(e => e.CostOfWork)
                .HasMaxLength(10)
                .HasColumnName("costofwork");
            entity.Property(e => e.Wip)
                .HasMaxLength(10)
                .HasColumnName("wip");
            entity.Property(e => e.ProfitLoss)
                .HasMaxLength(10)
                .HasColumnName("profitloss");
            entity.Property(e => e.Detail)
                .HasMaxLength(255)
                .HasColumnName("detail");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
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
