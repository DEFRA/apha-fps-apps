using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class EuGradeConversionMap : IEntityTypeConfiguration<EuGradeConversion>
    {
        public void Configure(EntityTypeBuilder<EuGradeConversion> entity)
        {
            entity.HasKey(e => e.VlaGrade).HasName("pk_tbleugrade_conversion");

            entity.ToTable("tbleugrade_conversion", DbConstants.MabArchiveSchemaName);

            entity.Property(e => e.VlaGrade)
                .HasMaxLength(50)
                .HasColumnName("vlagrade");

            entity.Property(e => e.EuGrade)
                .HasMaxLength(50)
                .HasColumnName("eugrade");
        }
    }
}
