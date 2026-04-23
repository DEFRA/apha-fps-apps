using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectViewMap : IEntityTypeConfiguration<ProjectView>
    {


        public void Configure(EntityTypeBuilder<ProjectView> entity)
        {
            entity
                .HasNoKey()
                .ToView("vtlkpproject", "fps");

            entity.Property(e => e.BudgetCvl)
                .HasColumnType("money")
                .HasColumnName("budget_cvl");
            entity.Property(e => e.CarryOver)
                .HasColumnType("money")
                .HasColumnName("carryover");
            entity.Property(e => e.CarryOverSeed)
                .HasColumnType("money")
                .HasColumnName("carryoverseed");
            entity.Property(e => e.CaseWorkSub)
                .HasPrecision(5, 4)
                .HasColumnName("caseworksub");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Contract)
                .HasColumnType("citext")
                .HasColumnName("contract");
            entity.Property(e => e.CostBookNo)
                .HasMaxLength(50)
                .HasColumnName("costbookno");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.CustIncome)
                .HasColumnType("money")
                .HasColumnName("custincome");
            entity.Property(e => e.Customer)
                .HasColumnType("citext")
                .HasColumnName("customer");
            entity.Property(e => e.DateCosted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecosted");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Disease)
                .HasColumnType("citext")
                .HasColumnName("disease");
            entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
            entity.Property(e => e.FecCost)
                .HasColumnType("money")
                .HasColumnName("feccost");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.IncomeAccountCode)
                .HasColumnType("citext")
                .HasColumnName("incomeaccountcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");
            entity.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");
            entity.Property(e => e.OwningRc)
                .HasMaxLength(50)
                .HasColumnName("owningrc");
            entity.Property(e => e.ParentProject)
                .HasColumnType("citext")
                .HasColumnName("parentproject");
            entity.Property(e => e.PlanCaseWorkDebit)
                .HasColumnType("money")
                .HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Profit)
                .HasColumnType("money")
                .HasColumnName("profit");
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
            entity.Property(e => e.ShortTitle)
                .HasMaxLength(30)
                .HasColumnName("shorttitle");
            entity.Property(e => e.SubAccountCode)
                .HasColumnType("citext")
                .HasColumnName("subaccountcode");
            entity.Property(e => e.TransferIncome)
                .HasColumnType("money")
                .HasColumnName("transferincome");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("useremail");
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
