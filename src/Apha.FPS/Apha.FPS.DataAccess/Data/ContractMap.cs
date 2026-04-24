using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ContractMap : IEntityTypeConfiguration<Contract>
    {


        public void Configure(EntityTypeBuilder<Contract> entity)
        {
            entity.HasKey(e => e.ContractNo).HasName("tblcontract_pk___2__10");

            entity.ToTable("tblcontract", "fps");

            entity.Property(e => e.ContractNo)
                .HasMaxLength(10)
                .HasColumnName("contractno");
            entity.Property(e => e.Category)
                .HasMaxLength(20)
                .HasColumnName("category");
            entity.Property(e => e.ContractDoc).HasColumnName("contractdoc");
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.EndDate).HasColumnName("enddate");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.RegisteredDate).HasColumnName("registereddate");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
        }
    }
}
