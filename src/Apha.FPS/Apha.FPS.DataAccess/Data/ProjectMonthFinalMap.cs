using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectMonthFinalMap : IEntityTypeConfiguration<ProjectMonthFinal>
    {
        public void Configure(EntityTypeBuilder<ProjectMonthFinal> entity)
        {
            entity.HasKey(e => new { e.Project, e.MonthNo })
                .HasName("aaaaaprojectmonthfinal_pk");

            entity.ToTable("projectmonthfinal", "fps");

            entity.Property(e => e.Project).HasMaxLength(20).HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.PeriodName).HasMaxLength(50).HasColumnName("periodname");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag");
            entity.Property(e => e.CostProfile).HasColumnType("money").HasColumnName("costprofile");
            entity.Property(e => e.Subcontracts).HasColumnType("money").HasColumnName("subcontracts");
            entity.Property(e => e.Animals).HasColumnType("money").HasColumnName("animals");
            entity.Property(e => e.NonAnimals).HasColumnType("money").HasColumnName("nonanimals");
            entity.Property(e => e.TimeCosts).HasColumnType("money").HasColumnName("timecosts");
            entity.Property(e => e.TransferCosts).HasColumnType("money").HasColumnName("transfercosts");
            entity.Property(e => e.TotalCost).HasColumnType("money").HasColumnName("totalcost");
            entity.Property(e => e.Invoices).HasColumnType("money").HasColumnName("invoices");
            entity.Property(e => e.Coiw).HasColumnType("money").HasColumnName("coiw");
            entity.Property(e => e.PortSales).HasColumnType("money").HasColumnName("portsales");
            entity.Property(e => e.CumCost).HasColumnType("money").HasColumnName("cumcost");
            entity.Property(e => e.CumProfile).HasColumnType("money").HasColumnName("cumprofile");
            entity.Property(e => e.SumOfCostProfile).HasColumnType("money").HasColumnName("sumofcostprofile");
            entity.Property(e => e.CumInvoices).HasColumnType("money").HasColumnName("cuminvoices");
            entity.Property(e => e.CumCoiw).HasColumnType("money").HasColumnName("cumcoiw");
            entity.Property(e => e.CumPortSales).HasColumnType("money").HasColumnName("cumportsales");
            entity.Property(e => e.MilestoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.SumOfMilestoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.CwDebit).HasColumnType("money").HasColumnName("cwdebit");
            entity.Property(e => e.CwCredit).HasColumnType("money").HasColumnName("cwcredit");
            entity.Property(e => e.CumCwDebit).HasColumnType("money").HasColumnName("cumcwdebit");
            entity.Property(e => e.CumCwCredit).HasColumnType("money").HasColumnName("cumcwcredit");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CumSubcontracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.X).HasColumnName("x");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}
