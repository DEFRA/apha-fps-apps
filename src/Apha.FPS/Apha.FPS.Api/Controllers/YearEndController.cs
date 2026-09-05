using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/yearend")]
    public class YearEndController : ControllerBase
    {
        private readonly IYearEndService _yearEndService;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="YearEndController"/> with the required
        /// service and AutoMapper dependencies.
        /// </summary>
        /// <param name="yearEndService">Application service used to manage year-end operations.</param>
        /// <param name="fpsRequestContext">Provides the identity of the currently authenticated user.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public YearEndController(IYearEndService yearEndService, IFpsRequestContext fpsRequestContext, IMapper mapper)
        {
            _yearEndService = yearEndService;
            _fpsRequestContext = fpsRequestContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the execution history for a batch job by job name.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with a list of <see cref="BatchJobHistoryRes"/>.</returns>
        [HttpGet("datasetup/batchjob/history")]
        public async Task<IActionResult> GetYearEndDataSetupBatchJobHistory([FromQuery] QueryParameters<string> query, 
            [FromQuery] string jobName)
        {
            var result = await _yearEndService.GetBatchJobsHistoryAsync(query, jobName);
            return Ok(_mapper.Map<PaginationRes<BatchJobHistoryRes>>(result));
        }

        /// <summary>
        /// Checks whether year end datasetup batch job can be initiated.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if the job can run; <c>false</c> if it is already running.</returns>
        [HttpGet("datasetup/caninitiate")]
        public async Task<IActionResult> CanInitiateYearEndDataSetupRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanInitiateYearEndDataSetupRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Checks whether year end datasetup batch job can be Approved or rejected.
        /// </summary>
        /// <param name="jobName">The name of the batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if initiate request exists for the job; <c>false</c> if no initiate request exists for the job.</returns>
        [HttpGet("dataSetup/canapproveorreject")]
        public async Task<IActionResult> CanApproveOrRejectYearEndDataSetupRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Resolves the <c>JobExecutionId</c> of the single Year End Data Setup request currently in
        /// <c>Initiated</c> status, if any - the request the Confirm workflow is editing. Used by the
        /// FPSApps UI to recover which request it is editing after a page refresh, without relying on
        /// any client-side storage.
        /// </summary>
        /// <returns><c>200 OK</c> with <see cref="YearEndInitiatedRequestRes"/>; <c>JobExecutionId</c> is
        /// <see langword="null"/> when no request is Initiated.</returns>
        [HttpGet("datasetup/initiated")]
        public async Task<IActionResult> GetInitiatedDataSetupJobExecutionIdAsync()
        {
            var jobExecutionId = await _yearEndService.GetInitiatedDataSetupJobExecutionIdAsync();
            return Ok(new YearEndInitiatedRequestRes { JobExecutionId = jobExecutionId });
        }

        /// <summary>
        /// Enqueue the YearEndInitiation batch job for approval.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// all config exists, verifies no instance is already running, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("datasetup/initiation")]
        public async Task<IActionResult> EnqueueYearEndDataSetupInitiationJob([FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupInitiationJobAsync(request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobQueueRes>(result));

        }

        /// <summary>
        /// Approves the exact Year End Data Setup request identified by <paramref name="jobExecutionId"/>
        /// (planned-year staging design) - validates that request's staged Config Value/Month Hours rows
        /// (never the real tables), then publishes the event that triggers the Worker for its persisted
        /// target year.
        /// </summary>
        /// <param name="jobExecutionId">The Data Setup request being approved.</param>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("datasetup/approval")]
        public async Task<IActionResult> EnqueueYearEndDataSetupApprovalJob([FromQuery] Guid jobExecutionId, [FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupApprovalJobAsync(jobExecutionId, request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobEventTriggerRes>(result));
        }

        /// <summary>
        /// Rejects the exact Year End Data Setup request identified by <paramref name="jobExecutionId"/>
        /// (planned-year staging design) and deletes its staged Config Value/Month Hours rows in the same
        /// transaction as the status transition.
        /// </summary>
        /// <param name="jobExecutionId">The Data Setup request being rejected.</param>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("dataSetup/reject")]
        public async Task<IActionResult> EnqueueYearEndDataSetupRejectJob([FromQuery] Guid jobExecutionId, [FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndDataSetupRejectJobAsync(jobExecutionId, request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves the execution history for the year-end cut-over batch job.
        /// </summary>
        /// <param name="query">Pagination/filter query parameters.</param>
        /// <param name="jobName">The name of the cut-over batch job.</param>
        /// <returns><c>200 OK</c> with a paginated list of <see cref="BatchJobHistoryRes"/>.</returns>
        [HttpGet("cutover/batchjob/history")]
        public async Task<IActionResult> GetYearEndCutOverBatchJobHistory([FromQuery] QueryParameters<string> query, [FromQuery] string jobName)
        {
            var result = await _yearEndService.GetBatchJobsHistoryAsync(query, jobName);
            return Ok(_mapper.Map<PaginationRes<BatchJobHistoryRes>>(result));
        }

        /// <summary>
        /// Checks whether the year-end cut-over batch job can be initiated.
        /// </summary>
        /// <param name="jobName">The name of the cut-over batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if the job can be initiated; otherwise <c>false</c>.</returns>
        [HttpGet("cutover/caninitiate")]
        public async Task<IActionResult> CanInitiateYearEndCutOverRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanInitiateYearEndCutOverRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Checks whether the year-end cut-over batch job can be approved or rejected.
        /// </summary>
        /// <param name="jobName">The name of the cut-over batch job.</param>
        /// <returns><c>200 OK</c> with <c>true</c> if an initiate request exists; otherwise <c>false</c>.</returns>
        [HttpGet("cutover/canapproveorreject")]
        public async Task<IActionResult> CanApproveOrRejectYearEndCutOverRequestAsync([FromQuery] string jobName)
        {
            var result = await _yearEndService.CanApproveOrRejectYearEndCutOverRequestAsync(jobName);
            return Ok(result);
        }

        /// <summary>
        /// Resolves the <c>JobExecutionId</c> of the single Year End CutOver request currently in
        /// <c>Initiated</c> status, if any.
        /// </summary>
        [HttpGet("cutover/initiated")]
        public async Task<IActionResult> GetInitiatedCutOverJobExecutionIdAsync()
        {
            var jobExecutionId = await _yearEndService.GetInitiatedCutOverJobExecutionIdAsync();
            return Ok(new YearEndInitiatedRequestRes { JobExecutionId = jobExecutionId });
        }

        /// <summary>
        /// Enqueue the YearEnd CutOver Initiation batch job for approval.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// verifies no instance is already running, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("cutover/initiation")]
        public async Task<IActionResult> EnqueueYearEndCutOverInitiationJob([FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndCutOverInitiationJobAsync(request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobQueueRes>(result));
        }

        /// <summary>
        /// Enqueue the YearEndCutOverApproval batch job for approve and publish event for batch job.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// verifies no instance is already running and year not already processed, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("cutover/approval")]
        public async Task<IActionResult> EnqueueYearEndCutOverApprovalJob([FromQuery] Guid jobExecutionId, [FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndCutOverApprovalJobAsync(jobExecutionId, request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(_mapper.Map<BatchJobEventTriggerRes>(result));
        }

        /// <summary>
        /// Enqueue the YearEnd CutOver Initiation rejection batch job for approval.
        /// Validates that <paramref name="request"/>.<c>planned year</c> is valid,
        /// verifies initiation request is present, then enqueues the job. 
        /// </summary>
        /// <param name="request">Request body containing the planned year.</param>
        /// <returns><c>202 Accepted</c> with the enqueued <see cref="BatchJobQueueRes"/>.</returns>
        [HttpPost("cutover/reject")]
        public async Task<IActionResult> EnqueueYearEndCutOverRejectJobAsync([FromQuery] Guid jobExecutionId, [FromBody] YearEndDataSetupReq request, [FromHeader(Name = "X-Correlation-ID")] string correlationId)
        {
            var result = await _yearEndService.EnqueueYearEndCutOverRejectJobAsync(jobExecutionId, request.PlannedYear, _fpsRequestContext.FpsYear, _fpsRequestContext.UserEmailId, correlationId);

            return Ok(result);
        }
    }
}