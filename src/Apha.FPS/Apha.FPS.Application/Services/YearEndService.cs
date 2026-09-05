using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Apha.FPS.Application.Services
{
    public class YearEndService : IYearEndService
    {
        private const string YearEndDataSetupJobName = "YearEnd-DataSetup";
        private const string YearEndCutOverJobName = "YearEnd-CutOver";

        private readonly IYearEndRepository _yearEndRepository;
        private readonly IFpsSettingRepository _fpsSettingRepository;
        private readonly IMonthHourRepository _monthHourRepository;
        private readonly IYearMasterRepository _yearMasterRepository;
        private readonly IYearEndStagingRepository _yearEndStagingRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IGraphEmailService _emailService;
        private readonly ILogger<YearEndService> _logger;
        private readonly IMapper _mapper;
        private readonly YearEndEmailSettings _emailSettings;

        public YearEndService(IYearEndRepository yearEndRepository,
            IFpsSettingRepository fpsSettingRepository,
            IMonthHourRepository monthHourRepository,
            IYearMasterRepository yearMasterRepository,
            IYearEndStagingRepository yearEndStagingRepository,
            IEventPublisherService eventPublisherService,
            IGraphEmailService emailService,
            IOptions<YearEndEmailSettings> emailSettings,
            ILogger<YearEndService> logger,
            IMapper mapper)
        {
            _yearEndRepository = yearEndRepository;
            _fpsSettingRepository = fpsSettingRepository;
            _monthHourRepository = monthHourRepository;
            _yearMasterRepository = yearMasterRepository;
            _yearEndStagingRepository = yearEndStagingRepository;
            _eventPublisherService = eventPublisherService;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
            _emailSettings = emailSettings.Value;
        }

        public async Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var history = await _yearEndRepository.GetBatchJobsHistoryAsync(filter, jobName);
            return _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(history);
        }

        public async Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(jobName);
        }

        public async Task<bool> CanApproveOrRejectYearEndDataSetupRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(jobName);
        }

        public async Task<Guid?> GetInitiatedDataSetupJobExecutionIdAsync()
        {
            return await _yearEndRepository.GetInitiatedDataSetupJobExecutionIdAsync();
        }

        public async Task<BatchJobQueueDto> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear, int contextYear, 
            string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndDataSetupJobName;

            var note = $"'{jobName}' is initiated for {plannedYear}.";

            // Deliberately no ValidateConfiguration here (planned-year staging design,
            // 2026-09-03) - Initiate no longer requires Config Value/Month Hours to already exist.
            // That requirement moves to Approve, which validates staged rows instead of the real
            // tables. See fps-year-end-planned-year-staging-design-2026-09-03.md decision-4/8.
            await ValidateDataSetupRequestInput(plannedYear, requestedBy, errors, jobName, false);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(jobName, requestedBy, correlationId, note, plannedYear);

            try
            {
                await SendEmailAsync(jobName, "Initiation", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End initiation notification email.");
            }

            return _mapper.Map<BatchJobQueueDto>(queued);
        }

        public async Task<BatchJobEventTriggerDto> EnqueueYearEndDataSetupApprovalJobAsync(Guid jobExecutionId, int plannedYear, int contextYear,
            string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndDataSetupJobName;

            // Planned-year staging design (Workstream 6): resolve the exact request this Approve call is
            // for - never "whichever request happens to be Initiated". An unresolvable JobExecutionId is
            // a wiring failure (matches Confirm's Workstream 5 convention), not a business validation
            // error, so it's left uncaught here.
            var request = await _yearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                ?? throw new KeyNotFoundException($"Year End Data Setup request '{jobExecutionId}' was not found.");

            if (!string.Equals(request.Status, "Initiated", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        $"This request is no longer editable (status: {request.Status}).", "REQUEST_NOT_EDITABLE")
                ]);
            }

            if (!request.TargetFpsYear.HasValue)
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        $"Request '{jobExecutionId}' has no target year set and cannot be approved.", "TARGET_FPSYEAR_MISSING")
                ]);
            }

            // Once the exact request is resolved, a caller-supplied plannedYear that disagrees with the
            // persisted target year is stale/wrong - fail here, before running any other validation
            // against it, rather than silently using the persisted value and hoping the caller notices.
            if (plannedYear != request.TargetFpsYear.Value)
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        $"The requested planned year ({plannedYear}) does not match this request's target year ({request.TargetFpsYear.Value}).", "PLANNEDYEAR_MISMATCH")
                ]);
            }

            var note = $"'{jobName}' is approved for {request.TargetFpsYear.Value}.";

            await ValidateDataSetupRequestInput(plannedYear, requestedBy, errors, jobName, true);
            await ValidateConfiguration(errors, request);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            BatchJobQueue queued;
            try
            {
                queued = await _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(request.JobQueueId, requestedBy, note);
            }
            catch (KeyNotFoundException)
            {
                // The row was Initiated when resolved above but no longer is by the time the repository
                // re-checked at write time - someone else approved/rejected it in between. Same
                // caller-visible error as the earlier, more common check.
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        "This request is no longer editable - it was modified by another request in the meantime.", "REQUEST_NOT_EDITABLE")
                ]);
            }

            // Row's own persisted JobExecutionId, not the raw correlationId - Initiate and Approve
            // are separate requests that never share a correlation id. Persisted target year, not the
            // caller-supplied plannedYear (already verified equal above, but the persisted value is the
            // one that must drive the Worker regardless).
            // TEMP: EventBridge disabled for local testing (revert before pushing).
            // var eventDetail = BuildYearEndJobEvent(jobName, requestedBy, queued.JobExecutionId.ToString(), request.TargetFpsYear.Value);
            // var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);
            // await _yearEndRepository.SetTriggeredMetadataAsync(queued.JobExecutionId.ToString(), requestedBy);
            var eventId = string.Empty;

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            result.EventId = eventId;

            try
            {
                await SendEmailAsync(jobName, "Approval", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End approval notification email.");
            }

            return _mapper.Map<BatchJobEventTriggerDto>(result);
        }

        public async Task<bool> EnqueueYearEndDataSetupRejectJobAsync(Guid jobExecutionId, int plannedYear, int contextYear,
            string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndDataSetupJobName;

            // Resolve the exact request, same as Approve. Deliberately no TargetFpsYear/plannedYear
            // checks here (unlike Approve) - Reject must remain usable to clean up a malformed/legacy
            // request even if TargetFpsYear is null or the caller's plannedYear is stale; the exact
            // JobExecutionId already identifies the target unambiguously, and Reject builds no trigger.
            var request = await _yearEndStagingRepository.ResolveRequestAsync(jobExecutionId)
                ?? throw new KeyNotFoundException($"Year End Data Setup request '{jobExecutionId}' was not found.");

            if (!string.Equals(request.Status, "Initiated", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        $"This request is no longer editable (status: {request.Status}).", "REQUEST_NOT_EDITABLE")
                ]);
            }

            var note = $"'{jobName}' is rejected for {request.TargetFpsYear?.ToString() ?? "unknown"}.";

            if (string.IsNullOrEmpty(requestedBy))
                errors.Add(new BusinessValidationError($"Unable to identify the rejector. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));

            var canApprove = await _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(jobName);
            if (!canApprove)
                errors.Add(new BusinessValidationError($"There is no initiated request for rejection for job '{jobName}'.", "INVALID_Approval"));

            var initiator = await _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(jobName);
            if (!string.IsNullOrEmpty(initiator) && initiator == requestedBy)
                errors.Add(new BusinessValidationError($"Initiator and rejector for job '{jobName}' cannot be the same person. The request was created by '{initiator}'.", "INVALID_Approval"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            BatchJobQueue queued;
            try
            {
                queued = await _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(request.JobQueueId, requestedBy, note);
            }
            catch (KeyNotFoundException)
            {
                // Same narrow-race translation as Approve - the row was Initiated when resolved above
                // but no longer is by the time the repository re-checked at write time.
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        "This request is no longer editable - it was modified by another request in the meantime.", "REQUEST_NOT_EDITABLE")
                ]);
            }

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);

            try
            {
                await SendEmailAsync(jobName, "Rejection", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End rejection notification email.");
            }

            return true;
        }

        public async Task<bool> CanInitiateYearEndCutOverRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(jobName);
        }

        public async Task<bool> CanApproveOrRejectYearEndCutOverRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(jobName);
        }

        public async Task<BatchJobQueueDto> EnqueueYearEndCutOverInitiationJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndCutOverJobName;

            var note = $"'{jobName}' is initiated for {plannedYear}.";

            await ValidateCutOverRequestInput(plannedYear, requestedBy, errors, jobName, false);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueCutOverInitiationBatchJobAsync(jobName, requestedBy, correlationId, note);

            try
            {
                await SendEmailAsync(jobName, "Initiation", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End CutOver notification email.");
            }

            return _mapper.Map<BatchJobQueueDto>(queued);
        }

        public async Task<BatchJobEventTriggerDto> EnqueueYearEndCutOverApprovalJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndCutOverJobName;

            var note = $"'{jobName}' is approved for {plannedYear}.";

            await ValidateCutOverRequestInput(plannedYear, requestedBy, errors, jobName, true);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueCutOverApprovalBatchJobAsync(jobName, requestedBy, correlationId, note);

            // Same JobExecutionId reasoning as the DataSetup approval path above.
            var eventDetail = BuildYearEndJobEvent(jobName, requestedBy, queued.JobExecutionId.ToString(), plannedYear);

            var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);

            await _yearEndRepository.SetTriggeredMetadataAsync(queued.JobExecutionId.ToString(), requestedBy);

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            result.EventId = eventId;

            try
            {
                await SendEmailAsync(jobName, "Approval", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End CutOver approval notification email.");
            }

            return _mapper.Map<BatchJobEventTriggerDto>(result);
        }

        public async Task<bool> EnqueueYearEndCutOverRejectJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var jobName = YearEndCutOverJobName;

            var note = $"'{jobName}' is rejected for {plannedYear}.";

            if (string.IsNullOrEmpty(requestedBy))
                errors.Add(new BusinessValidationError($"Unable to identify the rejector. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));

            var canApprove = await _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(jobName);
            if (!canApprove)
                errors.Add(new BusinessValidationError($"There is no initiated request for rejection for job '{jobName}'.", "INVALID_Approval"));

            var initiator = await _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(jobName);
            if (!string.IsNullOrEmpty(initiator) && initiator == requestedBy)
                errors.Add(new BusinessValidationError($"Initiator and rejector for job '{jobName}' cannot be the same person. The request was created by '{initiator}'.", "INVALID_Approval"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueCutOverRejectBatchJobAsync(jobName, requestedBy, correlationId, note);

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);

            try
            {
                await SendEmailAsync(jobName, "Rejection", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Year End CutOver rejection notification email.");
            }

            return true;
        }
        
        private async Task ValidateDataSetupRequestInput(int plannedYear, string requestedBy, List<BusinessValidationError> errors, string jobName, bool isApprovalRequest)
        {
            if (plannedYear == 0 || (plannedYear < 1900 || plannedYear > 9999))
                errors.Add(new BusinessValidationError($"PlannedYear is not valid.", "INVALID_PlannedYear"));

            if (string.IsNullOrEmpty(requestedBy))
            {
                var userRole = isApprovalRequest ? "approver" : "requester";
                errors.Add(new BusinessValidationError($"Unable to identify the {userRole}. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));
            }

            var plannedYearEntity = await _yearMasterRepository.GetFpsYearByIdAsync(plannedYear);

            if (plannedYearEntity != null)
                errors.Add(new BusinessValidationError($"YearEnd Datasetup already completed for the planned year {plannedYear}. You cannot reinitiate/approve request.", "INVALID_Rerun"));

            if (isApprovalRequest)
            {
                var canApprove = await _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(jobName);
                if (!canApprove)
                    errors.Add(new BusinessValidationError($"There is no initiated request for approval for job '{jobName}'.", "INVALID_Approval"));

                var initiator = await _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(jobName);

                if (!string.IsNullOrEmpty(initiator) && initiator == requestedBy)
                {
                    errors.Add(new BusinessValidationError($"Initiator and approver for job '{jobName}' cannot be the same person. The request was created by '{initiator}'.", "INVALID_Approval"));
                }
            }
            else
            {
                var canRun = await _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(jobName);
                if (!canRun)
                {
                    errors.Add(new BusinessValidationError($"Job '{jobName}' is already running or initiated. Please try after sometime.", "INVALID_Initiation"));
                }
            }
        }

        private async Task ValidateConfiguration(List<BusinessValidationError> errors, YearEndRequestSummary request)
        {
            await ValidateSettingConfiguration(errors, request);
            await ValidateMonthConfiguration(errors, request);
        }

        private async Task ValidateSettingConfiguration(List<BusinessValidationError> errors, YearEndRequestSummary request)
        {
            // Planned-year staging design (Workstream 6): staging-only overlay read - ExistsForPlannedYear
            // is "Yes" only when a staged row exists, never a current-year preview/fallback value, so
            // this correctly validates what was actually Confirmed for this exact request, not the real
            // tblsettings table.
            var configs = await _fpsSettingRepository.GetYearEndSettingsAsync(request);
            bool hasUnplannedOpenConfigs = configs.Any(x => string.Equals(x.ExistsForPlannedYear, "No", StringComparison.OrdinalIgnoreCase));

            if (hasUnplannedOpenConfigs)
            {
                errors.Add(new BusinessValidationError($"Configuration values for the IDs (HoursInDay or CapApprovalReceivedForReset) are missing for the planned year. Please verify and add the required configuration.", "Missing_Config"));
            }
            else
            {
                if (configs.Any(x => x.Id == "HoursInDay"))
                {
                    var value = configs.FirstOrDefault(x => x.Id == "HoursInDay")?.Setting;
                    bool isValid = !string.IsNullOrWhiteSpace(value) &&
                        decimal.TryParse(value, out var number) && number > 0;

                    if (!isValid)
                        errors.Add(new BusinessValidationError($"Configuration values for the IDs HoursInDay is not valid. Please provide a numeric value.", "Missing_HoursInDay"));
                }

                if (configs.Any(x => x.Id == "CapApprovalReceivedForReset"))
                {
                    var value = configs.FirstOrDefault(x => x.Id == "CapApprovalReceivedForReset")?.Setting;
                    bool isValid = string.Equals(value?.Trim().ToLower(), "yes", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(value?.Trim().ToLower(), "no", StringComparison.OrdinalIgnoreCase);

                    if (!isValid)
                        errors.Add(new BusinessValidationError($"Configuration values for the IDs CapApprovalReceivedForReset is not valid. Please provide 'Yes' or 'No'.", "Missing_CapApprovalReceivedForReset"));
                }
            }
        }

        private async Task ValidateMonthConfiguration(List<BusinessValidationError> errors, YearEndRequestSummary request)
        {
            // Same staging-only overlay reasoning as ValidateSettingConfiguration above.
            var monthConfigs = await _monthHourRepository.GetYearEndMonthHoursAsync(request);

            bool hasUnplannedMonthConfigs = monthConfigs.Any(x => string.Equals(x.ExistsForPlannedYear, "No", StringComparison.OrdinalIgnoreCase));

            if (hasUnplannedMonthConfigs)
                errors.Add(new BusinessValidationError($"Month(s) Working days, VID hours and CVL hours are missing for the planned year. Please verify and provide value for each missing month.", "Missing_Config"));

            bool hasmissingMissingVal = monthConfigs.Any(x => x.Days < 0 || x.VidHours < 0 || x.CvlHours < 0);

            if (hasmissingMissingVal)
                errors.Add(new BusinessValidationError($"Provided Month( Working days, VID hours and CVL hours values are not valid for the planned year. Values should be non-negative and greater than zero.  Please verify and provide valid value for each month.", "Missing_Config"));
        }

        private async Task ValidateCutOverRequestInput(int plannedYear, string requestedBy, List<BusinessValidationError> errors, string jobName, bool isApprovalRequest)
        {
            if (plannedYear == 0 || (plannedYear < 1900 || plannedYear > 9999))
                errors.Add(new BusinessValidationError($"PlannedYear is not valid.", "INVALID_PlannedYear"));

            if (string.IsNullOrEmpty(requestedBy))
            {
                var userRole = isApprovalRequest ? "approver" : "requester";
                errors.Add(new BusinessValidationError($"Unable to identify the {userRole}. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));
            }

            var plannedYearEntity = await _yearMasterRepository.GetFpsYearByIdAsync(plannedYear);

            if (plannedYearEntity == null ||
                (plannedYearEntity != null && string.Equals(plannedYearEntity.YearStatus, "open", StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new BusinessValidationError($"YearEnd CutOver already completed and planned year {plannedYear} is live. You cannot reinitiate or approve request.", "INVALID_Rerun"));
            }

            if (isApprovalRequest)
            {
                var canApprove = await _yearEndRepository.CanApproveOrRejectYearEndCutOverRequestAsync(jobName);
                if (!canApprove)
                    errors.Add(new BusinessValidationError($"There is no initiated request for approval for job '{jobName}'.", "INVALID_Approval"));

                var initiator = await _yearEndRepository.GetYearEndCutOverRequestInitiatorAsync(jobName);

                if (!string.IsNullOrEmpty(initiator) && initiator == requestedBy)
                {
                    errors.Add(new BusinessValidationError($"Initiator and approver for job '{jobName}' cannot be the same person. The request was created by '{initiator}'.", "INVALID_Approval"));
                }
            }
            else
            {
                var canRun = await _yearEndRepository.CanInitiateYearEndCutOverRequestAsync(jobName);
                if (!canRun)
                {
                    errors.Add(new BusinessValidationError($"Job '{jobName}' is already running or initiated. Please try after sometime.", "INVALID_Initiation"));
                }
            }
        }
        
        private static EventDetail BuildYearEndJobEvent(string jobName, string requestedBy, string correlationId, int plannedYear)
        {
            return new EventDetail
            {
                JobExecutionId = correlationId,
                JobName = jobName,
                RunMode = "Manual",
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                ParametersJson = JsonSerializer.Serialize(new
                {
                    plannedYear = $"{plannedYear:D4}"
                })
            };
        }
        
        private async Task SendEmailAsync(string jobName, string notificationType, CancellationToken cancellationToken)
        {
            if (string.Equals(jobName, YearEndDataSetupJobName, StringComparison.OrdinalIgnoreCase))
            {
                await SendDataSetupEmailAsyc(notificationType, cancellationToken);
            }
            
            if (string.Equals(jobName, YearEndCutOverJobName, StringComparison.OrdinalIgnoreCase))
            {
                await SendCutOverEmailAsyc(notificationType, cancellationToken);
            }
        }

        private async Task SendDataSetupEmailAsyc(string notificationType, CancellationToken cancellationToken)
        {
            if (notificationType == "Approval")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.DataSetupApprovalEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.DataSetupApprovalEmailSubject,
                    Body = _emailSettings.DataSetupApprovalEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }

            if (notificationType == "Rejection")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.DataSetupRejectionEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.DataSetupRejectionEmailSubject,
                    Body = _emailSettings.DataSetupRejectionEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }

            if (notificationType == "Initiation")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.DataSetupInitiatedEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.DataSetupInitiatedEmailSubject,
                    Body = _emailSettings.DataSetupInitiatedEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }
        }

        private async Task SendCutOverEmailAsyc(string notificationType, CancellationToken cancellationToken)
        {
            if (notificationType == "Approval")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.CutOverApprovalEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.CutOverApprovalEmailSubject,
                    Body = _emailSettings.CutOverApprovalEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }

            if (notificationType == "Rejection")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.CutOverRejectionEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.CutOverRejectionEmailSubject,
                    Body = _emailSettings.CutOverRejectionEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }

            if (notificationType == "Initiation")
            {
                await _emailService.SendEmailAsync(new EmailMessageModel
                {
                    To = _emailSettings.CutOverInitiatedEmailRecipient!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Subject = _emailSettings.CutOverInitiatedEmailSubject,
                    Body = _emailSettings.CutOverInitiatedEmailBody,
                    IsBodyHtml = false,
                }, cancellationToken);
            }
        }
    }
}