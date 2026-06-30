using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin, API-PACTShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/workgroupreport")]
    public class WorkGroupReportController : ControllerBase
    {
        private readonly IWorkGroupReportService _service;
        private readonly IMapper _mapper;

        public WorkGroupReportController(IWorkGroupReportService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Triggers the email-send process for all work groups flagged for email within the
        /// specified profit centre and calendar month. Returns a per-work-group result set
        /// containing the work group name, recipient address, send status, and failure reason
        /// where applicable.
        /// </summary>
        /// <param name="request">Contains the target profit-centre code and calendar month number.</param>
        /// <param name="cancellationToken">Token used to cancel the operation if the request is aborted.</param>
        [HttpPost("send")]
        public async Task<IActionResult> SendEmails([FromBody] WorkGroupReportEmailReq request, CancellationToken cancellationToken)
        {
            var results = await _service.SendEmailsAsync(request.ProfitCentre, request.MonthNumber, cancellationToken);
            return Ok(_mapper.Map<IEnumerable<WorkGroupReportEmailResultRes>>(results));
        }

        [HttpPost("cos90s/export")]
        public async Task<IActionResult> ExportCos90s([FromBody] WorkGroupCos90sExportReq request, CancellationToken cancellationToken)
        {
            var result = await _service.ExportCos90sAsync(
                request.ProfitCentre,
                request.MonthNumber,
                request.Year,
                request.PactId,
                cancellationToken);

            return Ok(_mapper.Map<WorkGroupCos90SExportRes>(result));
        }
    }
}
