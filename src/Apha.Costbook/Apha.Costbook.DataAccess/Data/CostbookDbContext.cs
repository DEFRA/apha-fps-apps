/*
 * TRANSFORMENGINE MIGRATION — CostbookDbContext.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Added DbSet<CapsStaff> CapsStaffs property for mabarchive.tblcapsstaff
 *   - Registered CapsStaffMap via ApplyConfiguration in OnModelCreating
 *   - All other DbSet and map registrations preserved unchanged
 *
 * PRESERVED:
 *   - All existing DbSet properties and HasQueryFilter registrations
 *   - IFPSYearContext injection for year-scoped entity filters
 *   - All other ApplyConfiguration calls
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: CapsStaff has no FpsYear column — no HasQueryFilter applied (correct per DDL)
 */

using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Apha.Costbook.DataAccess.Data;

public partial class CostbookDbContext : DbContext
{
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

    public virtual DbSet<YearMaster> YearMasters { get; set; }

    // TRANSFORMENGINE: Added — DbSet for mabarchive.tblcapsstaff (CapsStaff CAPS Staff Tab)
    public virtual DbSet<CapsStaff> CapsStaffs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DiseaseMap());

        modelBuilder.ApplyConfiguration(new ProgramMap());
        modelBuilder.Entity<Program>().HasQueryFilter(e => e.FpScalYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new CustomerMap());

        modelBuilder.ApplyConfiguration(new ProjectMap());
        modelBuilder.ApplyConfiguration(new StaffMap());

        modelBuilder.ApplyConfiguration(new FpsTestOrProductMap());
        modelBuilder.Entity<FpsTestOrProduct>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new FpsAnimalsMap());
        modelBuilder.Entity<FpsAnimals>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new FpsAdditionalCostMap());
        modelBuilder.Entity<FpsAdditionalCost>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new FpsAccountCategoryMap());
        modelBuilder.Entity<FpsAccountCategory>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new AccountGroupMap());

        modelBuilder.ApplyConfiguration(new WorkGroupGradeMap());
        modelBuilder.Entity<WorkGroupGrade>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new ProfitCentreGradeMap());
        modelBuilder.Entity<ProfitCentreGrade>().HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);

        modelBuilder.ApplyConfiguration(new EuGradeConversionMap());       

        modelBuilder.ApplyConfiguration(new StaffRequirementMap());
        modelBuilder.ApplyConfiguration(new TestRequirementMap());
        modelBuilder.ApplyConfiguration(new AnimalRequirementMap());
        modelBuilder.ApplyConfiguration(new AdditionalCostMap());

        modelBuilder.ApplyConfiguration(new SettingsMap());
        modelBuilder.ApplyConfiguration(new ProjectYearMap());
        modelBuilder.ApplyConfiguration(new YearMasterMap());

        // TRANSFORMENGINE: Added — CapsStaffMap registration for mabarchive.tblcapsstaff (no year filter — no fpsyear column)
        modelBuilder.ApplyConfiguration(new CapsStaffMap());

    }
}
