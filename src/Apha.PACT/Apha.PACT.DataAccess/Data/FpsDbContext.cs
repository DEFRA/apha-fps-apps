using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsRequestContext _fpsRequestContext;
        public int FilterFpsYear => _fpsRequestContext.FpsYear;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsRequestContext fpsRequestContext)
            : base(options)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<JobCode> JobCodes { get; set; }
        public virtual DbSet<TimeCodeValid> TimeCodeValids { get; set; }
        public virtual DbSet<WorkGroup> WorkGroups { get; set; }
        public virtual DbSet<ProjectInvoice> ProjectInvoices { get; set; }
        public virtual DbSet<ProjectSubContract> ProjectSubContracts { get; set; }
        public virtual DbSet<TestCapability> TestCapabilities { get; set; }
        public virtual DbSet<TestRequirement> TestRequirements { get; set; }
        public virtual DbSet<TestorProduct> TestorProducts { get; set; }
        public virtual DbSet<TestRequirementLog> TestRequirementLogs { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectMap());
            modelBuilder.Entity<Project>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new WorkGroupMap());
            modelBuilder.Entity<WorkGroup>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TimeCodeValidMap());
            modelBuilder.Entity<TimeCodeValid>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new JobCodeMap());
            modelBuilder.Entity<JobCode>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestCapabilityMap());
            modelBuilder.Entity<TestCapability>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementMap());
            modelBuilder.Entity<TestRequirement>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestorProductMap());
            modelBuilder.Entity<TestorProduct>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new TestRequirementLogMap());
            modelBuilder.Entity<TestRequirementLog>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyOutputMap());
            modelBuilder.Entity<MonthlyOutput>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectInvoiceMap());
            modelBuilder.Entity<ProjectInvoice>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.Entity<TestOrProduct>(entity =>
            {
                entity.HasKey(e => new { e.ItemCode, e.FpsYear }).HasName("pk_testorproduct");

                entity.ToTable("testorproduct", "fps");

                entity.Property(e => e.ItemCode)
                    .HasMaxLength(50)
                    .HasColumnName("itemcode");
                entity.Property(e => e.FpsYear).HasColumnName(fpsYear);
                entity.Property(e => e.ItemDescription)
                    .HasMaxLength(255)
                    .HasColumnName("itemdescription");
                entity.Property(e => e.TestManager)
                    .HasMaxLength(50)
                    .HasColumnName("testmanager");
                entity.Property(e => e.JobStatus)
                    .HasMaxLength(50)
                    .HasColumnName("jobstatus");
                entity.Property(e => e.UnitPriceVla)
                    .HasColumnType("money")
                    .HasColumnName("unitpricevla");
                entity.Property(e => e.PriceAhvg)
                    .HasColumnType("money")
                    .HasColumnName("priceahvg");
                entity.Property(e => e.Owner)
                    .HasMaxLength(10)
                    .HasColumnName("owner");
                entity.Property(e => e.ChargeMethod)
                    .HasMaxLength(50)
                    .HasColumnName("chargemethod");
                entity.Property(e => e.ShortDescription)
                    .HasMaxLength(100)
                    .HasColumnName("shortdescription");
                entity.Property(e => e.DefraUnitPrice)
                    .HasColumnType("money")
                    .HasColumnName("defraunitprice");

                entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
            });


            modelBuilder.ApplyConfiguration(new ProjectSubContractMap());
            modelBuilder.Entity<ProjectSubContract>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);
        }
    }
}