using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class StaffMap : IEntityTypeConfiguration<Staff>
    {
        public void Configure(EntityTypeBuilder<Staff> entity)
        {
            entity.HasKey(e => e.Mnumber).HasName("pk_tblcapsstaff");

            entity.ToTable("tblcapsstaff", DbConstants.MabArchiveSchemaName);

            entity.Property(e => e.Mnumber)
                .HasMaxLength(50)
                .HasColumnName("mnumber");
            entity.Property(e => e.Dt2number)
                .HasMaxLength(50)
                .HasColumnName("dt2number");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        }
    }
}
