using System;
using System.Threading;
using System.Threading.Tasks;
using AphaBatchJobs.Core.Interfaces;
using AphaBatchJobs.Core.Interfaces.Adhoc;
using AphaBatchJobs.Core.Models;
using Microsoft.Extensions.Logging;

namespace AphaBatchJobs.Application.Adhoc
{
    /// <summary>
    /// Adhoc job implementation that orchestrates the RecreateSummaries workflow.
    /// Executes a 24-step sequential process to recreate project summaries from core data,
    /// including 16 core calculation procedures and 8 email notification procedures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Execution Flow (24 steps total):
    /// 1. Delete existing casework details
    /// 2. Process restriction expirations
    /// 3-8. Create activity records from various source tables
    /// 9-15. Create/update financial cost calculations
    /// 16. Create month account codes
    /// 17-24. Send email notifications to affected parties
    /// </para>
    /// <para>
    /// Each step has a 300-second timeout. If any step fails, execution halts immediately
    /// and returns appropriate exit code. All operations are logged with correlation IDs
    /// for distributed tracing and debugging.
    /// </para>
    /// </remarks>
    public sealed class AdhocRecreateSummariesJob : IAdhocJob
    {
        private readonly ILogger<AdhocRecreateSummariesJob> _logger;
        private readonly ICorrelationIdService _correlationIdService;
        
        // Core service dependencies
        private readonly IDeleteMonthImportDetailsService _deleteMonthImportDetails;
        private readonly IRestrictionExpiredService _restrictionExpired;
        private readonly ICreateActivityRestrictionDetailService _createActivityRestrictionDetail;
        private readonly IJoinedOnDeleteService _joinedOnDelete;
        private readonly ICreateFromEmpHireService _createFromEmpHire;
        private readonly ICreateActivityEmpHireService _createActivityEmpHire;
        private readonly IChangeOfStatusDeleteService _changeOfStatusDelete;
        private readonly ICreateActivityChangeOfStatusService _createActivityChangeOfStatus;
        private readonly ICreateActivityEmpLeftDateService _createActivityEmpLeftDate;
        private readonly ICreateProjectMonthCaseworkService _createProjectMonthCasework;
        private readonly ICreateTimeCostCalcsService _createTimeCostCalcs;
        private readonly IDeleteEmpMonthTimeDetailsService _deleteEmpMonthTimeDetails;
        private readonly ICreateActivityEmpMonthTimeService _createActivityEmpMonthTime;
        private readonly IDeleteMonthImportTimingsService _deleteMonthImportTimings;
        private readonly ICreateActivityMonthImportTimingService _createActivityMonthImportTiming;
        private readonly ICreateMonthAccountCodeService _createMonthAccountCode;
        
        // Email service dependencies
        private readonly IEmailEmpHireService _emailEmpHire;
        private readonly IEmailJoinedOnService _emailJoinedOn;
        private readonly IEmailChangeOfStatusService _emailChangeOfStatus;
        private readonly IEmailLeftDateService _emailLeftDate;
        private readonly IEmailRestrictionService _emailRestriction;
        private readonly IEmailExpiredRestrictionService _emailExpiredRestriction;
        private readonly IEmailImportSummaryService _emailImportSummary;
        private readonly IEmailProbationSummaryService _emailProbationSummary;
        
        private const int StepTimeoutSeconds = 300;

        /// <summary>
        /// Initializes a new instance of the AdhocRecreateSummariesJob class.
        /// </summary>
        public AdhocRecreateSummariesJob(
            ILogger<AdhocRecreateSummariesJob> logger,
            ICorrelationIdService correlationIdService,
            IDeleteMonthImportDetailsService deleteMonthImportDetails,
            IRestrictionExpiredService restrictionExpired,
            ICreateActivityRestrictionDetailService createActivityRestrictionDetail,
            IJoinedOnDeleteService joinedOnDelete,
            ICreateFromEmpHireService createFromEmpHire,
            ICreateActivityEmpHireService createActivityEmpHire,
            IChangeOfStatusDeleteService changeOfStatusDelete,
            ICreateActivityChangeOfStatusService createActivityChangeOfStatus,
            ICreateActivityEmpLeftDateService createActivityEmpLeftDate,
            ICreateProjectMonthCaseworkService createProjectMonthCasework,
            ICreateTimeCostCalcsService createTimeCostCalcs,
            IDeleteEmpMonthTimeDetailsService deleteEmpMonthTimeDetails,
            ICreateActivityEmpMonthTimeService createActivityEmpMonthTime,
            IDeleteMonthImportTimingsService deleteMonthImportTimings,
            ICreateActivityMonthImportTimingService createActivityMonthImportTiming,
            ICreateMonthAccountCodeService createMonthAccountCode,
            IEmailEmpHireService emailEmpHire,
            IEmailJoinedOnService emailJoinedOn,
            IEmailChangeOfStatusService emailChangeOfStatus,
            IEmailLeftDateService emailLeftDate,
            IEmailRestrictionService emailRestriction,
            IEmailExpiredRestrictionService emailExpiredRestriction,
            IEmailImportSummaryService emailImportSummary,
            IEmailProbationSummaryService emailProbationSummary)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationIdService = correlationIdService ?? throw new ArgumentNullException(nameof(correlationIdService));
            
