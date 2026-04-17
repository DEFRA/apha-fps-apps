using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
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
        public virtual DbSet<TestCapability> TestCapabilities { get; set; }
        public virtual DbSet<TestRequirement> TestRequirements { get; set; }
        public virtual DbSet<TestorProduct> TestorProducts { get; set; }
        public virtual DbSet<TestRequirementLog> TestRequirementLogs { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProjectMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new WorkGroupMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new TimeCodeValidMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new JobCodeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new TestCapabilityMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new TestRequirementMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new TestorProductMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new TestRequirementLogMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new MonthlyOutputMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProjectInvoiceMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProjectSubContractMap(_fPSYearContext));
        }
    }
}