using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class FpsAdditionalCostMap : IEntityTypeConfiguration<FpsAdditionalCost>
    {
        public void Configure(EntityTypeBuilder<FpsAdditionalCost> entity)
        {
            entity.HasKey(e => new { e.JobCode, e.Account, e.Description, e.FpsYear }).HasName("pk_tbladditionalcosts");

            entity.ToTable("tbladditionalcosts", "fps");

            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");
            entity.Property(e => e.Account)
                .HasMaxLength(50)
                .HasColumnName("account");
            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Frequency)
                .HasMaxLength(5)
                .HasColumnName("freq");
            entity.Property(e => e.ItemCost)
                .HasPrecision(19, 4)
                .HasColumnName("itemcost");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");
        }
    }
}
