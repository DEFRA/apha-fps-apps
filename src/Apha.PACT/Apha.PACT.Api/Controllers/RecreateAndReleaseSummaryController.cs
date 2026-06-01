using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PACT.Api.Controllers
{
    [Authorize(Roles = "API-PACTUser,API-PACTAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/recreatesummarieslog")]
    public class RecreateAndReleaseSummaryController : ControllerBase
    {
        private readonly IRecreateAndReleaseSummaryService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initialises a new instance of <see cref="RecreateAndReleaseSummaryController"/> with the required
        /// service and AutoMapper dependencies.
        /// </summary>
        /// <param name="service">Application service used to retrieve recreate summaries log data.</param>
        /// <param name="mapper">AutoMapper instance used to project application DTOs to API response contracts.</param>
        public RecreateAndReleaseSummaryController(IRecreateAndReleaseSummaryService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all recreate summaries logs from the system.
        /// </summary>
        /// <returns>
        /// <c>200 OK</c> with an <see cref="IEnumerable{RecreateSummariesLogRes}"/> containing all logs.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _service.GetAllLogsAsync();
            return Ok(_mapper.Map<IEnumerable<RecreateSummariesLogRes>>(items));
        }
    }
}
