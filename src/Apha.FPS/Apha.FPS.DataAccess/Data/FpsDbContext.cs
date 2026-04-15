using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Data
{
    public partial class FpsDbContext : DbContext
    {
        private readonly IFpsRequestContext _fPSYearContext;

        public FpsDbContext(DbContextOptions<FpsDbContext> options, IFpsRequestContext fPSYearContext)
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
        public virtual DbSet<ProjectGroup> ProjectGroups { get; set; }
        public virtual DbSet<AccountCode> AccountCodes { get; set; }
        public virtual DbSet<SubAccount> SubAccounts { get; set; }
        public virtual DbSet<UserCategory> UserCategories { get; set; }
        public virtual DbSet<StaffActiveView> StaffActiveView { get; set; }
        public virtual DbSet<WorkgroupGradeGeneralView> WorkgroupGradeGeneralView { get; set; }

        public virtual DbSet<ProgramView> ProgramViews { get; set; }
        public virtual DbSet<ProjectView> ProjectViews { get; set; }
        public virtual DbSet<StaffJobTblView> StaffJobTblViews { get; set; }
        public virtual DbSet<StaffGeneralView> StaffGeneralViews { get; set; }
        public virtual DbSet<StaffView> StaffViews { get; set; }
        public virtual DbSet<StaffPickView> StaffPickViews { get; set; }
        public virtual DbSet<AnimalRequestView> AnimalRequestViews { get; set; }
        public virtual DbSet<PactProjectView> PactProjectViews { get; set; }
        public virtual DbSet<PactWorkGroupGradeView> PactWorkGroupGradeViews { get; set; }
        public virtual DbSet<YearMaster> YearMasters { get; set; }
        public virtual DbSet<StaffJobLog> StaffJobLogs { get; set; }
        public virtual DbSet<AnimalRequestLog> AnimalRequestLogs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new UserProgramMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProgramMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProjectMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffJobMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new WgEmployeeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new EmployeeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new FpsSettingMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new WorkgroupMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new WorkgroupGradeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProfitCentreGradeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new UserProfitcentreMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProfitCentreMap());
            modelBuilder.ApplyConfiguration(new JobCodeMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StatusMap());
            modelBuilder.ApplyConfiguration(new DiseaseMap());
            modelBuilder.ApplyConfiguration(new CustomerMap());
            modelBuilder.ApplyConfiguration(new ContractMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new AnimalMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new AnimalRequestMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProjectGroupMap());
            modelBuilder.ApplyConfiguration(new AccountCodeMap());
            modelBuilder.ApplyConfiguration(new SubAccountMap());
            modelBuilder.ApplyConfiguration(new UserCategoryMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffActiveViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new WorkgroupGradeGeneralViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProgramViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new ProjectViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffJobTblViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffGeneralViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new StaffPickViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new AnimalRequestViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new PactProjectViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new PactWorkGroupGradeViewMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new YearMasterMap());
            modelBuilder.ApplyConfiguration(new StaffJobLogMap(_fPSYearContext));
            modelBuilder.ApplyConfiguration(new AnimalRequestLogMap(_fPSYearContext));
        }       
    }
}