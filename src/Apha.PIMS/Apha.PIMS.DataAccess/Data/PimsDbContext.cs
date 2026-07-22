/*
 * TRANSFORMENGINE MIGRATION — PimsDbContext.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - DbSet<Comment> Comments and DbSet<CommentTopic> CommentTopics registered for standalone Comments page migration
 *   - modelBuilder.ApplyConfiguration(new CommentMap()) and ApplyConfiguration(new CommentTopicMap()) registered in OnModelCreating
 *   - These DbSets support GetCommentsByProjectAsync (with optional topic filter), GetCommentTopicsAsync, and all CRUD operations
 *
 * PRESERVED:
 *   - All pre-existing DbSets (Projects, ProposedProjects, Risks, Milestones, etc.) unchanged
 *   - All pre-existing ApplyConfiguration registrations in OnModelCreating unchanged
 *   - modelBuilder.UseCollation("en_GB.utf8") unchanged
 *   - Partial class declaration unchanged
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Unused `using System`, `using System.Collections.Generic`, `using System.Text` — safe to remove in a housekeeping pass.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public partial class PimsDbContext : DbContext
    {
        
        public PimsDbContext(DbContextOptions<PimsDbContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Projects> MyTlkpProjects { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<CommentTopic> CommentTopics { get; set; }
        public virtual DbSet<ProposedProject> ProposedProjects { get; set; }
        public virtual DbSet<RadtrackProg> RadtrackProgs { get; set; }
        public virtual DbSet<ProjectDetail> ProjectDetails { get; set; }
        public virtual DbSet<ProjectLatestDetail> ProjectLatestDetails { get; set; } // New DbSet for ProjectLatestDetail
                                                                                     // Add these 4 new DbSets alongside the existing ones
        public virtual DbSet<ProjectRadTrackData> ProjectRadTrackData { get; set; }
        public virtual DbSet<Risk> Risks { get; set; }
        public virtual DbSet<ProjectStatus> ProjectStatuses { get; set; }
        public virtual DbSet<Year> Years { get; set; }
        public virtual DbSet<ProjSubContract> ProjSubContracts { get; set; }
        public virtual DbSet<AdditionalCosts> AdditionalCosts { get; set; }
        public virtual DbSet<ProjectAnimalPlan> ProjectAnimalPlans { get; set; }
        public virtual DbSet<MonthlyOutput> MonthlyOutputs { get; set; }
        public virtual DbSet<TestReqmt> TestReqmts { get; set; }
        public virtual DbSet<TimeCostCalcs> TimeCostCalcs { get; set; }
        public virtual DbSet<ProjectStaffPlan> ProjectStaffPlans { get; set; }
        public virtual DbSet<ProjectMonthFinal> ProjectMonthFinals { get; set; }
        public virtual DbSet<FpsYearTotal> FpsYearTotals { get; set; }

        public virtual DbSet<Milestone> Milestones { get; set; }
        public virtual DbSet<MilestoneFormDates> MilestoneFormDates { get; set; }
        public virtual DbSet<MilestoneType> MilestoneTypes { get; set; }
        public virtual DbSet<LogMilestone> LogMilestones { get; set; }
        public virtual DbSet<ProjectManager> ProjectManagers { get; set; }

        public virtual DbSet<StagingMilestone> StagingMilestones { get; set; }
        public virtual DbSet<RadTrackInvoice> RadTrackInvoices { get; set; }

        // Lookup: tblradtrackcontract � used by RadTrackInvoice contract dropdown.
        public virtual DbSet<RadTrackContract> RadTrackContracts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("en_GB.utf8");           

            modelBuilder.ApplyConfiguration(new ProjectMap());
            modelBuilder.ApplyConfiguration(new ProjectsMap());
            modelBuilder.ApplyConfiguration(new CommentMap());
            modelBuilder.ApplyConfiguration(new CommentTopicMap());
            modelBuilder.ApplyConfiguration(new ProposedProjectMap());
            modelBuilder.ApplyConfiguration(new RadtrackProgMap());
            modelBuilder.ApplyConfiguration(new ProjectDetailMap());
            modelBuilder.ApplyConfiguration(new ProjectLatestDetailMap());
            modelBuilder.ApplyConfiguration(new ProjectRadTrackDataMap());
            modelBuilder.ApplyConfiguration(new RiskMap());
            modelBuilder.ApplyConfiguration(new ProjectStatusMap());
            modelBuilder.ApplyConfiguration(new YearMap());
            modelBuilder.ApplyConfiguration(new ProjSubContractMap());
            modelBuilder.ApplyConfiguration(new AdditionalCostsMap());
            modelBuilder.ApplyConfiguration(new ProjectAnimalPlanMap());
            modelBuilder.ApplyConfiguration(new MonthlyOutputMap());
            modelBuilder.ApplyConfiguration(new TestReqmtMap());
            modelBuilder.ApplyConfiguration(new TimeCostCalcsMap());
            modelBuilder.ApplyConfiguration(new ProjectStaffPlanMap());
            modelBuilder.ApplyConfiguration(new ProjectMonthFinalMap());
            modelBuilder.ApplyConfiguration(new FpsYearTotalMap());
            modelBuilder.ApplyConfiguration(new MilestoneMap());
            modelBuilder.ApplyConfiguration(new MilestoneTypeMap());
            modelBuilder.ApplyConfiguration(new MilestoneFormDatesMap());
            modelBuilder.ApplyConfiguration(new LogMilestoneMap());
            modelBuilder.ApplyConfiguration(new ProjectManagerMap());
            modelBuilder.ApplyConfiguration(new StagingMilestoneMap());
            modelBuilder.ApplyConfiguration(new RadTrackInvoiceMap());
            modelBuilder.ApplyConfiguration(new RadTrackContractMap());
        }
    }
}
