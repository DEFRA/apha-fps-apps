using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the TestSupplier view — returns the paged project-supplier list for a given test code.
    /// CRUD for the underlying tlkptestreqmt table is handled by the PACT TestRequirement API.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/testsupplier")]
    public class TestSupplierController : ControllerBase
    {
        private readonly ITestSupplierService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSupplierController"/> class.
        /// </summary>
        /// <param name="service">Service for TestSupplier view queries.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public TestSupplierController(ITestSupplierService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paged list of project-supplier entries for the given test code.
        /// Includes project manager and test cost computed from the project join.
        /// </summary>
        /// <param name="query">Pagination and filter parameters.</param>
        /// <param name="testCode">The test code to filter by.</param>
        /// <param name="showRejected">When true, includes inactive (rejected) entries.</param>
        /// <returns>A paged list of TestSupplierView records.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPagedAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string testCode,
            [FromQuery] bool showRejected = false)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _service.GetPagedAsync(filter, testCode, showRejected);
            return Ok(_mapper.Map<PaginationRes<TestSupplierViewRes>>(result));
        }
    }
}
