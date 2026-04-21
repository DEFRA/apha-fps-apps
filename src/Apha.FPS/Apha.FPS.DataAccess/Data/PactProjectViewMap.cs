using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class PactProjectViewMap : IEntityTypeConfiguration<PactProjectView>
    {
        public void Configure(EntityTypeBuilder<PactProjectView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vpactproject", "fps");

            entity.Property(e => e.BudgetCvl)
                .HasColumnType("money")
                .HasColumnName("budget_cvl");
            entity.Property(e => e.BudgetExt)
                .HasColumnType("money")
                .HasColumnName("budget_ext");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Contract)
                .HasColumnType("citext")
                .HasColumnName("contract");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.Customer)
                .HasColumnType("citext")
                .HasColumnName("customer");
            entity.Property(e => e.Disease)
                .HasColumnType("citext")
                .HasColumnName("disease");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.ForecastCost)
                .HasColumnType("money")
                .HasColumnName("forecastcost");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");
            entity.Property(e => e.ParentProject)
                .HasColumnType("citext")
                .HasColumnName("parentproject");
            entity.Property(e => e.Program)
                .HasColumnType("citext")
                .HasColumnName("program");
            entity.Property(e => e.ProjectGroup)
                .HasColumnType("citext")
                .HasColumnName("projectgroup");
            entity.Property(e => e.ProjectParent)
                .HasMaxLength(50)
                .HasColumnName("projectparent");
            entity.Property(e => e.ProjectStatus)
                .HasColumnType("citext")
                .HasColumnName("projectstatus");
            entity.Property(e => e.ProjectTitle)
                .HasMaxLength(200)
                .HasColumnName("projecttitle");
            entity.Property(e => e.PvsIncome)
                .HasColumnType("money")
                .HasColumnName("pvsincome");
            entity.Property(e => e.SubAccountCode)
                .HasColumnType("citext")
                .HasColumnName("subaccountcode");
            entity.Property(e => e.TransferIncome)
                .HasColumnType("money")
                .HasColumnName("transferincome");
            entity.Property(e => e.WipCurrent)
                .HasColumnType("money")
                .HasColumnName("wip_current");
            entity.Property(e => e.WipEoy)
                .HasColumnType("money")
                .HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit)
                .HasColumnType("money")
                .HasColumnName("wip_limit");
        }
    }
}
