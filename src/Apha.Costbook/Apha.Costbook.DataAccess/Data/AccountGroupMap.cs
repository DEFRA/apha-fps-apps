using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class AccountGroupMap : IEntityTypeConfiguration<AccountGroup>
    {
        public void Configure(EntityTypeBuilder<AccountGroup> entity)
        {
            entity.HasKey(e => e.Csg7group).HasName("pk_tblcsg7_accountgroups");

            entity.ToTable("tblcsg7_accountgroups", DbConstants.MabArchiveSchemaName);

            entity.Property(e => e.Csg7group)
                .HasMaxLength(15)
                .HasColumnName("csg7group");
            entity.Property(e => e.Useinflation)
                .HasDefaultValue(true)
                .HasColumnName("useinflation");
        }
    }
}
