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
        public virtual DbSet<TestCapability> TestCapabilities { get; set; }
        public virtual DbSet<TestRequirement> TestRequirements { get; set; }
        public virtual DbSet<TestorProduct> TestorProducts { get; set; }
        public virtual DbSet<TestReqLog> TestReqLogs { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }
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

            modelBuilder.Entity<TestCapability>(entity =>
            {
                entity.HasKey(e => new { e.TestCode, e.WorkGroup, e.FpsYear }).HasName("pk_tlkptestcapability");

                entity.ToTable("tlkptestcapability", "fps");

                entity.HasIndex(e => e.PlanPortfolio, "tlkptestcapability_planportfol");
                entity.Property(e => e.TestCode)
                    .HasColumnType("citext")
                    .HasColumnName("testcode");
                entity.Property(e => e.WorkGroup)
                    .HasColumnType("citext")
                    .HasColumnName("workgroup");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.PlanPortfolio)
                    .HasColumnType("citext")
                    .HasColumnName("planportfolio");
                entity.Property(e => e.PredOutturn)
                    .HasDefaultValueSql("0")
                    .HasColumnName("predoutturn");
                entity.Property(e => e.SmsCode)
                    .HasMaxLength(50)
                    .HasColumnName("smscode");
                entity.Property(e => e.Sop)
                    .HasMaxLength(50)
                    .HasColumnName("sop");
                entity.Property(e => e.UnitCost)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("unitcost");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<TestRequirement>(entity =>
            {
                entity.HasKey(e => new { e.TestCode, e.Buyer, e.FpsYear }).HasName("pk_tlkptestreqmt");

                entity.ToTable("tlkptestreqmt", "fps");

                entity.HasIndex(e => e.TestBuyerCode, "reference10");
                entity.HasIndex(e => e.ProjectBuyerCode, "reference19");

                entity.HasIndex(e => e.TestCode, "reference31");

                entity.Property(e => e.TestCode)
                    .HasColumnType("citext")
                    .HasColumnName("testcode");
                entity.Property(e => e.Buyer)
                    .HasColumnType("citext")
                    .HasColumnName("buyer");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.Active)
                    .HasDefaultValue((short)1)
                    .HasColumnName("active");
                entity.Property(e => e.DateCreated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("datecreated");
                entity.Property(e => e.NoRequired).HasColumnName("norequired");
                entity.Property(e => e.ProjectBuyerCode)
                    .HasMaxLength(50)
                    .HasColumnName("projectbuyercode");
                entity.Property(e => e.TestBuyerCode)
                    .HasMaxLength(50)
                    .HasColumnName("testbuyercode");
                entity.Property(e => e.UnitPrice)
                    .HasColumnType("money")
                    .HasColumnName("unitprice");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<TestorProduct>(entity =>
            {
                entity.HasKey(e => new { e.ItemCode, e.FpsYear }).HasName("pk_testorproduct");

                entity.ToTable("testorproduct", "fps");

                entity.Property(e => e.ItemCode)
                    .HasColumnType("citext")
                    .HasColumnName("itemcode");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.ChargeMethod)
                    .HasMaxLength(5)
                    .HasColumnName("chargemethod");
                entity.Property(e => e.DefraUnitPrice)
                    .HasColumnType("money")
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
                    .HasColumnType("money")
                    .HasColumnName("priceahvg");
                entity.Property(e => e.ShortDescription)
                    .HasMaxLength(18)
                    .IsFixedLength()
                    .HasColumnName("shortdescription");
                entity.Property(e => e.TestManager)
                    .HasMaxLength(50)
                    .HasColumnName("testmanager");
                entity.Property(e => e.UnitPriceVla)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("unitpricevla");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<TestReqLog>(entity =>
            {
                entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_testreq_log");

                entity.ToTable("testreq_log", "fps");

                entity.HasIndex(e => e.SequenceNo, "idx_testreqlog_sequenceno")
                    .HasAnnotation("Npgsql:StorageParameter:deduplicate_items", "true")
                    .HasAnnotation("Npgsql:StorageParameter:fillfactor", "100");

                entity.HasIndex(e => e.DateTime, "testreq_log_ind_dt");

                entity.HasIndex(e => e.JobCode, "testreq_log_ind_jc");

                entity.Property(e => e.SequenceNo)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("sequenceno");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.Active).HasColumnName("active");
                entity.Property(e => e.Buyer)
                    .HasMaxLength(20)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("buyer");
                entity.Property(e => e.DateTime)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("date_time");
                entity.Property(e => e.InsertDelete)
                    .HasMaxLength(2)
                    .IsFixedLength()
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("insert_delete");
                entity.Property(e => e.JobCode)
                    .HasMaxLength(50)
                    .HasComment("Generated column based on projectbuyercode")
                    .HasColumnName("jobcode");
                entity.Property(e => e.NoRequired).HasColumnName("norequired");
                entity.Property(e => e.ProjectBuyerCode)
                    .HasMaxLength(50)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("projectbuyercode");
                entity.Property(e => e.TestBuyerCode)
                    .HasMaxLength(50)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("testbuyercode");
                entity.Property(e => e.TestCode)
                    .HasMaxLength(20)
                    .HasColumnName("testcode");
                entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
                entity.Property(e => e.UserId)
                    .HasMaxLength(20)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("user_id");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<MonthlyOutput>(entity =>
            {
                entity.HasKey(e => new { e.TestCode, e.Buyer, e.Month, e.WorkGroup, e.FpsYear }).HasName("pk_monthlyoutput");

                entity.ToTable("monthlyoutput", "fps");

                entity.HasIndex(e => e.Month, "month");

                entity.HasIndex(e => e.WorkGroup, "monthlyoutput_workgroup");

                entity.HasIndex(e => new { e.TestCode, e.Buyer }, "reference14");

                entity.HasIndex(e => new { e.WorkGroup, e.TestCode }, "reference25");
                entity.HasIndex(e => e.TestCode, "testcode");

                entity.Property(e => e.TestCode)
                    .HasColumnType("citext")
                    .HasColumnName("testcode");
                entity.Property(e => e.Buyer)
                    .HasColumnType("citext")
                    .HasColumnName("buyer");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.WorkGroup)
                    .HasColumnType("citext")
                    .HasColumnName("workgroup");
                entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
                entity.Property(e => e.Volume).HasColumnName("volume");
                entity.Property(e => e.WgBuyer)
                    .HasMaxLength(50)
                    .HasColumnName("wgbuyer");
                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });

        }
    }
}