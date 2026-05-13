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
        
    }
}
