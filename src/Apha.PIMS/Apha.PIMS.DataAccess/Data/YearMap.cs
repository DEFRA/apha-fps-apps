using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class YearMap : IEntityTypeConfiguration<Year>
    {
        public void Configure(EntityTypeBuilder<Year> entity)
        {
            entity.HasKey(e => e.Value).HasName("pk_tlkpyear");

            entity.ToTable("tlkpyear", "mabarchive");

            entity.Property(e => e.Value)
                .ValueGeneratedNever()
                .HasColumnName("year");
            entity.Property(e => e.Latestmonthreleased).HasColumnName("latestmonthreleased");
        }
    }
}
