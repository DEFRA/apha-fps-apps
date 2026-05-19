using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProposedProjectMap : IEntityTypeConfiguration<ProposedProject>
    {
        public void Configure(EntityTypeBuilder<ProposedProject> entity)
        {
            entity.HasKey(e => e.Id).HasName("pk_tblproposedproject");

            entity.ToTable("tblproposedproject", "mabarchive");

            entity.HasIndex(e => e.Parentproject, "uq_tblproposedproject_parentproject").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Costbookno)
                .HasMaxLength(50)
                .HasColumnName("costbookno");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
            entity.Property(e => e.Projectstatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.Projecttitle)
                .HasMaxLength(200)
                .HasColumnName("projecttitle");
            entity.Property(e => e.Reason)
                .HasMaxLength(250)
                .HasColumnName("reason");
        }
    }
}
