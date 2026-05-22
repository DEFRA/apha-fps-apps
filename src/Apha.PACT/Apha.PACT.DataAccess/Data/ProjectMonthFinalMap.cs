using Apha.PACT.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class ProjectMonthFinalMap : IEntityTypeConfiguration<ProjectMonthFinal>
    {
        private const string MoneyColumnType = "money";

        public void Configure(EntityTypeBuilder<ProjectMonthFinal> entity)
        {
            entity.HasKey(e => new { e.Project, e.MonthNo }).HasName("projectmonthfinal_aaaaaprojectmonthfinal_pk");

            entity.ToTable("projectmonthfinal", "fps");

            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.MonthNo).HasColumnName("monthno");
            entity.Property(e => e.Animals)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("animals");
            entity.Property(e => e.Coiw)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("coiw");
            entity.Property(e => e.CostProfile)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("costprofile");
            entity.Property(e => e.CumCoiw)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumcoiw");
            entity.Property(e => e.CumCost)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumcost");
            entity.Property(e => e.CumCwCredit)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumcwcredit");
            entity.Property(e => e.CumCwDebit)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumcwdebit");
            entity.Property(e => e.CumFlag).HasColumnName("cumflag");
            entity.Property(e => e.CumInvoices)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cuminvoices");
            entity.Property(e => e.CumPayCosts).HasColumnName("cumpaycosts");
            entity.Property(e => e.CumPortSales)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumportsales");
            entity.Property(e => e.CumProfile)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cumprofile");
            entity.Property(e => e.CumSubcontracts).HasColumnName("cumsubcontracts");
            entity.Property(e => e.CumTestCosts).HasColumnName("cumtestcosts");
            entity.Property(e => e.CumTotalHours).HasColumnName("cumtotalhours");
            entity.Property(e => e.CwCredit)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cwcredit");
            entity.Property(e => e.CwDebit)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("cwdebit");
            entity.Property(e => e.DueDone).HasColumnName("due__done");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Invoices)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("invoices");
            entity.Property(e => e.MstoneDue).HasColumnName("mstonedue");
            entity.Property(e => e.NonAnimals)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("nonanimals");
            entity.Property(e => e.OnTime).HasColumnName("ontime");
            entity.Property(e => e.PayCosts).HasColumnName("paycosts");
            entity.Property(e => e.PeriodName)
                .HasMaxLength(50)
                .HasColumnName("periodname");
            entity.Property(e => e.PortSales)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("portsales");
            entity.Property(e => e.Subcontracts)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("subcontracts");
            entity.Property(e => e.SumOfCostProfile)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("sumofcostprofile");
            entity.Property(e => e.SumOfDueDone).HasColumnName("sumofdue__done");
            entity.Property(e => e.SumOfMstoneDue).HasColumnName("sumofmstonedue");
            entity.Property(e => e.SumOfOnTime).HasColumnName("sumofontime");
            entity.Property(e => e.TimeCosts)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("timecosts");
            entity.Property(e => e.TotalCost)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("totalcost");
            entity.Property(e => e.TotalHours).HasColumnName("totalhours");
            entity.Property(e => e.TransferCosts)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("transfercosts");
            entity.Property(e => e.X).HasColumnName("x");
        }
    }
}