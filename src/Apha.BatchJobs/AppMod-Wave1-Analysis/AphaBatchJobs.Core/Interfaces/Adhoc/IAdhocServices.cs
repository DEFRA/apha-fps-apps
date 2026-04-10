using System.Threading;
using System.Threading.Tasks;

namespace AphaBatchJobs.Core.Interfaces.Adhoc
{
    // Core procedure service interfaces (Steps 1-16)
    
    /// <summary>Deletes existing month import details to allow fresh creation.</summary>
    public interface IDeleteMonthImportDetailsService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Processes and removes expired restriction records.</summary>
    public interface IRestrictionExpiredService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity tracking records for restriction detail changes.</summary>
    public interface ICreateActivityRestrictionDetailService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Processes deletions for joined-on status changes.</summary>
    public interface IJoinedOnDeleteService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates data from employee hire records.</summary>
    public interface ICreateFromEmpHireService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity records for employee hire events.</summary>
    public interface ICreateActivityEmpHireService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Processes deletions for employee status changes.</summary>
    public interface IChangeOfStatusDeleteService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity records for employee status changes.</summary>
    public interface ICreateActivityChangeOfStatusService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity records for employee leave dates.</summary>
    public interface ICreateActivityEmpLeftDateService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates project month casework records (entry point for financial data).</summary>
    public interface ICreateProjectMonthCaseworkService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Calculates time-based cost allocations across projects.</summary>
    public interface ICreateTimeCostCalcsService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Deletes existing employee monthly time details.</summary>
    public interface IDeleteEmpMonthTimeDetailsService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity records for employee monthly time entries.</summary>
    public interface ICreateActivityEmpMonthTimeService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Deletes existing month import timing records.</summary>
    public interface IDeleteMonthImportTimingsService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates activity records for month import timings.</summary>
    public interface ICreateActivityMonthImportTimingService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Creates month account code mappings (exit point for main workflow).</summary>
    public interface ICreateMonthAccountCodeService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    // Email notification service interfaces (Steps 17-24)

    /// <summary>Sends email notifications for new employee hires.</summary>
    public interface IEmailEmpHireService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends email notifications for joined-on status changes.</summary>
    public interface IEmailJoinedOnService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends email notifications for employee status changes.</summary>
    public interface IEmailChangeOfStatusService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends email notifications for employee leave dates.</summary>
    public interface IEmailLeftDateService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends email notifications for active restrictions.</summary>
    public interface IEmailRestrictionService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends email notifications for expired restrictions.</summary>
    public interface IEmailExpiredRestrictionService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends summary email for monthly import activity.</summary>
    public interface IEmailImportSummaryService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>Sends summary email for probation-related activities.</summary>
    public interface IEmailProbationSummaryService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
