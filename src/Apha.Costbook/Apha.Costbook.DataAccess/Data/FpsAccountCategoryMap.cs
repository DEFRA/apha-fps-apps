using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class FpsAccountCategoryMap : IEntityTypeConfiguration<FpsAccountCategory>
    {
        public void Configure(EntityTypeBuilder<FpsAccountCategory> entity)
        {
            entity.HasKey(e => new { e.AccShortName, e.FpsYear }).HasName("pk_tblkpaccountcategory");

            entity.ToTable("tblkpaccountcategory", DbConstants.FpsSchemaName);

            entity.HasIndex(e => e.AccountType, "accounttype");

            entity.Property(e => e.AccShortName)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("accshortname");
            entity.Property(e => e.FpsYear).HasColumnName(DbConstants.FpsYearColumnName);
            entity.Property(e => e.AccountDescription)
                .HasMaxLength(50)
                .HasColumnName("accountdescription");
            entity.Property(e => e.AccountType)
                .HasColumnType(DbConstants.CitextColumnType)
                .HasColumnName("accounttype");
            entity.Property(e => e.ConstituentAccountCodes)
                .HasMaxLength(100)
                .HasColumnName("constituentaccountcodes");
            entity.Property(e => e.Csg7Group)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("csg7_group");
            entity.Property(e => e.ProjectSpecific).HasColumnName("projectspecific");
            entity.Property(e => e.RcSpecific).HasColumnName("rcspecific");
        }
    }
}
