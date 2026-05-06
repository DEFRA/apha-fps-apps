using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess.Data;

public partial class CostbookDbContext : DbContext
{
    private const string FpsYearColumnName = "fpsyear";
    private const string FpsSchemaName = "fps";
    private const string MabArchiveSchemaName = "mabarchive";
    private const string MoneyColumnType = "money";
    private const string CitextColumnType = "citext";

    private readonly IFPSYearContext _fPSYearContext;
    public CostbookDbContext(DbContextOptions<CostbookDbContext> options, IFPSYearContext fPSYearContext)
            : base(options)
    {
        _fPSYearContext = fPSYearContext;

    }
    public virtual DbSet<Disease> Diseases { get; set; }

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }

    public virtual DbSet<AdditionalCost> AdditionalCosts { get; set; }

    public virtual DbSet<AnimalRequirement> AnimalRequirements { get; set; }

    public virtual DbSet<ProjectYear> ProjectYears { get; set; }

    public virtual DbSet<StaffRequirement> StaffRequirements { get; set; }  
    public virtual DbSet<TestRequirement> TestRequirements { get; set; }
        
    public virtual DbSet<Settings> DatabaseSettings { get; set; }
    public virtual DbSet<FpsTestOrProduct> FpsTestorProducts { get; set; }
    public virtual DbSet<FpsAnimals> FpsAnimals { get; set; }
    public virtual DbSet<FpsAccountCategory> FpsAccountCategories { get; set; }
    public virtual DbSet<AccountGroup> AccountGroups { get; set; }
    public virtual DbSet<WorkGroupGrade> WorkGroupGrades { get; set; }

    public virtual DbSet<ProfitCentreGrade> ProfitCentreGrades { get; set; }

    public virtual DbSet<FpsAdditionalCost> FpsAdditionalCosts { get; set; }

    public virtual DbSet<EuGradeConversion> EuGradeConversions { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.DiseaseName).HasName("tbldisease_pk___4__10");

            entity.ToTable("tbldisease", FpsSchemaName);

            entity.Property(e => e.DiseaseName)
                .HasMaxLength(50)
                .HasColumnName("disease");
        });

        modelBuilder.Entity<Program>(entity =>
        {
            entity.HasKey(e => e.ProgramNo).HasName("tlkpprogram_pk__tlkpprogram__2180fb33");

            entity.ToTable("tlkpprogram", FpsSchemaName);

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
            entity.Property(e => e.FpScalYear).HasColumnName(FpsYearColumnName);
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
                .HasColumnType(MoneyColumnType)
                .HasColumnName("target");
            entity.HasQueryFilter(e => e.FpScalYear == _fPSYearContext.FPSYear);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerName).HasName("tlkpcustomer_pk___1__15");

            entity.ToTable("tlkpcustomer", FpsSchemaName);

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("tblproject_aaaaatblproject_pk");

            entity.ToTable("tblproject", MabArchiveSchemaName);

            entity.Property(e => e.ProjectId)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .HasColumnName("contract number");
            entity.Property(e => e.ContractPrice).HasColumnName("contractprice");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer name");
            entity.Property(e => e.DateOfSubmission).HasColumnName("date of submission");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Euroconvrate).HasColumnName("euroconvrate");
            entity.Property(e => e.FinancialYears).HasColumnName("financialyears");
            entity.Property(e => e.Inflation)
                .HasDefaultValue(0)
                .HasColumnName("inflation");
            entity.Property(e => e. IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.PlanCategory)
                .HasMaxLength(50)
                .HasColumnName("plancat");
            entity.Property(e => e.PreparedBy)
                .HasMaxLength(50)
                .HasColumnName("prepared by");
            entity.Property(e => e.Programme)
                .HasMaxLength(50)
                .HasColumnName("programme");
            entity.Property(e => e.ProjectTitle)
                .HasMaxLength(100)
                .HasColumnName("projecttitle");
            entity.Property(e => e.ProjectWorkgroup)
                .HasMaxLength(50)
                .HasColumnName("projectworkgroup");
            entity.Property(e => e.StartDate).HasColumnName("startdate");
            entity.Property(e => e.StartFYear)
                .HasDefaultValueSql("0")
                .HasColumnName("startfyear");
            entity.Property(e => e.SubmittedByFName)
                .HasMaxLength(50)
                .HasColumnName("submittedbyfname");
            entity.Property(e => e.SubmittedByLName)
                .HasMaxLength(50)
                .HasColumnName("submittedbylname");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Mnumber).HasName("tblcapsstaff_pk_tblcapsstaff");

            entity.ToTable("tblcapsstaff", MabArchiveSchemaName);

            entity.Property(e => e.Mnumber)
                .HasMaxLength(50)
                .HasColumnName("mnumber");
            entity.Property(e => e.Dt2number)
                .HasMaxLength(50)
                .HasColumnName("dt2number");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });
        modelBuilder.Entity<FpsTestOrProduct>(entity =>
        {
            entity.HasKey(e => new { e.ItemCode, e.FpsYear }).HasName("pk_testorproduct");

            entity.ToTable("testorproduct", FpsSchemaName);

            entity.Property(e => e.ItemCode)
                .HasColumnType(CitextColumnType)
                .HasColumnName("itemcode");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.ChargeMethod)
                .HasMaxLength(5)
                .HasColumnName("chargemethod");
            entity.Property(e => e.DefraUnitPrice)
                .HasColumnType(MoneyColumnType)
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
                .HasColumnType(MoneyColumnType)
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
                .HasColumnType(MoneyColumnType)
                .HasColumnName("unitpricevla");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        });

        modelBuilder.Entity<FpsAnimals>(entity =>
        {
            entity.HasKey(e => new { e.AnimalType, e.FpsYear }).HasName("pk_tblanimals");

            entity.ToTable("tblanimals", FpsSchemaName);

            entity.Property(e => e.AnimalType)
                .HasColumnType(CitextColumnType)
                .HasColumnName("animaltype");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.DailyRate)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("dailyrate");
            entity.Property(e => e.DefraDailyRate)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("defradailyrate");
            entity.Property(e => e.PlanByWeek)
                .HasDefaultValue(false)
                .HasColumnName("planbyweek");
            entity.Property(e => e.SecurityLevel)
                .HasMaxLength(50)
                .HasColumnName("security_level");
            entity.Property(e => e.Species)
                .HasMaxLength(50)
                .HasColumnName("species");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
        });

        modelBuilder.Entity<FpsAdditionalCost>(entity =>
        {
            entity.HasKey(e => new { e.JobCode, e.Account, e.Description, e.FpsYear }).HasName("pk_tbladditionalcosts");

            entity.ToTable("tbladditionalcosts", FpsSchemaName);

            entity.Property(e => e.JobCode)
                .HasColumnType(CitextColumnType)
                .HasColumnName("jobcode");
            entity.Property(e => e.Account)
                .HasColumnType(CitextColumnType)
                .HasColumnName("account");
            entity.Property(e => e.Description)
                .HasMaxLength(20)
                .HasColumnName("description");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.Frequency)
                .HasMaxLength(5)
                .HasColumnName("freq");
            entity.Property(e => e.ItemCost)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("itemcost");
            entity.Property(e => e.Supplier)
                .HasMaxLength(50)
                .HasColumnName("supplier");

            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
        });
        modelBuilder.Entity<FpsAccountCategory>(entity =>
        {
            entity.HasKey(e => new { e.AccShortName, e.FpsYear }).HasName("pk_tblkpaccountcategory");

            entity.ToTable("tblkpaccountcategory", FpsSchemaName);

            entity.HasIndex(e => e.AccountType, "accounttype");

            entity.Property(e => e.AccShortName)
                .HasColumnType(CitextColumnType)
                .HasColumnName("accshortname");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.AccountDescription)
                .HasMaxLength(50)
                .HasColumnName("accountdescription");
            entity.Property(e => e.AccountType)
                .HasColumnType(CitextColumnType)
                .HasColumnName("accounttype");
            entity.Property(e => e.ConstituentAccountCodes)
                .HasMaxLength(100)
                .HasColumnName("constituentaccountcodes");
            entity.Property(e => e.Csg7Group)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("csg7_group");
            entity.Property(e => e.ProjectSpecific).HasColumnName("projectspecific");
            entity.Property(e => e.RcSpecific).HasColumnName("rcspecific");

            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
        });
        modelBuilder.Entity<AccountGroup>(entity =>
        {
            entity.HasKey(e => e.Csg7group).HasName("aaaaatblcsg7_accountgroups_pk");

            entity.ToTable("tblcsg7_accountgroups", MabArchiveSchemaName);

            entity.Property(e => e.Csg7group)
                .HasMaxLength(15)
                .HasColumnName("csg7group");
            entity.Property(e => e.Useinflation)
                .HasDefaultValue(true)
                .HasColumnName("useinflation");
        });
        modelBuilder.Entity<WorkGroupGrade>(entity =>
        {
            entity.HasKey(e => new { e.WgGrade, e.FpsYear }).HasName("pk_workgroupgrade");

            entity.ToTable("workgroupgrade", FpsSchemaName);

            entity.Property(e => e.WgGrade)
                .HasColumnType(CitextColumnType)
                .HasColumnName("wggrade");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.AvSalary)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("avsalary");
            entity.Property(e => e.ChargeRateWg)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("chargeratewg");
            entity.Property(e => e.DirectRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("directratewg");
            entity.Property(e => e.GradeCode)
                .HasColumnType(CitextColumnType)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsChangedBy)
                .HasMaxLength(50)
                .HasColumnName("hrschangedby");
            entity.Property(e => e.NprWg)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("nprwg");
            entity.Property(e => e.OhrWg)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("ohrwg");
            entity.Property(e => e.PayRateWg)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("payratewg");
            entity.Property(e => e.ProfitCentreGrade)
                .HasColumnType(CitextColumnType)
                .HasColumnName("profitcentregrade");
            entity.Property(e => e.WorkGroup)
                .HasColumnType(CitextColumnType)
                .HasColumnName("workgroup");            

            entity.HasOne(d => d.ProfitCentreGradeNavigation).WithMany(p => p.WorkGroupGrades)
                .HasForeignKey(d => new { d.ProfitCentreGrade, d.FpsYear })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_workgroupgrade_profitcentregrade");

            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
        });
        modelBuilder.Entity<ProfitCentreGrade>(entity =>
        {
            entity.HasKey(e => new { e.PcGrade , e.FpsYear }).HasName("pk_profitcentregrade");

            entity.ToTable("profitcentregrade", FpsSchemaName);

            entity.HasIndex(e => e.ProfitCentre, "profitcentregrade_profitcentre");

            entity.Property(e => e.PcGrade)
                .HasColumnType(CitextColumnType)
                .HasColumnName("pcgrade");
            entity.Property(e => e.FpsYear).HasColumnName(FpsYearColumnName);
            entity.Property(e => e.ChargeRate)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("chargerate");
            entity.Property(e => e.DefraChargeRate)
                .HasColumnType(MoneyColumnType)
                .HasColumnName("defrachargerate");
            entity.Property(e => e.DirectRate)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("directrate");
            entity.Property(e => e.DivisionGrade)
                .HasColumnType(CitextColumnType)
                .HasColumnName("divisiongrade");
            entity.Property(e => e.GradeCode)
                .HasColumnType(CitextColumnType)
                .HasColumnName("gradecode");
            entity.Property(e => e.HrsAvailable)
                .HasDefaultValueSql("0")
                .HasColumnName("hrsavailable");
            entity.Property(e => e.Npr)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("npr");
            entity.Property(e => e.Ohr)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("ohr");
            entity.Property(e => e.OldChargeRate)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("oldchargerate");
            entity.Property(e => e.PayRate)
                .HasDefaultValueSql("0")
                .HasColumnType(MoneyColumnType)
                .HasColumnName("payrate");
            entity.Property(e => e.ProfitCentre)
                .HasColumnType(CitextColumnType)
                .HasColumnName("profitcentre");

           
        });

        modelBuilder.Entity<EuGradeConversion>(entity =>
        {
            entity.HasKey(e => e.VlaGrade).HasName("pk_tbleugrade_conversion");

            entity.ToTable("tbleugrade_conversion", MabArchiveSchemaName);

            entity.Property(e => e.VlaGrade)
                .HasMaxLength(50)
                .HasColumnName("vlagrade");

            entity.Property(e => e.EuGrade)
                .HasMaxLength(50)
                .HasColumnName("eugrade");
        });

        modelBuilder.ApplyConfiguration(new StaffRequirementMap());
        modelBuilder.ApplyConfiguration(new TestRequirementMap());
        modelBuilder.ApplyConfiguration(new AnimalRequirementMap());
        modelBuilder.ApplyConfiguration(new AdditionalCostMap());
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CostbookDbContext).Assembly);

    }
}