            _deleteMonthImportDetails = deleteMonthImportDetails ?? throw new ArgumentNullException(nameof(deleteMonthImportDetails));
            _restrictionExpired = restrictionExpired ?? throw new ArgumentNullException(nameof(restrictionExpired));
            _createActivityRestrictionDetail = createActivityRestrictionDetail ?? throw new ArgumentNullException(nameof(createActivityRestrictionDetail));
            _joinedOnDelete = joinedOnDelete ?? throw new ArgumentNullException(nameof(joinedOnDelete));
            _createFromEmpHire = createFromEmpHire ?? throw new ArgumentNullException(nameof(createFromEmpHire));
            _createActivityEmpHire = createActivityEmpHire ?? throw new ArgumentNullException(nameof(createActivityEmpHire));
            _changeOfStatusDelete = changeOfStatusDelete ?? throw new ArgumentNullException(nameof(changeOfStatusDelete));
            _createActivityChangeOfStatus = createActivityChangeOfStatus ?? throw new ArgumentNullException(nameof(createActivityChangeOfStatus));
            _createActivityEmpLeftDate = createActivityEmpLeftDate ?? throw new ArgumentNullException(nameof(createActivityEmpLeftDate));
            _createProjectMonthCasework = createProjectMonthCasework ?? throw new ArgumentNullException(nameof(createProjectMonthCasework));
            _createTimeCostCalcs = createTimeCostCalcs ?? throw new ArgumentNullException(nameof(createTimeCostCalcs));
            _deleteEmpMonthTimeDetails = deleteEmpMonthTimeDetails ?? throw new ArgumentNullException(nameof(deleteEmpMonthTimeDetails));
            _createActivityEmpMonthTime = createActivityEmpMonthTime ?? throw new ArgumentNullException(nameof(createActivityEmpMonthTime));
            _deleteMonthImportTimings = deleteMonthImportTimings ?? throw new ArgumentNullException(nameof(deleteMonthImportTimings));
            _createActivityMonthImportTiming = createActivityMonthImportTiming ?? throw new ArgumentNullException(nameof(createActivityMonthImportTiming));
            _createMonthAccountCode = createMonthAccountCode ?? throw new ArgumentNullException(nameof(createMonthAccountCode));
            
            _emailEmpHire = emailEmpHire ?? throw new ArgumentNullException(nameof(emailEmpHire));
            _emailJoinedOn = emailJoinedOn ?? throw new ArgumentNullException(nameof(emailJoinedOn));
            _emailChangeOfStatus = emailChangeOfStatus ?? throw new ArgumentNullException(nameof(emailChangeOfStatus));
            _emailLeftDate = emailLeftDate ?? throw new ArgumentNullException(nameof(emailLeftDate));
            _emailRestriction = emailRestriction ?? throw new ArgumentNullException(nameof(emailRestriction));
            _emailExpiredRestriction = emailExpiredRestriction ?? throw new ArgumentNullException(nameof(emailExpiredRestriction));
            _emailImportSummary = emailImportSummary ?? throw new ArgumentNullException(nameof(emailImportSummary));
            _emailProbationSummary = emailProbationSummary ?? throw new ArgumentNullException(nameof(emailProbationSummary));
        }

        /// <summary>
        /// Executes the RecreateSummaries orchestration job asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
        /// <returns>JobExecutionResult with status, message, and exit code.</returns>
        public async Task<JobExecutionResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var correlationId = _correlationIdService.GetCorrelationId() ?? Guid.NewGuid().ToString();
            _correlationIdService.SetCorrelationId(correlationId);

            _logger.LogInformation(
                "Starting RecreateSummaries adhoc job execution. CorrelationId: {CorrelationId}",
                correlationId);

            var startTime = DateTime.UtcNow;

