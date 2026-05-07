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
        public virtual DbSet<Month> Months { get; set; }
        public virtual DbSet<TestRequirementLog> TestRequirementLogs { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }
        public virtual DbSet<MonthlyTime> MonthlyTimes { get; set; }
        public virtual DbSet<MonthlyInvoicesSummary> MonthlyInvoicesSummary { get; set; }
        public virtual DbSet<ProjectMonth> ProjectMonths { get; set; }
        public virtual DbSet<ProjectMonthFinal> ProjectMonthFinals { get; set; }
        public virtual DbSet<PeriodMonth> PeriodMonths { get; set; }

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

            modelBuilder.ApplyConfiguration(new ProjectSubContractMap());
            modelBuilder.Entity<ProjectSubContract>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthlyTimeMap());
            modelBuilder.Entity<MonthlyTime>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new MonthMap());


            modelBuilder.ApplyConfiguration(new MonthlyInvoicesSummaryMap());
            modelBuilder.Entity<MonthlyInvoicesSummary>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectMonthFinalMap());
            modelBuilder.Entity<ProjectMonthFinal>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new ProjectMonthMap());
            modelBuilder.Entity<ProjectMonth>().HasQueryFilter(e => e.FpsYear == FilterFpsYear);

            modelBuilder.ApplyConfiguration(new PeriodMonthMap());
        }
    }
}