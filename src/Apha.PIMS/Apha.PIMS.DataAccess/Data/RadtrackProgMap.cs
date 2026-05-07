using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class RadtrackProgMap : IEntityTypeConfiguration<RadtrackProg>
    {
        public void Configure(EntityTypeBuilder<RadtrackProg> entity)
        {
            entity.HasKey(e => e.Program).HasName("pk_tblradtrackprog");

            entity.ToTable("tblradtrackprog", "mabarchive");

            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");
            entity.Property(e => e.Publicationprefix)
                .HasMaxLength(5)
                .HasColumnName("publicationprefix");
            entity.Property(e => e.Radtrackprog).HasColumnName("radtrackprog");
        }
    }
}
