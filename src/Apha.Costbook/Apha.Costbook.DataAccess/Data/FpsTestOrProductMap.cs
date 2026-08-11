using Apha.Costbook.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class FpsTestOrProductMap : IEntityTypeConfiguration<FpsTestOrProduct>
    {
        public void Configure(EntityTypeBuilder<FpsTestOrProduct> entity)
        {
            entity.HasKey(e => new { e.ItemCode, e.FpsYear }).HasName("pk_testorproduct");

            entity.ToTable("testorproduct", "fps");

            entity.Property(e => e.ItemCode)
                .HasMaxLength(20)
                .HasColumnName("itemcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.ChargeMethod)
                .HasMaxLength(5)
                .HasColumnName("chargemethod");
            entity.Property(e => e.DefraUnitPrice)
                .HasPrecision(19, 4)
                .HasColumnName("defraunitprice");
            entity.Property(e => e.ItemDescription)
                .HasMaxLength(200)
                .HasColumnName("itemdescription");
            entity.Property(e => e.JobStatus)
                .HasMaxLength(2)
                .HasColumnName("jobstatus");
            entity.Property(e => e.Owner)
                .HasMaxLength(2)
                .HasColumnName("owner");
            entity.Property(e => e.PriceAhvg)
                .HasPrecision(19, 4)
                .HasColumnName("priceahvg");
            entity.Property(e => e.ShortDescription)
                .HasMaxLength(18)
                .IsFixedLength()
                .HasColumnName("shortdescription");
            entity.Property(e => e.TestManager)
                .HasMaxLength(50)
                .HasColumnName("testmanager");
            entity.Property(e => e.UnitPriceVla)
                .HasPrecision(19, 4)
                .HasDefaultValue(0m)
                .HasColumnName("unitpricevla");
        }
    }
}
