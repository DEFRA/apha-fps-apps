using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class RadTrackInvoiceMap : IEntityTypeConfiguration<RadTrackInvoice>
    {
        public void Configure(EntityTypeBuilder<RadTrackInvoice> entity)
        {
            entity.HasKey(e => e.InvoiceCounter).HasName("pk_tblradtrackinvoice");

            entity.ToTable("tblradtrackinvoice", "mabarchive");

            entity.Property(e => e.InvoiceCounter)
                .HasColumnName("invoicecounter")
                .ValueGeneratedOnAdd().UseIdentityByDefaultColumn();
            entity.Property(e => e.ActualAmount).HasColumnName("actualamount");
            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasColumnName("contract");
            entity.Property(e => e.DateInvoiced)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateinvoiced");
            entity.Property(e => e.DateJobsheetRaised)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datejobsheetraised");
            entity.Property(e => e.DueAmount).HasColumnName("dueamount");
            entity.Property(e => e.DueDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("duedate");
            entity.Property(e => e.InvoicePaid)
                .HasDefaultValue((short)0)
                .HasColumnName("invoicepaid");
            entity.Property(e => e.InvoiceRef)
                .HasMaxLength(50)
                .HasColumnName("invoiceref");
            entity.Property(e => e.PlannedAmount).HasColumnName("plannedamount");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
        }
    }
}
