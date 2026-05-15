using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.Data
{
    public class CustomerMap : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(e => e.CustomerName).HasName("tlkpcustomer_pk___1__15");

            entity.ToTable("tlkpcustomer", DbConstants.FpsSchemaName);

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer");
        }
    }
}

