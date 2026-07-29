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
        private const string RecreateSummariesJobName = "RecreateSummary";

        private readonly IYearEndRepository _yearEndRepository;
        private readonly IFpsSettingRepository _fpsSettingRepository;
        private readonly IMonthHourService _monthHourService;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IMapper _mapper;

        public YearEndService(IYearEndRepository yearEndRepository,
            IFpsSettingRepository fpsSettingRepository,
            IMonthHourService monthHourService,
            IEventPublisherService eventPublisherService, IMapper mapper)
        {
            _yearEndRepository = yearEndRepository;
            _fpsSettingRepository = fpsSettingRepository;
            _monthHourService = monthHourService;
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

        private async Task<List<BusinessValidationError>> Validation()
        {
            var errors = new List<BusinessValidationError>();

            //var configs = await _fpsSettingRepository.GetYearEndSettingsAsync();
            //bool hasUnplannedOpenConfigs = configs.Any(x => string.Equals(x.FpsYearType, "open", StringComparison.OrdinalIgnoreCase) 
            //|| string.Equals(x.FpsYearType, "new", StringComparison.OrdinalIgnoreCase));

            //if (hasUnplannedOpenConfigs)
            //{
            //    errors.Add(new BusinessValidationError($"Configuration values for the IDs (HoursInDay or CapApprovalReceivedForReset) are missing for the planned year. Please verify and add the required configuration.", "Missing_Config"));
            //}
            //else
            //{
            //    if(configs.Any(x => x.Id == "HoursInDay"))
            //    { 
            //        var value = configs.FirstOrDefault(x => x.Id == "HoursInDay")?.Setting;
            //        bool isValid = !string.IsNullOrWhiteSpace(value) &&
            //            decimal.TryParse(value, out var number) && number < 0;

            //        if(!isValid)
            //        errors.Add(new BusinessValidationError($"Configuration values for the IDs HoursInDay is not valid. Please provide a numeric value.", "Missing_HoursInDay"));
            //    }
            //    if (configs.Any(x => x.Id == "CapApprovalReceivedForReset"))
            //    {
            //        var value = configs.FirstOrDefault(x => x.Id == "CapApprovalReceivedForReset")?.Setting;
            //        bool isValid = string.Equals(value?.Trim().ToLower(), "yes", StringComparison.OrdinalIgnoreCase) 
            //            || string.Equals(value?.Trim().ToLower(), "no", StringComparison.OrdinalIgnoreCase);

            //        if (!isValid)
            //            errors.Add(new BusinessValidationError($"Configuration values for the IDs CapApprovalReceivedForReset is not valid. Please provide 'Yes' or 'No'.", "Missing_CapApprovalReceivedForReset"));
            //    }
            //}

            //var monthConfigs = await _monthHourService.GetYearEndMonthHoursAsync();
            //bool hasUnplannedMonthConfigs = monthConfigs.Any(x => string.Equals(x.FpsYearType, "open", StringComparison.OrdinalIgnoreCase) 
            //|| string.Equals(x.FpsYearType, "new", StringComparison.OrdinalIgnoreCase));

            //if (hasUnplannedMonthConfigs)
            //    errors.Add(new BusinessValidationError($"Month Working days, VID hours and CVL hours are missing for the planned year. Please verify and add for each missing month.", "Missing_Config"));

            //bool hasmissingMissingVal = monthConfigs.Any(x => x.Days < 0 || x.VidHours < 0 || x.CvlHours < 0);

            //if (hasmissingMissingVal)
            //    errors.Add(new BusinessValidationError($"Month Working days, VID hours and CVL hours are missing for the planned year. Please verify and add for each missing month.", "Missing_Config"));

            //var canRun = await _yearEndRepository.CanRunBatchJobAsync(jobName);
            //if (!canRun)
            //{
            //    errors.Add(new BusinessValidationError($"Job '{jobName}' is already running or initiated., Please try after sometime.", "INVALID_Rerun"));
            //}

            //if (errors.Count > 0)
            //    throw new BusinessValidationErrorException(errors);
            return errors;
        }

        public async Task<bool> CanApproveYearEndDataSetupRequestAsync(string jobName)
        {
            return await _yearEndRepository.CanApproveYearEndDataSetupRequestAsync(jobName);
        }

        public async Task<BatchJobEventTriggerDto> TriggerYearEndInitiationJobAsync(int contextyear, string requestedBy, string correlationId)
        {
            int month = 1;
            var errors = new List<BusinessValidationError>();
            
            var note = $"'{RecreateSummariesJobName}' is initiated for {month} - {contextyear}.";

            //if (month < 1 || month > 12)
            //    errors.Add(new BusinessValidationError("Month must be a numeric value between 1 and 12.", "INVALID_MONTH"));

            if (contextyear == 0 || (contextyear < 1900 || contextyear > 9999))
                errors.Add(new BusinessValidationError($"The selected financial year is not valid. If the issue persists, contact support.", "INVALID_ContextYear"));

            if (string.IsNullOrEmpty(requestedBy))
                errors.Add(new BusinessValidationError($"Unable to identify the requester. Please sign in again and retry. If the issue persists, contact support.", "INVALID_User"));


            //var releasePeriods = await _releaseRepository.GetReleasePeriodsAsync();
            bool exists = true;//releasePeriods.Any(p => p.FinalSummariesRun == -1 && p.EndPeriod >= month);

            if (exists)
                errors.Add(new BusinessValidationError($"You cannot rerun a period when a later period has been run.","INVALID_Rerun"));

            var canRun = await _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(RecreateSummariesJobName);
            if (!canRun)
            {
                errors.Add(new BusinessValidationError($"Job '{RecreateSummariesJobName}' is already running or initiated., Please try after sometime.", "INVALID_Rerun"));
            }

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var queued = await _yearEndRepository.EnqueueBatchJobAsync(RecreateSummariesJobName, requestedBy, correlationId, note);

            var eventDetail = BuildReCreateJobEvent(requestedBy, correlationId, month, contextyear);

            var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            result.EventId = eventId; 
            return result;
        }

        private static EventDetail BuildReCreateJobEvent(string requestedBy, string correlationId, int month, int contextYear)
        {
            return new EventDetail
            {
                JobExecutionId = correlationId,
                JobName = RecreateSummariesJobName,
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