            try
            {
                // Step 1: Delete month import details
                var result = await ExecuteStepWithTimeoutAsync(
                    1,
                    "DeleteMonthImportDetails",
                    async (ct) => await ExecuteStep1_DeleteMonthImportDetailsAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 1 (DeleteMonthImportDetails) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 1: DeleteMonthImportDetails - {result.ErrorMessage}");
                }

                // Step 2: Process restriction expirations
                result = await ExecuteStepWithTimeoutAsync(
                    2,
                    "RestrictionExpired",
                    async (ct) => await ExecuteStep2_RestrictionExpiredAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 2 (RestrictionExpired) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 2: RestrictionExpired - {result.ErrorMessage}");
                }

                // Step 3: Create activity restriction detail records
                result = await ExecuteStepWithTimeoutAsync(
                    3,
                    "CreateActivityRestrictionDetail",
                    async (ct) => await ExecuteStep3_CreateActivityRestrictionDetailAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 3 (CreateActivityRestrictionDetail) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 3: CreateActivityRestrictionDetail - {result.ErrorMessage}");
                }

                // Step 4: Process joined-on deletions
                result = await ExecuteStepWithTimeoutAsync(
                    4,
                    "JoinedOnDelete",
                    async (ct) => await ExecuteStep4_JoinedOnDeleteAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 4 (JoinedOnDelete) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 4: JoinedOnDelete - {result.ErrorMessage}");
                }

                // Step 5: Create employee hire records
                result = await ExecuteStepWithTimeoutAsync(
                    5,
                    "CreateFromEmpHire",
                    async (ct) => await ExecuteStep5_CreateFromEmpHireAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 5 (CreateFromEmpHire) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 5: CreateFromEmpHire - {result.ErrorMessage}");
                }

                // Step 6: Create activity employee hire records
                result = await ExecuteStepWithTimeoutAsync(
                    6,
                    "CreateActivityEmpHire",
                    async (ct) => await ExecuteStep6_CreateActivityEmpHireAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 6 (CreateActivityEmpHire) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 6: CreateActivityEmpHire - {result.ErrorMessage}");
                }

                // Step 7: Process status deletions
                result = await ExecuteStepWithTimeoutAsync(
                    7,
                    "ChangeOfStatusDelete",
                    async (ct) => await ExecuteStep7_ChangeOfStatusDeleteAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 7 (ChangeOfStatusDelete) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 7: ChangeOfStatusDelete - {result.ErrorMessage}");
                }

                // Step 8: Create activity status change records
                result = await ExecuteStepWithTimeoutAsync(
                    8,
                    "CreateActivityChangeOfStatus",
                    async (ct) => await ExecuteStep8_CreateActivityChangeOfStatusAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 8 (CreateActivityChangeOfStatus) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 8: CreateActivityChangeOfStatus - {result.ErrorMessage}");
                }

                // Step 9: Create activity employee left date records
                result = await ExecuteStepWithTimeoutAsync(
                    9,
                    "CreateActivityEmpLeftDate",
                    async (ct) => await ExecuteStep9_CreateActivityEmpLeftDateAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 9 (CreateActivityEmpLeftDate) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 9: CreateActivityEmpLeftDate - {result.ErrorMessage}");
                }

                // Step 10: Create project month casework (entry point for financial calculations)
                result = await ExecuteStepWithTimeoutAsync(
                    10,
                    "CreateProjectMonthCasework",
                    async (ct) => await ExecuteStep10_CreateProjectMonthCaseworkAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 10 (CreateProjectMonthCasework) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 10: CreateProjectMonthCasework - {result.ErrorMessage}");
                }

                // Step 11: Create time cost calculations
                result = await ExecuteStepWithTimeoutAsync(
                    11,
                    "CreateTimeCostCalcs",
                    async (ct) => await ExecuteStep11_CreateTimeCostCalcsAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 11 (CreateTimeCostCalcs) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 11: CreateTimeCostCalcs - {result.ErrorMessage}");
                }

                // Step 12: Delete employee monthly time details
                result = await ExecuteStepWithTimeoutAsync(
                    12,
                    "DeleteEmpMonthTimeDetails",
                    async (ct) => await ExecuteStep12_DeleteEmpMonthTimeDetailsAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 12 (DeleteEmpMonthTimeDetails) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 12: DeleteEmpMonthTimeDetails - {result.ErrorMessage}");
                }

                // Step 13: Create activity employee monthly time records
                result = await ExecuteStepWithTimeoutAsync(
                    13,
                    "CreateActivityEmpMonthTime",
                    async (ct) => await ExecuteStep13_CreateActivityEmpMonthTimeAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 13 (CreateActivityEmpMonthTime) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 13: CreateActivityEmpMonthTime - {result.ErrorMessage}");
                }

                // Step 14: Delete month import timings
                result = await ExecuteStepWithTimeoutAsync(
                    14,
                    "DeleteMonthImportTimings",
                    async (ct) => await ExecuteStep14_DeleteMonthImportTimingsAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 14 (DeleteMonthImportTimings) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 14: DeleteMonthImportTimings - {result.ErrorMessage}");
                }

                // Step 15: Create activity month import timing records
                result = await ExecuteStepWithTimeoutAsync(
                    15,
                    "CreateActivityMonthImportTiming",
                    async (ct) => await ExecuteStep15_CreateActivityMonthImportTimingAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 15 (CreateActivityMonthImportTiming) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 15: CreateActivityMonthImportTiming - {result.ErrorMessage}");
                }

                // Step 16: Create month account codes (exit point for main workflow)
                result = await ExecuteStepWithTimeoutAsync(
                    16,
                    "CreateMonthAccountCode",
                    async (ct) => await ExecuteStep16_CreateMonthAccountCodeAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    return result.IsTimeout
                        ? JobExecutionResult.Timeout($"Step 16 (CreateMonthAccountCode) exceeded timeout of {StepTimeoutSeconds} seconds")
                        : JobExecutionResult.Failure($"Failed at step 16: CreateMonthAccountCode - {result.ErrorMessage}");
                }

                // Steps 17-24: Email notifications
                result = await ExecuteStepWithTimeoutAsync(
                    17,
                    "EmailEmpHire",
                    async (ct) => await ExecuteStep17_EmailEmpHireAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 17 (EmailEmpHire) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                    // Continue on email failures
                }

                result = await ExecuteStepWithTimeoutAsync(
                    18,
                    "EmailJoinedOn",
                    async (ct) => await ExecuteStep18_EmailJoinedOnAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 18 (EmailJoinedOn) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    19,
                    "EmailChangeOfStatus",
                    async (ct) => await ExecuteStep19_EmailChangeOfStatusAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 19 (EmailChangeOfStatus) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    20,
                    "EmailLeftDate",
                    async (ct) => await ExecuteStep20_EmailLeftDateAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 20 (EmailLeftDate) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    21,
                    "EmailRestriction",
                    async (ct) => await ExecuteStep21_EmailRestrictionAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 21 (EmailRestriction) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    22,
                    "EmailExpiredRestriction",
                    async (ct) => await ExecuteStep22_EmailExpiredRestrictionAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 22 (EmailExpiredRestriction) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    23,
                    "EmailImportSummary",
                    async (ct) => await ExecuteStep23_EmailImportSummaryAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 23 (EmailImportSummary) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                result = await ExecuteStepWithTimeoutAsync(
                    24,
                    "EmailProbationSummary",
                    async (ct) => await ExecuteStep24_EmailProbationSummaryAsync(ct),
                    cancellationToken);

                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Step 24 (EmailProbationSummary) failed but job continues. Error: {Error}, CorrelationId: {CorrelationId}",
                        result.ErrorMessage,
                        correlationId);
                }

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "RecreateSummaries adhoc job completed. Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                    duration.TotalMilliseconds,
                    correlationId);

                return JobExecutionResult.Success("All 24 steps completed successfully or with non-critical email failures");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "RecreateSummaries job execution cancelled. CorrelationId: {CorrelationId}",
                    correlationId);
                return JobExecutionResult.Timeout("Job execution was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "RecreateSummaries job execution failed with unexpected error. CorrelationId: {CorrelationId}",
                    correlationId);
                return JobExecutionResult.Failure($"Job failed with unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a single step with timeout and exception handling.
        /// </summary>
        private async Task<StepResult> ExecuteStepWithTimeoutAsync(
            int stepNumber,
            string stepName,
            Func<CancellationToken, Task<bool>> stepAction,
            CancellationToken cancellationToken)
        {
            var correlationId = _correlationIdService.GetCorrelationId();
            var stepStartTime = DateTime.UtcNow;

            _logger.LogInformation(
                "Step {StepNumber} ({StepName}) starting. CorrelationId: {CorrelationId}",
                stepNumber,
                stepName,
                correlationId);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(StepTimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                var success = await stepAction(linkedCts.Token);
                var duration = DateTime.UtcNow - stepStartTime;

                if (success)
                {
                    _logger.LogInformation(
                        "Step {StepNumber} ({StepName}) completed successfully. Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        stepNumber,
                        stepName,
                        duration.TotalMilliseconds,
                        correlationId);
                    return StepResult.SuccessResult();
                }
                else
                {
                    _logger.LogError(
                        "Step {StepNumber} ({StepName}) failed. Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                        stepNumber,
                        stepName,
                        duration.TotalMilliseconds,
                        correlationId);
                    return StepResult.FailureResult($"Step {stepNumber} ({stepName}) returned false");
                }
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                _logger.LogError(
                    "Step {StepNumber} ({StepName}) exceeded timeout of {Timeout} seconds. CorrelationId: {CorrelationId}",
                    stepNumber,
                    stepName,
                    StepTimeoutSeconds,
                    correlationId);
                return StepResult.TimeoutResult();
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - stepStartTime;
                _logger.LogError(
                    ex,
                    "Step {StepNumber} ({StepName}) failed with exception. Duration: {Duration}ms, CorrelationId: {CorrelationId}",
                    stepNumber,
                    stepName,
                    duration.TotalMilliseconds,
                    correlationId);
                return StepResult.FailureResult(ex.Message);
            }
        }

        #region Step Execution Methods (24 procedure orchestration)

        private async Task<bool> ExecuteStep1_DeleteMonthImportDetailsAsync(CancellationToken cancellationToken) => await _deleteMonthImportDetails.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep2_RestrictionExpiredAsync(CancellationToken cancellationToken) => await _restrictionExpired.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep3_CreateActivityRestrictionDetailAsync(CancellationToken cancellationToken) => await _createActivityRestrictionDetail.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep4_JoinedOnDeleteAsync(CancellationToken cancellationToken) => await _joinedOnDelete.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep5_CreateFromEmpHireAsync(CancellationToken cancellationToken) => await _createFromEmpHire.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep6_CreateActivityEmpHireAsync(CancellationToken cancellationToken) => await _createActivityEmpHire.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep7_ChangeOfStatusDeleteAsync(CancellationToken cancellationToken) => await _changeOfStatusDelete.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep8_CreateActivityChangeOfStatusAsync(CancellationToken cancellationToken) => await _createActivityChangeOfStatus.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep9_CreateActivityEmpLeftDateAsync(CancellationToken cancellationToken) => await _createActivityEmpLeftDate.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep10_CreateProjectMonthCaseworkAsync(CancellationToken cancellationToken) => await _createProjectMonthCasework.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep11_CreateTimeCostCalcsAsync(CancellationToken cancellationToken) => await _createTimeCostCalcs.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep12_DeleteEmpMonthTimeDetailsAsync(CancellationToken cancellationToken) => await _deleteEmpMonthTimeDetails.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep13_CreateActivityEmpMonthTimeAsync(CancellationToken cancellationToken) => await _createActivityEmpMonthTime.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep14_DeleteMonthImportTimingsAsync(CancellationToken cancellationToken) => await _deleteMonthImportTimings.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep15_CreateActivityMonthImportTimingAsync(CancellationToken cancellationToken) => await _createActivityMonthImportTiming.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep16_CreateMonthAccountCodeAsync(CancellationToken cancellationToken) => await _createMonthAccountCode.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep17_EmailEmpHireAsync(CancellationToken cancellationToken) => await _emailEmpHire.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep18_EmailJoinedOnAsync(CancellationToken cancellationToken) => await _emailJoinedOn.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep19_EmailChangeOfStatusAsync(CancellationToken cancellationToken) => await _emailChangeOfStatus.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep20_EmailLeftDateAsync(CancellationToken cancellationToken) => await _emailLeftDate.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep21_EmailRestrictionAsync(CancellationToken cancellationToken) => await _emailRestriction.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep22_EmailExpiredRestrictionAsync(CancellationToken cancellationToken) => await _emailExpiredRestriction.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep23_EmailImportSummaryAsync(CancellationToken cancellationToken) => await _emailImportSummary.ExecuteAsync(cancellationToken);
        private async Task<bool> ExecuteStep24_EmailProbationSummaryAsync(CancellationToken cancellationToken) => await _emailProbationSummary.ExecuteAsync(cancellationToken);

        #endregion

        /// <summary>
        /// Internal helper class for step execution results.
        /// </summary>
        private class StepResult
        {
            public bool Success { get; set; }
            public bool IsTimeout { get; set; }
            public string ErrorMessage { get; set; }

            public static StepResult SuccessResult() => new StepResult { Success = true };
            public static StepResult FailureResult(string errorMessage) => new StepResult { Success = false, ErrorMessage = errorMessage };
            public static StepResult TimeoutResult() => new StepResult { Success = false, IsTimeout = true };
        }
    }
}
