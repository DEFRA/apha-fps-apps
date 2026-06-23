using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectProfitabilityVlaViewMap : IEntityTypeConfiguration<ProjectProfitabilityVlaView>
    {
        public void Configure(EntityTypeBuilder<ProjectProfitabilityVlaView> entity)
        {
            entity.HasNoKey().ToView("vprojectprofitabilityvla", "fps");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.JobCode).HasMaxLength(50).HasColumnName("jobcode");
            entity.Property(e => e.Program).HasMaxLength(50).HasColumnName("program");
            entity.Property(e => e.Customer).HasMaxLength(255).HasColumnName("customer");
            entity.Property(e => e.Manager).HasMaxLength(255).HasColumnName("manager");
            entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
            entity.Property(e => e.StaffCosts).HasColumnType("money").HasColumnName("staffcosts");
            entity.Property(e => e.TestCost).HasColumnType("money").HasColumnName("testcost");
            entity.Property(e => e.AnimalCosts).HasColumnType("money").HasColumnName("animalcosts");
            entity.Property(e => e.AdditionalCosts).HasColumnType("money").HasColumnName("additionalcosts");
            entity.Property(e => e.TotalCosts).HasColumnType("money").HasColumnName("totalcosts");
            entity.Property(e => e.Budget).HasColumnType("money").HasColumnName("budget");
            entity.Property(e => e.Profit).HasColumnType("money").HasColumnName("profit");
            entity.Property(e => e.TargetProfit).HasColumnType("money").HasColumnName("targetprofit");
            entity.Property(e => e.OffTarget).HasColumnType("money").HasColumnName("offtarget");
        }
    }
}
