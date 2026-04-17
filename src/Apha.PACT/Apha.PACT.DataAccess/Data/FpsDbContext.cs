using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsRequestContext _fpsRequestContext;

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
            modelBuilder.ApplyConfiguration(new ProjectMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new WorkGroupMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new TimeCodeValidMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new JobCodeMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new TestCapabilityMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new TestRequirementMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new TestorProductMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new TestRequirementLogMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new MonthlyOutputMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new ProjectInvoiceMap(_fpsRequestContext));
            modelBuilder.ApplyConfiguration(new ProjectSubContractMap(_fpsRequestContext));
        }
    }
}