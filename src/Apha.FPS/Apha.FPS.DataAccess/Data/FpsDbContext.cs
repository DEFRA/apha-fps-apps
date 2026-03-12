using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsYearContext _fPSYearContext;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsYearContext fPSYearContext)
            : base(options)
        {
            _fPSYearContext = fPSYearContext;
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserProgram> UserPrograms { get; set; }
        public virtual DbSet<Program> Programs { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<StaffJob> StaffJobs { get; set; }
        public virtual DbSet<WgEmployee> WgEmployees { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<FpsSetting> TblSettings { get; set; }
        public virtual DbSet<Workgroup> Workgroups { get; set; }
        public virtual DbSet<WorkgroupGrade> WorkgroupGrades { get; set; }
        public virtual DbSet<ProfitCentreGrade> ProfitcentreGrades { get; set; }
        public virtual DbSet<UserProfitcentre> UserProfitcentres { get; set; }
        public virtual DbSet<ProfitCentre> ProfitCentres { get; set; }
        public virtual DbSet<JobCode> JobCodes { get; set; }
        public virtual DbSet<Status> Statuses { get; set; }
        public virtual DbSet<Disease> Diseases { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<Animal> Animals { get; set; }
        public virtual DbSet<AnimalRequest> AnimalRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("tblusers_pk__tblusers__1367e606");

                entity.ToTable("tblusers", "fps");

                entity.HasIndex(e => e.Username, "dbo_tblusers_username")
                .IsUnique()
                .UseCollation(new[] { "latin1_general_ci_as" });

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AgencyId).HasColumnName("agencyid");
                entity.Property(e => e.Comments)
                .HasMaxLength(255)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("comments");
                entity.Property(e => e.Dt2Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("dt2username");
                entity.Property(e => e.FrmWarning).HasColumnName("frmwarning");
                entity.Property(e => e.Username)
                .HasMaxLength(50)
                .UseCollation("latin1_general_ci_as")
                .HasColumnName("username");
            });

            modelBuilder.Entity<UserProgram>(entity =>
            {
                entity.HasKey(e => new { e.ProgramNo, e.UserID }).HasName("tbluser_program_pk__tbluser_program__26afc4a4");

                entity.ToTable("tbluser_program", "fps");

                entity.HasIndex(e => e.ProgramNo, "dbo_tbluser_program_xif84tbluser_program");

                entity.Property(e => e.ProgramNo)
                    .HasMaxLength(10)
                    .HasColumnName("programno");
                entity.Property(e => e.UserID).HasColumnName("user_id");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Program>(entity =>
            {
                entity.HasKey(e => e.ProgramNo).HasName("tlkpprogram_pk__tlkpprogram__2180fb33");

                entity.ToTable("tlkpprogram", "fps");

                entity.HasIndex(e => e.Minim, "dbo_tlkpprogram_tlkpprogram_minim");

                entity.Property(e => e.ProgramNo)
                    .HasMaxLength(10)
                    .HasColumnName("programno");
                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
                entity.Property(e => e.Directorate)
                    .HasMaxLength(15)
                    .HasColumnName("directorate");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .HasColumnName("manager");
                entity.Property(e => e.Minim)
                    .HasMaxLength(7)
                    .HasColumnName("minim");
                entity.Property(e => e.ProgramName)
                    .HasMaxLength(80)
                    .HasColumnName("programname");
                entity.Property(e => e.SectorName)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("'Charge'::character varying")
                    .HasColumnName("sector_name");
                entity.Property(e => e.Target)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("target");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.ParentProject).HasName("tlkpproject_pk__tlkpproject__6c83230f");

                entity.ToTable("tlkpproject", "fps");

                entity.HasIndex(e => e.ProjectStatus, "dbo_tlkpproject_projectstatus");

                entity.Property(e => e.ParentProject)
                    .HasMaxLength(20)
                    .HasColumnName("parentproject");
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
                    .HasMaxLength(10)
                    .HasDefaultValueSql("0")
                    .HasColumnName("contract");
                entity.Property(e => e.CostBookNo)
                    .HasMaxLength(50)
                    .HasColumnName("costbookno");
                entity.Property(e => e.CostCentre).HasColumnName("costcentre");
                entity.Property(e => e.CustIncome)
                    .HasColumnType("money")
                    .HasColumnName("custincome");
                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
                entity.Property(e => e.DateCosted)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("datecosted");
                entity.Property(e => e.DateCreated)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("datecreated");
                entity.Property(e => e.Disease)
                    .HasMaxLength(50)
                    .HasColumnName("disease");
                entity.Property(e => e.FecCost)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("feccost");
                entity.Property(e => e.Finished)
                    .HasDefaultValue((short)0)
                    .HasColumnName("finished");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.IncomeAccountCode)
                    .HasMaxLength(50)
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
                    .HasMaxLength(10)
                    .HasColumnName("program");
                entity.Property(e => e.ProjectGroup)
                    .HasMaxLength(50)
                    .HasColumnName("projectgroup");
                entity.Property(e => e.ProjectParent)
                    .HasMaxLength(50)
                    .HasColumnName("projectparent");
                entity.Property(e => e.ProjectStatus)
                    .HasMaxLength(50)
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
                    .HasMaxLength(50)
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
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<StaffJob>(entity =>
            {
                entity.HasKey(e => new { e.StaffId, e.JobCode }).HasName("tblstaffjob_pk__tblstaffjob__30392ede");

                entity.ToTable("tblstaffjob", "fps");

                entity.Property(e => e.StaffId)
                    .HasMaxLength(50)
                    .HasColumnName("staffid");
                entity.Property(e => e.JobCode)
                    .HasMaxLength(20)
                    .HasColumnName("jobcode");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.PlannedHours).HasColumnName("plannedhours");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<WgEmployee>(entity =>
            {
                entity.HasKey(e => e.PactId).HasName("tblwgemployee_pk_tblwgemployee_1__10");

                entity.ToTable("tblwgemployee", "fps");

                entity.Property(e => e.PactId)
                    .HasMaxLength(50)
                    .HasColumnName("pactid");
                entity.Property(e => e.EndDate).HasColumnName("enddate");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.HoursPerWeek).HasColumnName("hoursperweek");
                entity.Property(e => e.HrsAvail).HasColumnName("hrsavail");
                entity.Property(e => e.HrsPaid).HasColumnName("hrspaid");
                entity.Property(e => e.Leave).HasColumnName("leave");
                entity.Property(e => e.MakeAvailable)
                    .HasDefaultValueSql("'-1'::integer")
                    .HasColumnName("makeavailable");
                entity.Property(e => e.PersonClass)
                    .HasMaxLength(10)
                    .HasColumnName("personclass");
                entity.Property(e => e.PersonStatus)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("'A'::character varying")
                    .HasColumnName("personstatus");
                entity.Property(e => e.SickSpecial).HasColumnName("sickspecial");
                entity.Property(e => e.SpNumber)
                    .HasMaxLength(10)
                    .HasColumnName("spnumber");
                entity.Property(e => e.StartDate).HasColumnName("startdate");
                entity.Property(e => e.TimeRecorder)
                    .HasDefaultValue(0)
                    .HasColumnName("timerecorder");
                entity.Property(e => e.WorkGroupGrade)
                    .HasMaxLength(50)
                    .HasColumnName("workgroupgrade");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.SPNumber).HasName("tblemployee_pk___5__10");

                entity.ToTable("tblemployee", "fps");

                entity.Property(e => e.SPNumber)
                    .HasMaxLength(10)
                    .HasColumnName("spnumber");
                entity.Property(e => e.FirstName)
                    .HasMaxLength(20)
                    .HasColumnName("firstname");
                entity.Property(e => e.FPSCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.LastName)
                    .HasMaxLength(20)
                    .HasColumnName("lastname");
                entity.Property(e => e.Title)
                    .HasMaxLength(4)
                    .HasColumnName("title");
                entity.HasQueryFilter(e => e.FPSCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<FpsSetting>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("tblsettings_pk_tblsettings");

                entity.ToTable("tblsettings", "fps");

                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .HasColumnName("id");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.Notes)
                    .HasMaxLength(255)
                    .HasColumnName("notes");
                entity.Property(e => e.Setting)
                    .HasMaxLength(255)
                    .HasColumnName("setting");
                entity.Property(e => e.TestSetting)
                    .HasMaxLength(255)
                    .HasColumnName("testsetting");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Workgroup>(entity =>
            {
                entity.HasKey(e => e.WorkgroupName).HasName("workgroup_pk__workgroup__25518c17");

                entity.ToTable("workgroup", "fps");

                entity.HasIndex(e => e.ProfitCentre, "dbo_workgroup_profitcentre");

                entity.Property(e => e.WorkgroupName)
                    .HasMaxLength(50)
                    .HasColumnName("workgroup");
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
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.Owner)
                    .HasMaxLength(50)
                    .HasColumnName("owner");
                entity.Property(e => e.ProfitCentre)
                    .HasMaxLength(50)
                    .HasColumnName("profitcentre");
                entity.Property(e => e.SendEmail).HasColumnName("sendemail");
                entity.Property(e => e.SysTimestamp)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("systimestamp");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<WorkgroupGrade>(entity =>
            {
                entity.HasKey(e => e.WgGrade).HasName("workgroupgrade_pk__workgroupgrade__2de6d218");

                entity.ToTable("workgroupgrade", "fps");

                entity.Property(e => e.WgGrade)
                    .HasMaxLength(50)
                    .HasColumnName("wggrade");
                entity.Property(e => e.AvSalary)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("avsalary");
                entity.Property(e => e.ChargeRateWg)
                    .HasColumnType("money")
                    .HasColumnName("chargeratewg");
                entity.Property(e => e.DirectRateWg)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("directratewg");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.GradeCode)
                    .HasMaxLength(50)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("gradecode");
                entity.Property(e => e.HrsChangedBy)
                    .HasMaxLength(50)
                    .HasColumnName("hrschangedby");
                entity.Property(e => e.NprWg)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("nprwg");
                entity.Property(e => e.OhrWg)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("ohrwg");
                entity.Property(e => e.PayRateWg)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("payratewg");
                entity.Property(e => e.ProfitCentreGrade)
                    .HasMaxLength(20)
                    .HasColumnName("profitcentregrade");
                entity.Property(e => e.Workgroup)
                    .HasMaxLength(50)
                    .HasColumnName("workgroup");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<ProfitCentreGrade>(entity =>
            {
                entity.HasKey(e => e.PcGrade).HasName("profitcentregrade_pk__profitcentregrad__2bde8e15");

                entity.ToTable("profitcentregrade", "fps");

                entity.Property(e => e.PcGrade)
                    .HasMaxLength(20)
                    .HasColumnName("pcgrade");
                entity.Property(e => e.ChargeRate).HasColumnName("chargerate");
                entity.Property(e => e.DefraChargeRate).HasColumnName("defrachargerate");
                entity.Property(e => e.DirectRate)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("directrate");
                entity.Property(e => e.DivisionGrade)
                    .HasMaxLength(50)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("divisiongrade");
                entity.Property(e => e.FPSCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.GradeCode)
                    .HasMaxLength(50)
                    .UseCollation("latin1_general_ci_as")
                    .HasColumnName("gradecode");
                entity.Property(e => e.HrsAvailable)
                    .HasDefaultValueSql("0")
                    .HasColumnName("hrsavailable");
                entity.Property(e => e.NPR)
                    .HasDefaultValueSql("0")
                    .HasColumnType("money")
                    .HasColumnName("npr");
                entity.Property(e => e.OHR)
                    .HasColumnType("money")
                    .HasColumnName("ohr");
                entity.Property(e => e.OldChargeRate)
                    .HasColumnType("money")
                    .HasColumnName("oldchargerate");
                entity.Property(e => e.PayRate)
                    .HasColumnType("money")
                    .HasColumnName("payrate");
                entity.Property(e => e.ProfitCentre)
                    .HasMaxLength(50)
                    .HasColumnName("profitcentre");
                entity.HasQueryFilter(e => e.FPSCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<UserProfitcentre>(entity =>
            {
                entity.HasKey(e => new { e.ProfitCentre, e.UserId }).HasName("tbluser_profitcentre_pk__tbluser_profitce__77bfcb91");

                entity.ToTable("tbluser_profitcentre", "fps");

                entity.HasIndex(e => e.UserId, "dbo_tbluser_profitcentre_xif89tbluser_profitcentre");

                entity.HasIndex(e => e.ProfitCentre, "dbo_tbluser_profitcentre_xif90tbluser_profitcentre");

                entity.Property(e => e.ProfitCentre)
                    .HasMaxLength(50)
                    .HasColumnName("profitcentre");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<ProfitCentre>(entity =>
            {
                entity.HasKey(e => e.Profitcentre).HasName("tblkpprofitcentre_pk__tblkpprofitcentr__1db06a4f");

                entity.ToTable("tblkpprofitcentre", "fps");

                entity.HasIndex(e => e.Division, "dbo_tblkpprofitcentre_division");

                entity.Property(e => e.Profitcentre)
                    .HasMaxLength(50)
                    .HasColumnName("profitcentre");
                entity.Property(e => e.ContTarget)
                    .HasColumnType("money")
                    .HasColumnName("conttarget");
                entity.Property(e => e.Division)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("0")
                    .HasColumnName("division");
                entity.Property(e => e.DivisionId).HasColumnName("divisionid");
                entity.Property(e => e.EmailRecipient)
                    .HasMaxLength(50)
                    .HasColumnName("email_recipient");
                entity.Property(e => e.HighLevelSummary).HasColumnName("highlevelsummary");
                entity.Property(e => e.OutputSheet).HasColumnName("outputsheet");
                entity.Property(e => e.PactCoordinatorEmailName)
                    .HasMaxLength(50)
                    .HasColumnName("pactcoordinatoremailname");
                entity.Property(e => e.ProfitCentreHead)
                    .HasMaxLength(50)
                    .HasColumnName("profitcentrehead");
                entity.Property(e => e.ProfitCentreName)
                    .HasMaxLength(40)
                    .HasColumnName("profitcentrename");
                entity.Property(e => e.Timesheet).HasColumnName("timesheet");
                entity.Property(e => e.TimesheetLayout).HasColumnName("timesheetlayout");

            });

            modelBuilder.Entity<JobCode>(entity =>
            {
                entity.HasKey(e => e.JobCodeId).HasName("tlkpjobcode_pk_tlkpjobcode_new_1__15");

                entity.ToTable("tlkpjobcode", "fps");

                entity.Property(e => e.JobCodeId)
                    .HasMaxLength(50)
                    .HasColumnName("jobcode");
                entity.Property(e => e.Fpscalyear).HasColumnName("fpsyear");
                entity.Property(e => e.Jobcodename)
                    .HasMaxLength(255)
                    .HasColumnName("jobcodename");
                entity.Property(e => e.Jobcodeworkgroup)
                    .HasMaxLength(50)
                    .HasColumnName("jobcodeworkgroup");
                entity.Property(e => e.Newprog)
                    .HasMaxLength(20)
                    .HasColumnName("newprog");
                entity.Property(e => e.Parentproject)
                    .HasMaxLength(20)
                    .HasColumnName("parentproject");
                entity.Property(e => e.Type)
                    .HasMaxLength(15)
                    .HasColumnName("type");
                entity.HasQueryFilter(e => e.Fpscalyear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Status>(entity =>
            {
                entity.HasKey(e => e.StatusValue).HasName("tblstatus_pk___3__10");

                entity.ToTable("tblstatus", "fps");

                entity.Property(e => e.StatusValue)
                    .HasMaxLength(50)
                    .HasColumnName("status");
            });

            modelBuilder.Entity<Disease>(entity =>
            {
                entity.HasKey(e => e.DiseaseName).HasName("tbldisease_pk___4__10");

                entity.ToTable("tbldisease", "fps");

                entity.Property(e => e.DiseaseName)
                    .HasMaxLength(50)
                    .HasColumnName("disease");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerName).HasName("tlkpcustomer_pk___1__15");

                entity.ToTable("tlkpcustomer", "fps");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(e => e.Contractno).HasName("tblcontract_pk___2__10");

                entity.ToTable("tblcontract", "fps");

                entity.Property(e => e.Contractno)
                    .HasMaxLength(10)
                    .HasColumnName("contractno");
                entity.Property(e => e.Category)
                    .HasMaxLength(20)
                    .HasColumnName("category");
                entity.Property(e => e.Contractdoc).HasColumnName("contractdoc");
                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
                entity.Property(e => e.Duration).HasColumnName("duration");
                entity.Property(e => e.Enddate).HasColumnName("enddate");
                entity.Property(e => e.Fpscalyear).HasColumnName("fpsyear");
                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .HasColumnName("manager");
                entity.Property(e => e.Registereddate).HasColumnName("registereddate");
                entity.Property(e => e.Startdate).HasColumnName("startdate");
                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .HasColumnName("title");
                entity.HasQueryFilter(e => e.Fpscalyear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<Animal>(entity =>
            {
                entity.HasKey(e => e.AnimalType).HasName("tblanimals_pk__tblanimals__18ebb532");

                entity.ToTable("tblanimals", "fps");

                entity.Property(e => e.AnimalType)
                    .HasMaxLength(50)
                    .HasColumnName("animaltype");
                entity.Property(e => e.DailyRate)
                    .HasColumnType("money")
                    .HasColumnName("dailyrate");
                entity.Property(e => e.DefraDailyRate)
                    .HasColumnType("money")
                    .HasColumnName("defradailyrate");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.PlanByWeek)
                    .HasDefaultValue(false)
                    .HasColumnName("planbyweek");
                entity.Property(e => e.SecurityLevel)
                    .HasMaxLength(50)
                    .HasColumnName("security_level");
                entity.Property(e => e.Species)
                    .HasMaxLength(50)
                    .HasColumnName("species");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            modelBuilder.Entity<AnimalRequest>(entity =>
            {
                entity.HasKey(e => e.IndCounter).HasName("tblanimalreq_pk__tblanimalreq__7271068f");

                entity.ToTable("tblanimalreq", "fps");

                entity.Property(e => e.IndCounter).HasColumnName("indcounter");
                entity.Property(e => e.AnimalType)
                    .HasMaxLength(50)
                    .HasColumnName("animaltype");
                entity.Property(e => e.FpsCalYear).HasColumnName("fpsyear");
                entity.Property(e => e.JobCode)
                    .HasMaxLength(20)
                    .HasColumnName("jobcode");
                entity.Property(e => e.NumberOfAnimals).HasColumnName("numberofanimals");
                entity.Property(e => e.NumberOfDays).HasColumnName("numberofdays");
                entity.HasQueryFilter(e => e.FpsCalYear == _fPSYearContext.FPSYear);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}