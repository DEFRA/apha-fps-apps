using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text.Json;

namespace Apha.FPS.Application.Services
{
    public class YearEndService : IYearEndService
    {
        private const string YearEndDataSetupJobName = "YearEnd-DataSetup";

        private readonly IYearEndRepository _yearEndRepository;
        private readonly IFpsSettingRepository _fpsSettingRepository;
        private readonly IMonthHourRepository _monthHourRepository;
        private readonly IYearMasterRepository _yearMasterRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IMapper _mapper;

        public YearEndService(IYearEndRepository yearEndRepository,
            IFpsSettingRepository fpsSettingRepository,
            IMonthHourRepository monthHourRepository,
            IYearMasterRepository yearMasterRepository,
            IEventPublisherService eventPublisherService, IMapper mapper)
        {
            _yearEndRepository = yearEndRepository;
            _fpsSettingRepository = fpsSettingRepository;
            _monthHourRepository = monthHourRepository;
            _yearMasterRepository = yearMasterRepository;
            _eventPublisherService = eventPublisherService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var history = await _yearEndRepository.GetBatchJobsHistoryAsync(filter, jobName);
            return _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(history);
        }

        public async Task<bool> CanInitiateYearEndDataSetupRequestAsync(string jobName)
        {
           // List<BusinessValidationError> errors = await Validation();
            return await _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(jobName);
        }

        

        public async Task<bool> CanApproveYearEndDataSetupRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanApproveYearEndDataSetupRequestAsync(jobName);
        }

        public async Task<BatchJobQueueDto> EnqueueYearEndDataSetupInitiationJobAsync(int plannedYear, int contextyear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();

            var note = $"'{YearEndDataSetupJobName}' is initiated for {plannedYear}.";

            await ValidateConfiguration(errors);
            await ValidateDataSetupInput(plannedYear, requestedBy, errors, YearEndDataSetupJobName, false);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueDataSetupBatchJobAsync(YearEndDataSetupJobName, requestedBy, correlationId, note);

            //var eventDetail = BuildReCreateJobEvent(requestedBy, correlationId, month, contextyear);

            //var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);

            //var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            //result.EventId = eventId; 
            return _mapper.Map<BatchJobQueueDto>(queued);


        }

        public async Task<BatchJobQueueDto> EnqueueYearEndDataSetupApprovalJobAsync(int plannedYear, int contextYear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();

            var note = $"'{YearEndDataSetupJobName}' is approved for {plannedYear}.";
 
            await ValidateConfiguration(errors);
            await ValidateDataSetupInput(plannedYear, requestedBy, errors, YearEndDataSetupJobName, true);

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueApprovedDataSetupBatchJobAsync(YearEndDataSetupJobName, requestedBy, correlationId, note);

            return _mapper.Map<BatchJobQueueDto>(queued);
        }
 
        private async Task ValidateDataSetupInput(int plannedYear, string requestedBy, List<BusinessValidationError> errors, string jobName,bool isApprovalReq)
        {
            if (plannedYear == 0 || (plannedYear < 1900 || plannedYear > 9999))
                errors.Add(new BusinessValidationError($"PlannedYear is not valid.", "INVALID_PlannedYear"));

            if (string.IsNullOrEmpty(requestedBy) && !isApprovalReq)
                errors.Add(new BusinessValidationError($"Unable to identify the requester. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));
            else if (string.IsNullOrEmpty(requestedBy) && isApprovalReq)
                errors.Add(new BusinessValidationError($"Unable to identify the approver. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));

            var plannedYearEntity = await _yearMasterRepository.GetFpsYearByIdAsync(plannedYear);

            if (plannedYearEntity != null)
                //errors.Add(new BusinessValidationError($"YearEnd Datasetup already completed for the planned year {plannedYear}. You cannot reinitiate request.", "INVALID_Rerun"));

            if (jobName != null && jobName == YearEndDataSetupJobName && !isApprovalReq)
            {
                var canRun = await _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(jobName);
                if (!canRun)
                {
                    errors.Add(new BusinessValidationError($"Job '{jobName}' is already running or initiated., Please try after sometime.", "INVALID_Rerun"));
                }
            }
            if (jobName != null && jobName == YearEndDataSetupJobName && isApprovalReq)
            {
                var canApprove = await _yearEndRepository.CanApproveYearEndDataSetupRequestAsync(YearEndDataSetupJobName);
                if (!canApprove)
                    errors.Add(new BusinessValidationError($"There is no initiated request to approve for job '{YearEndDataSetupJobName}'.", "INVALID_Approve"));

                var initiator = await _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(YearEndDataSetupJobName);

                if(!string.IsNullOrEmpty(initiator) && initiator == requestedBy)
                {
                    errors.Add(new BusinessValidationError($"Initiator and approver for job '{YearEndDataSetupJobName}' cannot be the same person. The request was created by '{initiator}'.", "INVALID_Approve"));
                }
            }

            //return errors;
        }
        private async Task ValidateConfiguration(List<BusinessValidationError> errors)
        {
            //var errors = new List<BusinessValidationError>();

            var configs = await _fpsSettingRepository.GetYearEndSettingsAsync();
            bool hasUnplannedOpenConfigs = configs.Any(x => string.Equals(x.FpsYearType, "open", StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.FpsYearType, "new", StringComparison.OrdinalIgnoreCase));

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

            var monthConfigs = await _monthHourRepository.GetYearEndMonthHoursAsync();
            bool hasUnplannedMonthConfigs = monthConfigs.Any(x => string.Equals(x.FpsYearType, "open", StringComparison.OrdinalIgnoreCase)
            || string.Equals(x.FpsYearType, "new", StringComparison.OrdinalIgnoreCase));

            if (hasUnplannedMonthConfigs)
                errors.Add(new BusinessValidationError($"Month Working days, VID hours and CVL hours are missing for the planned year. Please verify and add for each missing month.", "Missing_Config"));

            bool hasmissingMissingVal = monthConfigs.Any(x => x.Days < 0 || x.VidHours < 0 || x.CvlHours < 0);

            if (hasmissingMissingVal)
                errors.Add(new BusinessValidationError($"Provided Month Working days, VID hours and CVL hours values are not valid for the planned year. Values should be non-negative and greater than zero.  Please verify and add valid value for each month.", "Missing_Config"));

           // return errors;
        }
        private static EventDetail BuildReCreateJobEvent(string requestedBy, string correlationId, int month, int contextYear)
        {
            return new EventDetail
            {
                JobExecutionId = correlationId,
                JobName = YearEndDataSetupJobName,
                RunMode = "Manual",
                RequestedBy = requestedBy,
                RequestedAtUtc = DateTime.UtcNow,
                ParametersJson = JsonSerializer.Serialize(new
                {
                    month = $"{contextYear:D4}-{month:D2}"
                })
            };
        }
    }
}
