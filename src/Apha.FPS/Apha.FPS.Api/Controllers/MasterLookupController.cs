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
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/masterlookup")]
    [ApiController]
    [ApiVersion("1.0")]
    public class MasterLookupController : ControllerBase
    {
        private readonly IMasterLookupService _masterLookupService;
        private readonly IMapper _mapper;

        public MasterLookupController(IMasterLookupService masterLookupService, IMapper mapper)
        {
            _masterLookupService = masterLookupService ?? throw new ArgumentNullException(nameof(masterLookupService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<List<MasterLookupRes>>> GetAllMasterLookupsAsync()
        {
            var masterLookups = await _masterLookupService.GetAllMasterLookupsAsync();
            return Ok(masterLookups.Select(m => new MasterLookupRes { MasterTableName = m }).ToList());
        }

        /// <summary>
        /// Retrieves all values from a registered lookup table.
        /// </summary>
        /// <param name="tableName">The registered master lookup table name.</param>
        [HttpGet("{tableName}/items")]
        public async Task<ActionResult<List<LookupItemRes>>> GetLookupItemsAsync(string tableName)
        {
            var items = await _masterLookupService.GetLookupItemsAsync(tableName);
            return Ok(items.Select(v => new LookupItemRes { Value = v }).ToList());
        }

        /// <summary>
        /// Retrieves a paginated, filtered and sorted set of values from a registered lookup table.
        /// </summary>
        /// <param name="tableName">The registered master lookup table name.</param>
        /// <param name="query">Pagination, search, sort and filter parameters.</param>
        [HttpGet("{tableName}/items/paged")]
        public async Task<ActionResult> GetLookupItemsPagedAsync(string tableName, [FromQuery] PaginationReq<string> query)
        {
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _masterLookupService.GetLookupItemsPagedAsync(tableName, filter);

            var response = new PaginationRes<LookupItemRes>(
                result.Data.Select(v => new LookupItemRes { Value = v }),
                _mapper.Map<Pagination>(result.PaginationData));

            return Ok(response);
        }

        /// <summary>
        /// Creates a new value in a registered lookup table.
        /// </summary>
        [HttpPost("{tableName}/items")]
        public async Task<ActionResult<bool>> CreateLookupItemAsync(string tableName, [FromBody] LookupItemReq request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Value))
            {
                throw new ArgumentException("Value is required.", nameof(request));
            }

            var created = await _masterLookupService.CreateLookupItemAsync(tableName, request.Value);
            return Ok(created);
        }

        /// <summary>
        /// Updates an existing value in a registered lookup table.
        /// </summary>
        [HttpPut("{tableName}/items")]
        public async Task<ActionResult<bool>> UpdateLookupItemAsync(string tableName, [FromBody] LookupItemReq request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Value) || string.IsNullOrWhiteSpace(request.OriginalValue))
            {
                throw new ArgumentException("Both the original and new values are required.", nameof(request));
            }

            var updated = await _masterLookupService.UpdateLookupItemAsync(tableName, request.OriginalValue!, request.Value);
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a value from a registered lookup table.
        /// </summary>
        [HttpDelete("{tableName}/items/{value}")]
        public async Task<ActionResult<bool>> DeleteLookupItemAsync(string tableName, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value is required.", nameof(value));
            }

            var deleted = await _masterLookupService.DeleteLookupItemAsync(tableName, value);
            return Ok(deleted);
        }
    }
}
