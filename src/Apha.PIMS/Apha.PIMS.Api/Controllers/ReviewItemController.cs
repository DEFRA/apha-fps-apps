/*
 * TRANSFORMENGINE MIGRATION — ReviewItemController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - MS Access Form (frmReviewItem) -> ASP.NET Core 10 Web API [ApiController]
 *   - VBA form CRUD operations -> REST endpoints: GET /reviewitem, GET /reviewitem/{itemid}, POST /reviewitem, PUT /reviewitem/{itemid}, DELETE /reviewitem/{itemid}
 *   - Access DAO data binding -> IReviewItemService dependency injection
 *   - Request/Response contracts mapped via AutoMapper (ReviewItemReq <-> ReviewItemDto <-> ReviewItemRes)
 *
 * PRESERVED:
 *   - Integer PK semantics (itemid)
 *   - All CRUD semantics from the original form
 *   - Authorization: API-PIMSUser, API-PIMSAdmin roles required
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm integer PK (itemid) generation strategy — verify DB identity/sequence vs application-assigned
 */
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reviewitem")]
    public class ReviewItemController : ControllerBase
    {
        private readonly IReviewItemService _service;
        private readonly IMapper _mapper;

        public ReviewItemController(IReviewItemService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>Get all review items.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // TRANSFORMENGINE: GetAllAsync -> GET /reviewitem (full list)
            List<ReviewItemDto> result = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<ReviewItemRes>>(result));
        }

        /// <summary>Get a single review item by itemid.</summary>
        [HttpGet("{itemid:int}")]
        public async Task<IActionResult> GetById(int itemid)
        {
            ReviewItemDto? result = await _service.GetByIdAsync(itemid);
            return result is null ? NotFound() : Ok(_mapper.Map<ReviewItemRes>(result));
        }

        /// <summary>Create a new review item.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ReviewItemReq request)
        {
            ReviewItemDto dto = _mapper.Map<ReviewItemDto>(request);
            ReviewItemDto created = await _service.CreateAsync(dto);
            ReviewItemRes res = _mapper.Map<ReviewItemRes>(created);
            return CreatedAtAction(nameof(GetById), new { itemid = res.ItemId, version = "1.0" }, res);
        }

        /// <summary>Update an existing review item.</summary>
        [HttpPut("{itemid:int}")]
        public async Task<IActionResult> Update(int itemid, [FromBody] ReviewItemReq request)
        {
            ReviewItemDto dto = _mapper.Map<ReviewItemDto>(request);
            // TRANSFORMENGINE: Route itemid is authoritative — set before service call
            dto.Itemid = itemid;
            ReviewItemDto updated = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<ReviewItemRes>(updated));
        }

        /// <summary>Delete a review item by itemid.</summary>
        [HttpDelete("{itemid:int}")]
        public async Task<IActionResult> Delete(int itemid)
        {
            await _service.DeleteAsync(itemid);
            return Ok(new { success = true });
        }
    }
}
