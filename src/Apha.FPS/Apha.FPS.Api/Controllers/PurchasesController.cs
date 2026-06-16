using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for the Purchases section in the Generic Bid feature.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/purchases")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchasesService _service;
        private readonly IMapper _mapper;

        public PurchasesController(IPurchasesService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns purchases for a given workgroup and account.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPurchasesAsync([FromQuery] string WorkGroupName, [FromQuery] string account)
        {
            var result = await _service.GetPurchasesAsync(WorkGroupName, account);
            return Ok(_mapper.Map<List<PurchaseRes>>(result));
        }

        /// <summary>
        /// Returns a paged, filtered and sorted list of purchases for a given workgroup and account.
        /// </summary>
        [HttpGet("paged")]
        public async Task<IActionResult> GetPurchasesPagedAsync(
            [FromQuery] QueryParameters<string> query,
            [FromQuery] string WorkGroupName,
            [FromQuery] string account)
        {
            var result = await _service.GetPurchasesPagedAsync(query, WorkGroupName, account);
            return Ok(_mapper.Map<PaginationRes<PurchaseRes>>(result));
        }

        /// <summary>
        /// Returns a single purchase by workgroup name, account and item description.
        /// </summary>
        [HttpGet("{WorkGroupName}/{account}/{itemDescription}")]
        public async Task<IActionResult> GetPurchaseByIdAsync(string WorkGroupName, string account, string itemDescription)
        {
            var result = await _service.GetPurchaseByIdAsync(WorkGroupName, account, itemDescription);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Adds a new purchase record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddPurchaseAsync([FromBody] PurchaseReq req)
        {
            var dto = _mapper.Map<PurchaseDto>(req);
            var result = await _service.AddPurchaseAsync(dto);
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Updates an existing purchase record.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdatePurchaseAsync([FromBody] PurchaseReq req)
        {
            var dto = _mapper.Map<PurchaseDto>(req);
            var result = await _service.UpdatePurchaseAsync(dto);
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Deletes a purchase record.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeletePurchaseAsync([FromQuery] string WorkGroupName, [FromQuery] string account, [FromQuery] string itemDescription)
        {
            var isDeleted = await _service.DeletePurchaseAsync(WorkGroupName, account, itemDescription);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }
    }
}
