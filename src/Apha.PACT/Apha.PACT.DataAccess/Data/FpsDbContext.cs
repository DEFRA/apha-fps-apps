using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    { 
        private const string fpsYear = "fpsyear";
        private readonly IFpsYearContext _fPSYearContext;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsYearContext fPSYearContext)
            : base(options)
        {
            _fPSYearContext = fPSYearContext;
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<JobCode> JobCodes { get; set; }
        public virtual DbSet<TimeCodeValid> TimeCodeValids { get; set; }
        public virtual DbSet<WorkGroup> WorkGroups { get; set; }
        public virtual DbSet<ProjectInvoice> ProjectInvoices { get; set; }
        public virtual DbSet<ProjectSubContract> ProjectSubContracts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => new { e.ParentProject, e.FpsYear }).HasName("pk_tlkpproject");

                entity.ToTable("tlkpproject", "fps");

                entity.HasIndex(e => e.ProjectStatus, "projectstatus");

                entity.Property(e => e.ParentProject)
                    .HasColumnType("citext")
                    .HasColumnName("parentproject");
                entity.Property(e => e.FpsYear).HasColumnName(fpsYear);
                entity.Property(e => e.BudgetCvl)
                    .HasDefaultValueSql("0")
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
                    .HasDefaultValueSql("0")
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
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("datecreated");
                entity.Property(e => e.Disease)
                    .HasColumnType("citext")
                    .HasColumnName("disease");
                entity.Property(e => e.FecCost)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("feccost");
                entity.Property(e => e.Finished)
                    .HasDefaultValue((short)0)
                    .HasColumnName("finished");
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
                entity.Property(e => e.PlanCaseWorkDebit)
                    .HasColumnType("money")
                    .HasColumnName("plancaseworkdebit");
                entity.Property(e => e.Profit)
                    .HasDefaultValueSql("0")
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
                entity.Property(e => e.WipCurrent)
                    .HasColumnType("money")
                    .HasColumnName("wip_current");
                entity.Property(e => e.WipEoy)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("wip_eoy");
                entity.Property(e => e.WipLimit)
                    .HasColumnType("money")
                    .HasColumnName("wip_limit");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<WorkGroup>(entity =>
            {
                entity.HasKey(e => new { e.WorkGroupName, e.FpsYear }).HasName("pk_workgroup");

                entity.ToTable("workgroup", "fps");

                entity.HasIndex(e => e.ProfitCentre, "workgroup_profitcentre");

                entity.Property(e => e.WorkGroupName)
                    .HasColumnType("citext")
                    .HasColumnName("workgroup");
                entity.Property(e => e.FpsYear).HasColumnName(fpsYear);
                entity.Property(e => e.CentralOverhead)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("centraloverhead");
                entity.Property(e => e.Cos90).HasColumnName("cos90");
                entity.Property(e => e.CostCentre).HasColumnName("costcentre");
                entity.Property(e => e.CostCentreOld).HasColumnName("costcentreold");
                entity.Property(e => e.Description)
                    .HasMaxLength(45)
                    .HasColumnName("description");
                entity.Property(e => e.EmailRecipient)
                    .HasMaxLength(50)
                    .HasColumnName("email_recipient");
                entity.Property(e => e.Owner)
                    .HasMaxLength(50)
                    .HasColumnName("owner");
                entity.Property(e => e.ProfitCentre)
                    .HasColumnType("citext")
                    .HasColumnName("profitcentre");
                entity.Property(e => e.SendEmail).HasColumnName("sendemail");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<TimeCodeValid>(entity =>
            {
                entity.HasKey(e => new { e.WorkGroup, e.TimeCode, e.ParentProject, e.FpsYear }).HasName("pk_timecodevalid");

                entity.ToTable("timecodevalid", "fps");

                entity.HasIndex(e => e.JobCode, "reference20");

                entity.HasIndex(e => new { e.TestCode, e.Portfolio }, "reference24");

                entity.HasIndex(e => e.ParentProject, "reference3");

                entity.Property(e => e.WorkGroup)
                    .HasColumnType("citext")
                    .HasColumnName("workgroup");
                entity.Property(e => e.TimeCode)
                    .HasColumnType("citext")
                    .HasColumnName("timecode");
                entity.Property(e => e.ParentProject)
                    .HasColumnType("citext")
                    .HasColumnName("parentproject");
                entity.Property(e => e.FpsYear).HasColumnName(fpsYear);
                entity.Property(e => e.Active).HasColumnName("active");
                entity.Property(e => e.JobCode)
                    .HasMaxLength(50)
                    .HasColumnName("jobcode");
                entity.Property(e => e.Portfolio)
                    .HasMaxLength(20)
                    .HasColumnName("portfolio");
                entity.Property(e => e.TestCode)
                    .HasMaxLength(50)
                    .HasColumnName("testcode");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<JobCode>(entity =>
            {
                entity.HasKey(e => new { e.JobCodeId, e.FpsYear }).HasName("pk_tlkpjobcode");

                entity.ToTable("tlkpjobcode", "fps");

                entity.Property(e => e.JobCodeId)
                    .HasMaxLength(50)
                    .HasColumnName("jobcode");
                entity.Property(e => e.FpsYear).HasColumnName(fpsYear);
                entity.Property(e => e.JobCodeName)
                    .HasMaxLength(255)
                    .HasColumnName("jobcodename");
                entity.Property(e => e.JobCodeWorkGroup)
                    .HasMaxLength(50)
                    .HasColumnName("jobcodeworkgroup");
                entity.Property(e => e.NewProg)
                    .HasMaxLength(20)
                    .HasColumnName("newprog");
                entity.Property(e => e.ParentProject)
                    .HasColumnType("citext")
                    .HasColumnName("parentproject");
                entity.Property(e => e.Type)
                    .HasMaxLength(15)
                    .HasColumnName("type");
                
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<ProjectInvoice>(entity =>
            {
                entity.HasKey(e => new { e.InvoiceCounter, e.FpsYear }).HasName("pk_proj_invoice");

                entity.ToTable("proj_invoice", "fps");

                entity.Property(e => e.InvoiceCounter).HasColumnName("invoicecounter");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.Amount)
                    .HasColumnType("money")
                    .HasColumnName("amount");
                entity.Property(e => e.CostOfWork)
                    .HasColumnType("money")
                    .HasColumnName("costofwork");
                entity.Property(e => e.Detail)
                    .HasMaxLength(100)
                    .HasColumnName("detail");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.ProfitLoss)
                    .HasColumnType("money")
                    .HasColumnName("profitloss");
                entity.Property(e => e.ProjectParent)
                    .HasColumnType("citext")
                    .HasColumnName("projectparent");
                entity.Property(e => e.Type)
                    .HasMaxLength(10)
                    .HasColumnName("type");
                entity.Property(e => e.Wip)
                    .HasColumnType("money")
                    .HasColumnName("wip");
                entity.Property(e => e.X)
                    .HasMaxLength(5)
                    .HasColumnName("x");

                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<ProjectSubContract>(entity =>
            {
                entity.HasKey(e => new { e.SubContCounter, e.FpsYear }).HasName("pk_proj_subcontract");

                entity.ToTable("proj_subcontract", "fps");

                entity.Property(e => e.SubContCounter)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("subcontcounter");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.AcctCode)
                    .HasMaxLength(30)
                    .HasColumnName("acctcode");
                entity.Property(e => e.Amount)
                    .HasColumnType("money")
                    .HasColumnName("amount");
                entity.Property(e => e.AnimalDays).HasColumnName("animaldays");
                entity.Property(e => e.DailyRate)
                    .HasColumnType("money")
                    .HasColumnName("dailyrate");
                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .HasColumnName("description");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.Project)
                    .HasColumnType("citext")
                    .HasColumnName("project");
                entity.Property(e => e.Supplier)
                    .HasMaxLength(50)
                    .HasColumnName("supplier");
                entity.Property(e => e.SupplierNumber).HasColumnName("suppliernumber");
                entity.Property(e => e.TestJob)
                    .HasMaxLength(50)
                    .HasColumnName("testjob");
                entity.Property(e => e.WorkGroup)
                    .HasMaxLength(50)
                    .HasColumnName("workgroup");

                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

        }
    }
}