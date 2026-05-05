using Apha.BatchJobs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>
/// Database context for batch jobs fps schema.
/// </summary>
public class BatchJobsDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the BatchJobsDbContext.
    /// </summary>
    /// <param name="options">DbContext configuration options.</param>
    public BatchJobsDbContext(DbContextOptions<BatchJobsDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the batch locks table.
    /// </summary>
    public DbSet<BatchLock> BatchLocks { get; set; }

    /// <summary>
    /// Gets or sets the foundation job master table.
    /// </summary>
    internal DbSet<TblJobMaster> TblJobMaster { get; set; }

    /// <summary>
    /// Gets or sets the foundation job status table.
    /// </summary>
    internal DbSet<TblJobStatus> TblJobStatus { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue table.
    /// </summary>
    internal DbSet<TblJobQueue> TblJobQueue { get; set; }

    /// <summary>
    /// Gets or sets the foundation job queue log table.
    /// </summary>
    internal DbSet<TblJobQueueLog> TblJobQueueLog { get; set; }

    /// <summary>
    /// Gets or sets scheduled load run lifecycle rows.
    /// </summary>
    internal DbSet<ScheduledLoadRunTable> ScheduledLoadRun { get; set; }

    /// <summary>
    /// Gets or sets scheduled load step audit rows.
    /// </summary>
    internal DbSet<ScheduledLoadStepRunTable> ScheduledLoadStepRun { get; set; }

    /// <summary>
    /// Gets or sets scheduled load validation result rows.
    /// </summary>
    internal DbSet<ScheduledLoadValidationResultTable> ScheduledLoadValidationResult { get; set; }

    /// <summary>
    /// Gets or sets source fixture rows for FPS year processing.
    /// </summary>
    internal DbSet<FpsSourceProjectYearTable> FpsSourceProjectYear { get; set; }

    /// <summary>
    /// Gets or sets yearly totals rows.
    /// </summary>
    internal DbSet<FpsYearTotalsTable> FpsYearTotals { get; set; }

    /// <summary>
    /// Gets or sets archived yearly totals rows.
    /// </summary>
    internal DbSet<FpsYearArchiveTable> FpsYearArchive { get; set; }

    /// <summary>
    /// Gets or sets current year project snapshot rows.
    /// </summary>
    internal DbSet<FpsProjectAllCurrentYearTable> FpsProjectAllCurrentYear { get; set; }

    /// <summary>
    /// Configures the model for the database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BatchLock table — mirrors fps.job_lock
        modelBuilder.Entity<BatchLock>(entity =>
        {
            entity.ToTable("job_lock", schema: "fps");
            entity.HasKey(e => e.LockId);
            entity.Property(e => e.LockId).HasColumnName("lock_id").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired().HasMaxLength(255);
            entity.Property(e => e.AcquiredAt).HasColumnName("acquired_at").IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at").IsRequired();
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired().HasMaxLength(64);
            entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
            entity.HasIndex(e => e.JobName).HasDatabaseName("idx_job_lock_job_name");
            entity.HasIndex(e => new { e.JobName, e.IsActive }).HasDatabaseName("idx_job_lock_job_name_active");
            entity.HasIndex(e => e.ExpiresAt).HasDatabaseName("idx_job_lock_expires_at");
        });

        // Configure foundation job master table — mirrors fps.job_master
        modelBuilder.Entity<TblJobMaster>(entity =>
        {
            entity.ToTable("job_master", schema: "fps");
            entity.HasKey(e => e.JobId);
            entity.Property(e => e.JobId).HasColumnName("jobid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobName).HasColumnName("jobname").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Frequency).HasColumnName("frequency").HasMaxLength(50);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(250);
            entity.Property(e => e.TimeToLive).HasColumnName("timetolive").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => e.JobName).IsUnique().HasDatabaseName("job_master_jobname_key");
        });

        // Configure foundation job status table — mirrors fps.job_status
        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity.ToTable("job_status", schema: "fps");
            entity.HasKey(e => e.StatusId);
            entity.Property(e => e.StatusId).HasColumnName("statusid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasIndex(e => new { e.JobId, e.Status }).IsUnique().HasDatabaseName("uq_job_status_jobid_status");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_job_status_jobid")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure foundation job queue table — mirrors fps.job_queue
        modelBuilder.Entity<TblJobQueue>(entity =>
        {
            entity.ToTable("job_queue", schema: "fps");
            entity.HasKey(e => e.JobQueueId);
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobId).HasColumnName("jobid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.StartDateTime).HasColumnName("startdatetime").IsRequired();
            entity.Property(e => e.EndDateTime).HasColumnName("enddatetime");
            entity.Property(e => e.ErrorMessage).HasColumnName("errormessage").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobId)
                  .HasConstraintName("fk_job_queue_jobid")
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_job_queue_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure foundation job queue log table — mirrors fps.job_queue_log
        modelBuilder.Entity<TblJobQueueLog>(entity =>
        {
            entity.ToTable("job_queue_log", schema: "fps");
            entity.HasKey(e => e.JobQueueLogId);
            entity.Property(e => e.JobQueueLogId).HasColumnName("jobqueuelogid").UseIdentityAlwaysColumn();
            entity.Property(e => e.JobQueueId).HasColumnName("jobqueueid").IsRequired();
            entity.Property(e => e.StatusId).HasColumnName("statusid").IsRequired();
            entity.Property(e => e.PerformedBy).HasColumnName("performedby").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LogTime).HasColumnName("logtime").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(500);
            entity.HasOne<TblJobQueue>()
                  .WithMany()
                  .HasForeignKey(e => e.JobQueueId)
                  .HasConstraintName("fk_job_queue_log_jobqueueid")
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<TblJobStatus>()
                  .WithMany()
                  .HasForeignKey(e => e.StatusId)
                  .HasConstraintName("fk_job_queue_log_statusid")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure scheduled load run table — mirrors fps.scheduled_load_run
        modelBuilder.Entity<ScheduledLoadRunTable>(entity =>
        {
            entity.ToTable("scheduled_load_run", schema: "fps");
            entity.HasKey(e => e.RunId).HasName("pk_scheduled_load_run");
            entity.Property(e => e.RunId).HasColumnName("run_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.JobName).HasColumnName("job_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.FpsYear).HasColumnName("fps_year").IsRequired();
            entity.Property(e => e.JobStartedAt).HasColumnName("job_started_at").IsRequired();
            entity.Property(e => e.JobCompletedAt).HasColumnName("job_completed_at");
            entity.Property(e => e.FinalStatus).HasColumnName("final_status").HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasColumnName("correlation_id").IsRequired().HasMaxLength(64);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.JobName, e.FpsYear }).HasDatabaseName("idx_scheduled_load_run_job_fps_year");
            entity.HasIndex(e => e.CorrelationId).HasDatabaseName("idx_scheduled_load_run_correlation_id");

            entity.HasOne<TblJobMaster>()
                  .WithMany()
                  .HasForeignKey(e => e.JobName)
                  .HasPrincipalKey(e => e.JobName)
                  .HasConstraintName("fk_scheduled_load_run_jobname")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure scheduled load step run table — mirrors fps.scheduled_load_step_run
        modelBuilder.Entity<ScheduledLoadStepRunTable>(entity =>
        {
            entity.ToTable("scheduled_load_step_run", schema: "fps");
            entity.HasKey(e => e.StepRunId).HasName("pk_scheduled_load_step_run");
            entity.Property(e => e.StepRunId).HasColumnName("step_run_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.StepName).HasColumnName("step_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.StepSequence).HasColumnName("step_sequence").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.StepStatus).HasColumnName("step_status").IsRequired().HasMaxLength(50);
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(e => e.RowsAffected).HasColumnName("rows_affected");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.RunId).HasDatabaseName("idx_scheduled_load_step_run_run_id");
            entity.HasIndex(e => e.StepStatus).HasDatabaseName("idx_scheduled_load_step_run_status");

            entity.HasOne<ScheduledLoadRunTable>()
                  .WithMany()
                  .HasForeignKey(e => e.RunId)
                  .HasConstraintName("fk_scheduled_load_step_run_run_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure scheduled load validation result table — mirrors fps.scheduled_load_validation_result
        modelBuilder.Entity<ScheduledLoadValidationResultTable>(entity =>
        {
            entity.ToTable("scheduled_load_validation_result", schema: "fps");
            entity.HasKey(e => e.ValidationId).HasName("pk_scheduled_load_validation_result");
            entity.Property(e => e.ValidationId).HasColumnName("validation_id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.RunId).HasColumnName("run_id").IsRequired();
            entity.Property(e => e.AssertionCode).HasColumnName("assertion_code").IsRequired().HasMaxLength(50);
            entity.Property(e => e.AssertionDescription).HasColumnName("assertion_description").IsRequired().HasMaxLength(500);
            entity.Property(e => e.ExpectedValue).HasColumnName("expected_value").HasColumnType("numeric(18,2)");
            entity.Property(e => e.ActualValue).HasColumnName("actual_value").HasColumnType("numeric(18,2)");
            entity.Property(e => e.Passed).HasColumnName("passed").IsRequired();
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message").HasMaxLength(500);
            entity.Property(e => e.CheckedAt).HasColumnName("checked_at").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => new { e.RunId, e.Passed }).HasDatabaseName("idx_scheduled_load_validation_run_passed");
            entity.HasIndex(e => e.AssertionCode).HasDatabaseName("idx_scheduled_load_validation_assertion_code");
            entity.HasIndex(e => new { e.RunId, e.AssertionCode })
                  .IsUnique()
                  .HasDatabaseName("uq_scheduled_load_validation_run_assertion");

            entity.HasOne<ScheduledLoadRunTable>()
                  .WithMany()
                  .HasForeignKey(e => e.RunId)
                  .HasConstraintName("fk_scheduled_load_validation_result_run_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure fps source fixture table — fps.fps_source_project_year
        modelBuilder.Entity<FpsSourceProjectYearTable>(entity =>
        {
            entity.ToTable("fps_source_project_year", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_source_project_year");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_source_project_year_fps_year");
        });

        // Configure fps year totals table — fps.fps_year_totals
        modelBuilder.Entity<FpsYearTotalsTable>(entity =>
        {
            entity.ToTable("fps_year_totals", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_year_totals");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_year_totals_fps_year");
        });

        // Configure fps year archive table — fps.fps_year_archive
        modelBuilder.Entity<FpsYearArchiveTable>(entity =>
        {
            entity.ToTable("fps_year_archive", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_year_archive");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").IsRequired().HasMaxLength(10);
            entity.Property(e => e.TotalAdditionalCosts).HasColumnName("totaladditionalcosts").HasColumnType("money");
            entity.Property(e => e.TotalAnimalCosts).HasColumnName("totalanimalcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalStaffCosts).HasColumnName("totalstaffcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalTestCosts).HasColumnName("totaltestcosts").HasColumnType("double precision");
            entity.Property(e => e.TotalCosts).HasColumnName("totalcosts").HasColumnType("double precision");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnName("totalincome").HasColumnType("money").IsRequired();
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.RequiredProfit).HasColumnName("requiredprofit").HasColumnType("money");
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").IsRequired().HasMaxLength(50);
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.TotalPayCosts).HasColumnName("totalpaycosts").HasColumnType("double precision");
            entity.Property(e => e.ArchivedAt).HasColumnName("archived_at").IsRequired().HasDefaultValueSql("NOW()");
            entity.Property(e => e.ArchiveReason).HasColumnName("archive_reason").IsRequired().HasMaxLength(100).HasDefaultValue("Before deletion");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_year_archive_fps_year");
            entity.HasIndex(e => e.ArchivedAt).HasDatabaseName("idx_fps_year_archive_archived_at");
        });

        // Configure fps current year project-all table — fps.fps_project_all_current_year
        modelBuilder.Entity<FpsProjectAllCurrentYearTable>(entity =>
        {
            entity.ToTable("fps_project_all_current_year", schema: "fps");
            entity.HasKey(e => new { e.Year, e.ParentProject }).HasName("pk_fps_project_all_current_year");
            entity.Property(e => e.Year).HasColumnName("year").HasColumnType("smallint").IsRequired();
            entity.Property(e => e.ParentProject).HasColumnName("parentproject").IsRequired().HasMaxLength(20);
            entity.Property(e => e.Program).HasColumnName("program").HasMaxLength(10);
            entity.Property(e => e.Customer).HasColumnName("customer").HasMaxLength(50);
            entity.Property(e => e.Manager).HasColumnName("manager").HasMaxLength(50);
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome").HasColumnType("money");
            entity.Property(e => e.CustIncome).HasColumnName("custincome").HasColumnType("money");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy").HasColumnType("money");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit").HasColumnType("money");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current").HasColumnType("money");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus").HasMaxLength(50);
            entity.Property(e => e.DateCreated).HasColumnName("datecreated").HasColumnType("date");
            entity.Property(e => e.FecCost).HasColumnName("feccost").HasColumnType("money");
            entity.Property(e => e.Profit).HasColumnName("profit").HasColumnType("money");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl").HasColumnType("money");
            entity.Property(e => e.CaseworkSub).HasColumnName("caseworksub").HasColumnType("numeric(5,4)");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome").HasColumnType("money");
            entity.Property(e => e.PlanCaseworkDebit).HasColumnName("plancaseworkdebit").HasColumnType("money");
            entity.Property(e => e.Source).HasColumnName("source").HasColumnType("character(5)");
            entity.Property(e => e.Disease).HasColumnName("disease").HasMaxLength(50);
            entity.Property(e => e.Contract).HasColumnName("contract").HasMaxLength(10);
            entity.Property(e => e.Finished).HasColumnName("finished").HasColumnType("smallint");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover").HasColumnType("money");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject").HasColumnType("smallint");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre").HasColumnType("double precision");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode").HasMaxLength(50);
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode").HasMaxLength(50);
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup").HasMaxLength(50);
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode").HasMaxLength(50);
            entity.Property(e => e.RefreshedAt).HasColumnName("refreshed_at").IsRequired().HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Year).HasDatabaseName("idx_fps_project_all_current_year_fps_year");
        });
    }
}
