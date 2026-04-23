using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class StaffRequirementMap : IEntityTypeConfiguration<StaffRequirement>
    {
        public void Configure(EntityTypeBuilder<StaffRequirement> entity)
        {
           
                entity.HasKey(e => e.SrIdentity).HasName("aaaaatblstaffrequ_pk");

                entity.ToTable("tblstaffrequ", "mabarchive");

                entity.HasIndex(e => new { e.Project, e.Year }, "tblprojectyeartblstaffrequ");

                entity.Property(e => e.SrIdentity).HasColumnName("sr_identity");
                entity.Property(e => e.Chargerate)
                    .HasDefaultValueSql("0")
                    .HasColumnName("chargerate");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
                entity.Property(e => e.Nodays)
                    .HasDefaultValueSql("0")
                    .HasColumnName("nodays");
                entity.Property(e => e.Nohours)
                    .HasDefaultValueSql("0")
                    .HasColumnName("nohours");
                entity.Property(e => e.Npr).HasColumnName("npr");
                entity.Property(e => e.Ohr).HasColumnName("ohr");
                entity.Property(e => e.Payrate).HasColumnName("payrate");
                entity.Property(e => e.Project)
                    .HasMaxLength(50)
                    .HasColumnName("project");
                entity.Property(e => e.WgGrade)
                    .HasMaxLength(20)
                    .HasColumnName("wggrade");
                entity.Property(e => e.Year)
                    .HasDefaultValue(0)
                    .HasColumnName("year");
          
        }
    }
}
