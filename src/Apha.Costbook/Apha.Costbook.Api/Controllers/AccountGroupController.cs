/*
 * TRANSFORMENGINE MIGRATION — AccountGroupController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New [ApiController] created from MS Access frmMaintainance Tab 3 (CSG7 Inflation Options)
 *   - Full CRUD: GET all, GET by csg7Group, POST, PUT, DELETE
 *   - Routes: GET    /api/v1/accountgroup
 *             GET    /api/v1/accountgroup/{csg7Group}
 *             POST   /api/v1/accountgroup
 *             PUT    /api/v1/accountgroup/{csg7Group}
 *             DELETE /api/v1/accountgroup/{csg7Group}
 *   - Depends on IAccountGroupService (Phase 3)
 *   - Uses AutoMapper for AccountGroupDto <-> AccountGroupReq/Res conversions
 *   - POST returns CreatedAtAction pointing to GetAccountGroup (resource identity = Csg7Group)
 *   - Authorization: read actions open to API-CostbookUser; mutating actions restricted to API-CostbookAdmin
 *   - Phase 14 security: [Required] + [MaxLength(15)] added to AccountGroupReq.Csg7Group in Apha.Common
 *     to enforce DB varchar(15) constraint at model-binding layer before service call
 *
 * PRESERVED:
 *   - All service operation semantics preserved from IAccountGroupService
 *   - Exception-driven flow (ArgumentException, KeyNotFoundException) handled by ExceptionMiddleware
 *   - Csg7Group natural string PK from AccountGroupReq.Csg7Group on create; route param on update/delete
 *   - string.IsNullOrWhiteSpace(csg7Group) guard on DELETE preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — max-length annotation resolved in Phase 14.
 */

using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.Costbook.Api.Controllers
{
    // TRANSFORMENGINE: Full CRUD controller for mabarchive.tblcsg7_accountgroups — covers Tab 3 (CSG7 Inflation Options) of frmMaintainance
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/accountgroup")]
    [Authorize(Roles = "API-CostbookAdmin,API-CostbookUser")]
    public class AccountGroupController : ControllerBase
    {
        private readonly IAccountGroupService _service;
        private readonly IMapper _mapper;

        public AccountGroupController(IAccountGroupService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all CSG7 account groups ordered by Csg7Group key.
        /// Drives the Tab 3 data grid and the CSG7 group dropdown in the Account Categories modal.
        /// </summary>
        /// <returns>200 OK with list of <see cref="AccountGroupRes"/> entries.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAccountGroups()
        {
            // TRANSFORMENGINE: GET /api/v1/accountgroup — returns full list for Tab 3 grid + AccCat modal dropdown
            var dtos = await _service.GetAllAsync();
            return Ok(_mapper.Map<List<AccountGroupRes>>(dtos));
        }

        /// <summary>
        /// Returns a single CSG7 account group by Csg7Group key.
        /// </summary>
        /// <param name="csg7Group">CSG7 group name (primary key).</param>
        /// <returns>200 OK with <see cref="AccountGroupRes"/>, or 404 if not found.</returns>
        [HttpGet("{csg7Group}")]
        public async Task<IActionResult> GetAccountGroup(string csg7Group)
        {
            // TRANSFORMENGINE: GET /api/v1/accountgroup/{csg7Group} — single record lookup
            var dto = await _service.GetByCsg7GroupAsync(csg7Group);
            if (dto == null) return NotFound();
            return Ok(_mapper.Map<AccountGroupRes>(dto));
        }

        /// <summary>
        /// Creates a new CSG7 account group.
        /// </summary>
        /// <param name="req">Account group fields. Csg7Group must be unique (varchar 15).</param>
        /// <returns>201 Created with <see cref="AccountGroupRes"/> of the new record.</returns>
        [HttpPost]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> AddAccountGroup([FromBody] AccountGroupReq req)
        {
            // TRANSFORMENGINE: POST /api/v1/accountgroup — create from Tab 3 modal (formTblCsg7)
            var dto = _mapper.Map<AccountGroupDto>(req);
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetAccountGroup), new { csg7Group = created.Csg7Group }, _mapper.Map<AccountGroupRes>(created));
        }

        /// <summary>
        /// Updates an existing CSG7 account group.
        /// </summary>
        /// <param name="csg7Group">CSG7 group name (route key).</param>
        /// <param name="req">Updated account group fields.</param>
        /// <returns>200 OK with updated <see cref="AccountGroupRes"/>.</returns>
        [HttpPut("{csg7Group}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> UpdateAccountGroup(string csg7Group, [FromBody] AccountGroupReq req)
        {
            // TRANSFORMENGINE: PUT /api/v1/accountgroup/{csg7Group} — update from Tab 3 edit modal
            var dto = _mapper.Map<AccountGroupDto>(req);
            var updated = await _service.UpdateAsync(csg7Group, dto);
            return Ok(_mapper.Map<AccountGroupRes>(updated));
        }

        /// <summary>
        /// Deletes the CSG7 account group identified by Csg7Group.
        /// </summary>
        /// <param name="csg7Group">CSG7 group name (route key).</param>
        /// <returns>204 No Content on success.</returns>
        [HttpDelete("{csg7Group}")]
        [Authorize(Roles = "API-CostbookAdmin")]
        public async Task<IActionResult> DeleteAccountGroup(string csg7Group)
        {
            // TRANSFORMENGINE: DELETE /api/v1/accountgroup/{csg7Group} — delete from Tab 3 confirm modal
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group is required for deletion.");

            await _service.DeleteAsync(csg7Group);
            return NoContent();
        }
    }
}
