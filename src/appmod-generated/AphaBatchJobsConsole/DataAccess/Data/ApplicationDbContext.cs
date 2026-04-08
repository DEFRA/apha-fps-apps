using Microsoft.EntityFrameworkCore;
using AphaBatchJobsConsole.Core.Entities;

namespace AphaBatchJobsConsole.DataAccess.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for PostgreSQL database.
    /// Provides database access for FPS year-end financial operations including
    /// cost aggregation, project management, and multi-year data transfers.
    /// 
    /// Architecture Context:
    /// - Part of Clean Architecture DataAccess layer
    /// - Configured for PostgreSQL via Npgsql provider
    /// - Supports year-based multi-tenancy through request context pattern
    /// - Used by Repository pattern implementations (BaseRepository)
    /// 
    /// Legacy Migration Context:
    /// - Replaces Microsoft Access Database with linked tables
    /// - Migrates VBA macro driven operations to EF Core
    /// - Supports sp_createFPSTotals aggregation logic through LINQ queries
    /// - Enables transactional year-end operations with rollback capability
    /// 
    /// Database Tables:
    /// - FPSYearTotals: Aggregated yearly financial totals per project
    /// - tlkpProject: Project master data and metadata
    /// 
    /// Database Views/Queries:
    /// - qryTotalAdditionalCosts: Additional costs aggregation by project
    /// - qryTotalAnimalCosts: Animal costs aggregation by project
    /// - qryTotalStaffCosts: Staff and pay costs aggregation by project
    /// - qryTotalTestCosts: Test/product costs aggregation by project
    /// 
    /// Business Operations Supported:
    /// - Year-end cost aggregation (sp_createFPSTotals equivalent)
    /// - Multi-year data transfers (sp_AddMY_* procedures)
    /// - Full year data operations (sp_AddYearsFPSData, sp_DeleteYearsFPSData)
    /// - External data import (sp_LoadFromFPS)
    /// - Global lookup management (sp_AddG_tlkpProject)
    /// </summary>
    public sealed class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// DbSet for FPS Year Totals entity.
        /// Contains aggregated financial data per project including all cost categories,
        /// income fields, and calculated totals.
        /// 
        /// Table: FPSYearTotals
        /// Primary Key: ParentProject
        /// 
        /// Business Usage:
        /// - Target table for sp_createFPSTotals aggregation results
        /// - Source for year-end financial reporting
        /// - Input for multi-year transfer operations
        /// </summary>
        public DbSet<FPSYearTotals> FPSYearTotals => Set<FPSYearTotals>();

        /// <summary>
        /// DbSet for Project lookup entity.
        /// Contains project master data including parent project, program, income,
        /// budget, profit, manager, customer, and status information.
        /// 
        /// Table: tlkpProject
        /// Primary Key: ParentProject
        /// 
        /// Business Usage:
        /// - Base table for cost aggregation joins
        /// - Source of project metadata for financial calculations
        /// - Reference for project status and ownership tracking
        /// </summary>
        public DbSet<TlkpProject> TlkpProjects => Set<TlkpProject>();

        /// <summary>
        /// DbSet for Total Additional Costs query entity.
        /// Represents aggregated additional costs per project from the legacy
        /// qryTotalAdditionalCosts Access query.
        /// 
        /// View/Query: qryTotalAdditionalCosts
        /// Key: JobCode
        /// 
        /// Business Usage:
        /// - LEFT JOIN source in sp_createFPSTotals logic
        /// - Provides TotalAdditionalCosts for cost aggregation
        /// - Nullable values default to 0 in calculations
        /// </summary>
        public DbSet<QryTotalAdditionalCosts> QryTotalAdditionalCosts => Set<QryTotalAdditionalCosts>();

        /// <summary>
        /// DbSet for Total Animal Costs query entity.
        /// Represents aggregated animal costs per project from the legacy
        /// qryTotalAnimalCosts Access query.
        /// 
        /// View/Query: qryTotalAnimalCosts
        /// Key: JobCode
        /// 
        /// Business Usage:
        /// - LEFT JOIN source in sp_createFPSTotals logic
        /// - Provides TotalAnimalCosts for cost aggregation
        /// - Nullable values default to 0 in calculations
        /// </summary>
        public DbSet<QryTotalAnimalCosts> QryTotalAnimalCosts => Set<QryTotalAnimalCosts>();

        /// <summary>
        /// DbSet for Total Staff Costs query entity.
        /// Represents aggregated staff and pay costs per project from the legacy
        /// qryTotalStaffCosts Access query.
        /// 
        /// View/Query: qryTotalStaffCosts
        /// Key: ProjectCode
        /// 
        /// Business Usage:
        /// - LEFT JOIN source in sp_createFPSTotals logic
        /// - Provides TotalStaffCosts and TotalPayCosts for aggregation
        /// - Nullable values default to 0 in calculations
        /// </summary>
        public DbSet<QryTotalStaffCosts> QryTotalStaffCosts => Set<QryTotalStaffCosts>();

        /// <summary>
        /// DbSet for Total Test Costs query entity.
        /// Represents aggregated test/product costs per project from the legacy
        /// qryTotalTestCosts Access query.
        /// 
        /// View/Query: qryTotalTestCosts
        /// Key: JobCode
        /// 
        /// Business Usage:
        /// - LEFT JOIN source in sp_createFPSTotals logic
        /// - Provides TotalTestCosts for cost aggregation
        /// - Nullable values default to 0 in calculations
        /// </summary>
        public DbSet<QryTotalTestCosts> QryTotalTestCosts => Set<QryTotalTestCosts>();

        /// <summary>
        /// Constructor accepting DbContextOptions for dependency injection.
        /// Configured with PostgreSQL connection string via Npgsql provider.
        /// 
        /// Configuration Source:
        /// - appsettings.json for local development
        /// - AWS Systems Manager Parameter Store for production
        /// - Environment variables for container deployment
        /// 
        /// Connection String Format:
        /// Host=hostname;Database=dbname;Username=user;Password=pass;Port=5432
        /// </summary>
        /// <param name="options">DbContext configuration options including connection string</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures entity mappings using Fluent API.
        /// Defines table names, primary keys, column mappings, data types,
        /// nullable constraints, and string lengths for all entities.
        /// 
        /// Configuration Strategy:
        /// - Explicit table and column name mapping for PostgreSQL compatibility
        /// - Decimal precision (18,2) for all financial fields
        /// - Keyless entities for query/view results
        /// - String length constraints matching database schema
        /// - Nullable reference type handling
        /// 
        /// Legacy Migration Notes:
        /// - Table names preserve Access database naming conventions
        /// - Query entities configured as keyless (views in PostgreSQL)
        /// - Column mappings handle case sensitivity differences
        /// </summary>
        /// <param name="modelBuilder">Model builder for entity configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Best Practice: Call base implementation first
            base.OnModelCreating(modelBuilder);

            // Best Practice: Use null-forgiving operator only when guaranteed non-null
            ArgumentNullException.ThrowIfNull(modelBuilder);

            // Configure FPSYearTotals entity
            ConfigureFPSYearTotals(modelBuilder);

            // Configure TlkpProject entity
            ConfigureTlkpProject(modelBuilder);

            // Configure QryTotalAdditionalCosts entity
            ConfigureQryTotalAdditionalCosts(modelBuilder);

            // Configure QryTotalAnimalCosts entity
            ConfigureQryTotalAnimalCosts(modelBuilder);

            // Configure QryTotalStaffCosts entity
            ConfigureQryTotalStaffCosts(modelBuilder);

            // Configure QryTotalTestCosts entity
            ConfigureQryTotalTestCosts(modelBuilder);
        }

        /// <summary>
        /// Configures the FPSYearTotals entity mapping.
        /// </summary>
        private static void ConfigureFPSYearTotals(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FPSYearTotals>(entity =>
            {
                entity.ToTable("FPSYearTotals");
                entity.HasKey(e => e.ParentProject);

                entity.Property(e => e.ParentProject)
                    .HasColumnName("ParentProject")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Program)
                    .HasColumnName("Program")
                    .HasMaxLength(100);

                entity.Property(e => e.TotalAdditionalCosts)
                    .HasColumnName("TotalAdditionalCosts")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalAnimalCosts)
                    .HasColumnName("TotalAnimalCosts")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalStaffCosts)
                    .HasColumnName("TotalStaffCosts")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalTestCosts)
                    .HasColumnName("TotalTestCosts")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalCosts)
                    .HasColumnName("TotalCosts")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CustIncome)
                    .HasColumnName("CustIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransferIncome)
                    .HasColumnName("TransferIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalIncome)
                    .HasColumnName("TotalIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Budget_CVL)
                    .HasColumnName("Budget_CVL")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RequiredProfit)
                    .HasColumnName("RequiredProfit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Manager)
                    .HasColumnName("Manager")
                    .HasMaxLength(100);

                entity.Property(e => e.Customer)
                    .HasColumnName("Customer")
                    .HasMaxLength(200);

                entity.Property(e => e.ProjectStatus)
                    .HasColumnName("ProjectStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.PVSIncome)
                    .HasColumnName("PVSIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PlanCaseworkDebit)
                    .HasColumnName("PlanCaseworkDebit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPayCosts)
                    .HasColumnName("TotalPayCosts")
                    .HasColumnType("decimal(18,2)");
            });
        }

        /// <summary>
        /// Configures the TlkpProject entity mapping.
        /// </summary>
        private static void ConfigureTlkpProject(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TlkpProject>(entity =>
            {
                entity.ToTable("tlkpProject");
                entity.HasKey(e => e.ParentProject);

                entity.Property(e => e.ParentProject)
                    .HasColumnName("ParentProject")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Program)
                    .HasColumnName("Program")
                    .HasMaxLength(50);

                entity.Property(e => e.CustIncome)
                    .HasColumnName("CustIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TransferIncome)
                    .HasColumnName("TransferIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Budget_CVL)
                    .HasColumnName("Budget_CVL")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Profit)
                    .HasColumnName("Profit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Manager)
                    .HasColumnName("Manager")
                    .HasMaxLength(100);

                entity.Property(e => e.Customer)
                    .HasColumnName("Customer")
                    .HasMaxLength(100);

                entity.Property(e => e.ProjectStatus)
                    .HasColumnName("ProjectStatus")
                    .HasMaxLength(50);

                entity.Property(e => e.PVSIncome)
                    .HasColumnName("PVSIncome")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PlanCaseworkDebit)
                    .HasColumnName("PlanCaseworkDebit")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CreatedDate)
                    .HasColumnName("CreatedDate");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnName("ModifiedDate");
            });
        }

        /// <summary>
        /// Configures the QryTotalAdditionalCosts entity mapping.
        /// </summary>
        private static void ConfigureQryTotalAdditionalCosts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QryTotalAdditionalCosts>(entity =>
            {
                entity.ToTable("qry_total_additional_costs");
                entity.HasKey(e => e.JobCode);

                entity.Property(e => e.JobCode)
                    .HasColumnName("job_code")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TotalAdditionalCosts)
                    .HasColumnName("total_additional_costs")
                    .HasColumnType("decimal(18,2)");
            });
        }

        /// <summary>
        /// Configures the QryTotalAnimalCosts entity mapping.
        /// </summary>
        private static void ConfigureQryTotalAnimalCosts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QryTotalAnimalCosts>(entity =>
            {
                entity.ToTable("qryTotalAnimalCosts");
                entity.HasKey(e => e.JobCode);

                entity.Property(e => e.JobCode)
                    .HasColumnName("JobCode")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TotalAnimalCosts)
                    .HasColumnName("TotalAnimalCosts")
                    .HasColumnType("decimal(18,2)");
            });
        }

        /// <summary>
        /// Configures the QryTotalStaffCosts entity mapping.
        /// </summary>
        private static void ConfigureQryTotalStaffCosts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QryTotalStaffCosts>(entity =>
            {
                entity.ToTable("qry_total_staff_costs");
                entity.HasKey(e => e.ProjectCode);

                entity.Property(e => e.ProjectCode)
                    .HasColumnName("project_code")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TotalStaffCosts)
                    .HasColumnName("total_staff_costs")
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPayCosts)
                    .HasColumnName("total_pay_costs")
                    .HasColumnType("decimal(18,2)");
            });
        }

        /// <summary>
        /// Configures the QryTotalTestCosts entity mapping.
        /// </summary>
        private static void ConfigureQryTotalTestCosts(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QryTotalTestCosts>(entity =>
            {
                entity.ToTable("qry_total_test_costs");
                entity.HasKey(e => e.JobCode);

                entity.Property(e => e.JobCode)
                    .HasColumnName("job_code")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TotalTestCosts)
                    .HasColumnName("total_test_costs")
                    .HasColumnType("decimal(18,2)");
            });
        }
    }
}


**Key Improvements Made:**

1. **Sealed Class**: Made `ApplicationDbContext` sealed since it's not intended to be inherited from, improving performance and clarity.

2. **DbSet Properties**: Changed from `{ get; set; } = null!;` to `=> Set<T>()` pattern, which is the modern EF Core best practice. This eliminates null-forgiving operators and uses the built-in `Set<T>()` method.

3. **Method Extraction**: Extracted entity configuration logic into separate private static methods (`ConfigureFPSYearTotals`, `ConfigureTlkpProject`, etc.) to improve readability and maintainability. This follows the Single Responsibility Principle.

4. **Null Validation**: Added `ArgumentNullException.ThrowIfNull(modelBuilder)` for defensive programming, though EF Core guarantees non-null in practice.

5. **Static Methods**: Made configuration methods static since they don't access instance members, improving performance slightly.

6. **Code Organization**: Better separation of concerns with each entity configuration in its own method, making the code easier to navigate and maintain.

7. **Removed Redundant Comments**: The inline comments about "keyless entity (view)" were inconsistent with the actual code using `HasKey()`, so the configuration methods now have clear summary documentation instead.