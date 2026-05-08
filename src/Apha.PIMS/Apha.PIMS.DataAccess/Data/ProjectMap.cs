using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class ProjectMap : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> entity)
        {
            entity.HasKey(e => e.Parentproject).HasName("pk_g_tlkpproject");

            entity.ToTable("g_tlkpproject", "mabarchive");

            entity.Property(e => e.Parentproject)
                .HasMaxLength(20)
                .HasColumnName("parentproject");
            entity.Property(e => e.Contract)
                .HasMaxLength(10)
                .HasColumnName("contract");
            entity.Property(e => e.Costbookno)
                .HasMaxLength(50)
                .HasColumnName("costbookno");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Projectstatus)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");
            entity.Property(e => e.Projecttitle)
                .HasMaxLength(200)
                .HasColumnName("projecttitle");
            entity.Property(e => e.Shorttitle)
                .HasMaxLength(30)
                .HasColumnName("shorttitle");
        }
    }
}
