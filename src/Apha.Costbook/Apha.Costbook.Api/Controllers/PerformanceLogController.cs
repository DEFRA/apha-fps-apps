using Apha.Common.Contracts;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.DataAccess.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    /// <summary>
    /// Accepts performance-log entries (e.g. outbound API-call timings posted by the web app)
    /// and hands them to the background writer for persistence to <c>performance_log</c>.
    /// </summary>
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("performance-logs")]
    public class PerformanceLogController : ControllerBase
    {
        private readonly IPerformanceLogQueue _queue;

        public PerformanceLogController(IPerformanceLogQueue queue)
        {
            _queue = queue;
        }

        [HttpPost("")]
        public IActionResult Post([FromBody] PerformanceLogReq request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            _queue.Enqueue(new PerformanceLog
            {
                LogType = string.IsNullOrWhiteSpace(request.LogType) ? "Api" : request.LogType,
                StartTimeUtc = request.StartTimeUtc,
                EndTimeUtc = request.EndTimeUtc,
                DurationMs = request.DurationMs,
                CommandKind = request.CommandKind,
                CommandId = request.CommandId,
                CommandText = request.CommandText,
                Parameters = request.Parameters,
                ApiName = request.ApiName,
                HttpMethod = request.HttpMethod,
                StatusCode = request.StatusCode,
                Outcome = request.Outcome,
                Url = request.Url,
                CorrelationId = request.CorrelationId,
                CreatedAt = DateTime.UtcNow
            });

            return Accepted();
        }
    }
}
