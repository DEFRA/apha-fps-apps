using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for managing budget bids and purchases at resource centre level.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/budgetresourcelevel")]
    public class BudgetResourceLevelController : ControllerBase
    {
        private readonly IBudgetResourceLevelService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetResourceLevelController"/> class.
        /// </summary>
        /// <param name="service">Service for budget resource level operations.</param>
        /// <param name="mapper">AutoMapper instance for DTO mapping.</param>
        public BudgetResourceLevelController(IBudgetResourceLevelService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns workgroups for a given profit centre.
        /// </summary>
        /// <param name="profitCentre">The profit centre identifier.</param>
        /// <returns>List of workgroups.</returns>
        [HttpGet("workgroups")]
        public async Task<IActionResult> GetWorkGroupsAsync([FromQuery] string profitCentre)
        {
            var result = await _service.GetWorkGroupsAsync(profitCentre);
            return Ok(result);
        }

        /// <summary>
        /// Returns bid view records for a given workgroup.
        /// </summary>
        /// <param name="workgroup">The workgroup name.</param>
        /// <returns>List of bid view records.</returns>
        [HttpGet("bids")]
        public async Task<IActionResult> GetBidViewAsync([FromQuery] string workgroup)
        {
            var result = await _service.GetBidViewAsync(workgroup);
            return Ok(_mapper.Map<List<BidViewRes>>(result));
        }

        /// <summary>
        /// Returns a single bid by workgroup name and account.
        /// </summary>
        /// <param name="workgroupName">The workgroup name.</param>
        /// <param name="account">The account short name.</param>
        /// <returns>The bid record.</returns>
        [HttpGet("bid/{workgroupName}/{account}")]
        public async Task<IActionResult> GetBidByIdAsync(string workgroupName, string account)
        {
            var result = await _service.GetBidByIdAsync(workgroupName, account);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Adds a new bid record.
        /// </summary>
        /// <param name="req">The bid request data.</param>
        /// <returns>The created bid record.</returns>
        [HttpPost("bid")]
        public async Task<IActionResult> AddBidAsync([FromBody] BidReq req)
        {
            var dto = _mapper.Map<BidDto>(req);
            var result = await _service.AddBidAsync(dto);
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Updates an existing bid record.
        /// </summary>
        /// <param name="req">The bid request data.</param>
        /// <returns>The updated bid record.</returns>
        [HttpPut("bid")]
        public async Task<IActionResult> UpdateBidAsync([FromBody] BidReq req)
        {
            var dto = _mapper.Map<BidDto>(req);
            var result = await _service.UpdateBidAsync(dto);
            return Ok(_mapper.Map<BidRes>(result));
        }

        /// <summary>
        /// Deletes a bid record by workgroup name and account.
        /// </summary>
        /// <param name="workgroupName">The workgroup name.</param>
        /// <param name="account">The account short name.</param>
        /// <returns>True if deleted; NotFound if not found.</returns>
        [HttpDelete("bid")]
        public async Task<IActionResult> DeleteBidAsync([FromQuery] string workgroupName, [FromQuery] string account)
        {
            var isDeleted = await _service.DeleteBidAsync(workgroupName, account);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }

        /// <summary>
        /// Returns purchases for a given workgroup and account.
        /// </summary>
        /// <param name="workgroupName">The workgroup name.</param>
        /// <param name="account">The account short name.</param>
        /// <returns>List of purchase records.</returns>
        [HttpGet("purchases")]
        public async Task<IActionResult> GetPurchasesAsync([FromQuery] string workgroupName, [FromQuery] string account)
        {
            var result = await _service.GetPurchasesAsync(workgroupName, account);
            return Ok(_mapper.Map<List<PurchaseRes>>(result));
        }

        /// <summary>
        /// Returns a single purchase by workgroup name, account and item description.
        /// </summary>
        /// <param name="workgroupName">The workgroup name.</param>
        /// <param name="account">The account short name.</param>
        /// <param name="itemDescription">The item description.</param>
        /// <returns>The purchase record.</returns>
        [HttpGet("purchase/{workgroupName}/{account}/{itemDescription}")]
        public async Task<IActionResult> GetPurchaseByIdAsync(string workgroupName, string account, string itemDescription)
        {
            var result = await _service.GetPurchaseByIdAsync(workgroupName, account, itemDescription);
            if (result == null)
                throw new KeyNotFoundException("Data not found.");
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Adds a new purchase record.
        /// </summary>
        /// <param name="req">The purchase request data.</param>
        /// <returns>The created purchase record.</returns>
        [HttpPost("purchase")]
        public async Task<IActionResult> AddPurchaseAsync([FromBody] PurchaseReq req)
        {
            var dto = _mapper.Map<PurchaseDto>(req);
            var result = await _service.AddPurchaseAsync(dto);
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Updates an existing purchase record.
        /// </summary>
        /// <param name="req">The purchase request data.</param>
        /// <returns>The updated purchase record.</returns>
        [HttpPut("purchase")]
        public async Task<IActionResult> UpdatePurchaseAsync([FromBody] PurchaseReq req)
        {
            var dto = _mapper.Map<PurchaseDto>(req);
            var result = await _service.UpdatePurchaseAsync(dto);
            return Ok(_mapper.Map<PurchaseRes>(result));
        }

        /// <summary>
        /// Deletes a purchase record.
        /// </summary>
        /// <param name="workgroupName">The workgroup name.</param>
        /// <param name="account">The account short name.</param>
        /// <param name="itemDescription">The item description.</param>
        /// <returns>True if deleted; NotFound if not found.</returns>
        [HttpDelete("purchase")]
        public async Task<IActionResult> DeletePurchaseAsync([FromQuery] string workgroupName, [FromQuery] string account, [FromQuery] string itemDescription)
        {
            var isDeleted = await _service.DeletePurchaseAsync(workgroupName, account, itemDescription);
            if (!isDeleted)
                throw new KeyNotFoundException("Data not found.");
            return Ok(isDeleted);
        }

        /// <summary>
        /// Returns profit centres available for budget resource level.
        /// </summary>
        /// <returns>List of profit centres.</returns>
        [HttpGet("profitcentres")]
        public async Task<IActionResult> GetProfitCentresAsync()
        {
            var result = await _service.GetProfitCentresAsync();
            return Ok(_mapper.Map<List<ProfitCentreRes>>(result));
        }

        /// <summary>
        /// Returns account categories for budget bids (RC-specific).
        /// </summary>
        /// <returns>List of account categories.</returns>
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAccountCategoriesAsync()
        {
            var categories = await _service.GetAccountCategoriesAsync();
            return Ok(_mapper.Map<List<AccountCategoryRes>>(categories));
        }
    }
}
