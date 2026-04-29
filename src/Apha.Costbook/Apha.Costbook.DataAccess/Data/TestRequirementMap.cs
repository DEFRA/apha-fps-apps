using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class TestRequirementMap : IEntityTypeConfiguration<TestRequirement>
    {
        public void Configure(EntityTypeBuilder<TestRequirement> entity)
        {
            entity.HasKey(e => new { e.Project, e.Year, e.TestCode }).HasName("pk_tbltestrequ");

            entity.ToTable("tbltestrequ", "mabarchive");

            entity.HasIndex(e => new { e.Project, e.Year }, "tblprojectyeartbltestrequ");

            entity.HasIndex(e => e.Project, "tbltestrequ_tbltestrequproject");

            entity.Property(e => e.Project)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.Year)
                .HasDefaultValue(0)
                .HasColumnName("year");
            entity.Property(e => e.TestCode)
                .HasMaxLength(50)
                .HasColumnName("testcode");
            entity.Property(e => e.NumberOfTests)
                .HasDefaultValueSql("0")
                .HasColumnName("notests");
            entity.Property(e => e.UnitPrice)
                .HasDefaultValueSql("0")
                .HasColumnName("unitprice");
        }
    }
}