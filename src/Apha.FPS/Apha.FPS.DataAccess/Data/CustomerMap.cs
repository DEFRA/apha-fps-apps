using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class CustomerMap : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> entity)
        {
            entity.HasKey(e => e.CustomerName).HasName("pk___1__15");

            entity.ToTable("tlkpcustomer", "fps");

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer");
        }
    }
}
