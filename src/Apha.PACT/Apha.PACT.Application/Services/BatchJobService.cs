using Apha.Common.Utilities.EventPublisher;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using System.Text.Json;

namespace Apha.PACT.Application.Services
{
    public class BatchJobService : IBatchJobService
    {
        private const string RecreateSummariesJobName = "RecreateSummary";

        private readonly IBatchJobRepository _repository;
        private readonly IRecreateAndReleaseSummaryRepository _releaseRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IMapper _mapper;

        public BatchJobService(IBatchJobRepository repository, IRecreateAndReleaseSummaryRepository releaseRepository,
            IEventPublisherService eventPublisherService, IMapper mapper)
        {
            _repository = repository;
            _releaseRepository = releaseRepository;
            _eventPublisherService= eventPublisherService;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<BatchJobHistoryDto>> GetBatchJobsHistoryAsync(QueryParameters<string> query, string jobName)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var history = await _repository.GetBatchJobsHistoryAsync(filter, jobName);
            return _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(history);
        }

        public async Task<bool> CanRunBatchJobAsync(string jobName)
        {
            return await _repository.CanRunBatchJobAsync(jobName);
        }

        public async Task<BatchJobEventTriggerDto> TriggerRecreateSummariesJobAsync(int month, int contextyear, string requestedBy, string correlationId)
        {
            var errors = new List<BusinessValidationError>();
            var note = $"'{RecreateSummariesJobName}' is initiated for {month} - {contextyear}.";


            if (month < 1 || month > 12)
                errors.Add(new BusinessValidationError("Month must be a numeric value between 1 and 12.", "INVALID_MONTH"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            if (contextyear == 0 || (contextyear < 1900 || contextyear > 9999))
                throw new InvalidOperationException($"Provided Context year is not valid.");

            if (string.IsNullOrEmpty(requestedBy))
                throw new InvalidOperationException($"RequestedBy is not required.");


            var releasePeriods = await _releaseRepository.GetReleasePeriodsAsync();
            bool exists = releasePeriods.Any(p => p.FinalSummariesRun == -1 && p.EndPeriod >= month);

            if (exists)
                throw new InvalidOperationException($"You cannot rerun a period when a later period has been run.");

            var canRun = await _repository.CanRunBatchJobAsync(RecreateSummariesJobName);
            if (!canRun)
            {
                throw new InvalidOperationException($"Job '{RecreateSummariesJobName}' is already running.");
            }

            var queued = await _repository.EnqueueBatchJobAsync(RecreateSummariesJobName, requestedBy, correlationId, note);

            var eventDetail = BuildReCreateJobEvent(requestedBy, correlationId, month, contextyear);

            var eventId = await _eventPublisherService.PublishAsync(eventDetail, CancellationToken.None);

            var result = _mapper.Map<BatchJobEventTriggerDto>(queued);
            result.EventId = eventId;          // set the field AutoMapper ignored
            return result;

           /// return _mapper.Map<BatchJobQueueDto>(queued);
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
