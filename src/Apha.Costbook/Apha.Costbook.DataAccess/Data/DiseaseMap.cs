using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class DiseaseMap : IEntityTypeConfiguration<Disease>
    {
        public void Configure(EntityTypeBuilder<Disease> entity)
        {
            entity.HasKey(e => e.DiseaseName).HasName("tbldisease_pk___4__10");

            entity.ToTable("tbldisease", DbConstants.FpsSchemaName);

            entity.Property(e => e.DiseaseName)
                .HasMaxLength(50)
                .HasColumnName("disease");
        }
    }
}
