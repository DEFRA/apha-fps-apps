using System;
using System.Collections.Generic;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Apha.Costbook.DataAccess.Data;

public partial class CostbookDbContext : DbContext
{
    public CostbookDbContext()
    {
    }
    public CostbookDbContext(DbContextOptions<CostbookDbContext> options)
        : base(options)
    {
    }
    public virtual DbSet<Disease> Diseases { get; set; } 

    public virtual DbSet<Program> Programs { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Staff> Staffs { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       modelBuilder.Entity<Disease>(entity =>
        {
            entity.HasKey(e => e.DiseaseName).HasName("tbldisease_pk___4__10");

            entity.ToTable("tbldisease", "fps");

            entity.Property(e => e.DiseaseName)
                .HasMaxLength(50)
                .HasColumnName("disease");
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
            entity.Property(e => e.FpScalYear).HasColumnName("fpsyear");
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
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerName).HasName("tlkpcustomer_pk___1__15");

            entity.ToTable("tlkpcustomer", "fps");

            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("tblproject_aaaaatblproject_pk");

            entity.ToTable("tblproject", "mabarchive");

            entity.Property(e => e.ProjectId)
                .HasMaxLength(50)
                .HasColumnName("project");
            entity.Property(e => e.ContractNumber)
                .HasMaxLength(50)
                .HasColumnName("contract number");
            entity.Property(e => e.Contractprice).HasColumnName("contractprice");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(50)
                .HasColumnName("customer name");
            entity.Property(e => e.DateOfSubmission).HasColumnName("date of submission");
            entity.Property(e => e.Disease)
                .HasMaxLength(50)
                .HasColumnName("disease");
            entity.Property(e => e.Euroconvrate).HasColumnName("euroconvrate");
            entity.Property(e => e.Financialyears).HasColumnName("financialyears");
            entity.Property(e => e.Inflation)
                .HasDefaultValue(0)
                .HasColumnName("inflation");
            entity.Property(e => e.Isdefraproject).HasColumnName("isdefraproject");
            entity.Property(e => e.Notes)
                .HasMaxLength(255)
                .HasColumnName("notes");
            entity.Property(e => e.Plancat)
                .HasMaxLength(50)
                .HasColumnName("plancat");
            entity.Property(e => e.PreparedBy)
                .HasMaxLength(50)
                .HasColumnName("prepared by");
            entity.Property(e => e.Programme)
                .HasMaxLength(50)
                .HasColumnName("programme");
            entity.Property(e => e.Projecttitle)
                .HasMaxLength(100)
                .HasColumnName("projecttitle");
            entity.Property(e => e.Projectworkgroup)
                .HasMaxLength(50)
                .HasColumnName("projectworkgroup");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Startfyear)
                .HasDefaultValueSql("0")
                .HasColumnName("startfyear");
            entity.Property(e => e.Submittedbyfname)
                .HasMaxLength(50)
                .HasColumnName("submittedbyfname");
            entity.Property(e => e.Submittedbylname)
                .HasMaxLength(50)
                .HasColumnName("submittedbylname");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Mnumber).HasName("tblcapsstaff_pk_tblcapsstaff");

            entity.ToTable("tblcapsstaff", "mabarchive");

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
       
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CostbookDbContext).Assembly);        
    }    
}
