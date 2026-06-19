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
    /// API controller for managing User Permissions (Maintain User Permissions form).
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [Route("api/v{version:apiVersion}/userpermission")]
    [ApiController]
    [ApiVersion("1.0")]
    public class UserPermissionController : ControllerBase
    {
        private readonly IUserPermissionService _service;
        private readonly IMapper _mapper;

        public UserPermissionController(IUserPermissionService service, IMapper mapper)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>Gets all users.</summary>
        [HttpGet("users")]
        public async Task<ActionResult> GetAllUsersAsync()
        {
            var users = await _service.GetAllUsersAsync();
            return Ok(_mapper.Map<List<UserRes>>(users));
        }

        /// <summary>Gets a paged list of users.</summary>
        [HttpGet("users/paged")]
        public async Task<ActionResult> GetAllUsersPagedAsync([FromQuery] QueryParameters<string> query)
        {
            var paged = await _service.GetAllUsersPagedAsync(query);
            return Ok(_mapper.Map<PaginationRes<UserRes>>(paged));
        }

        /// <summary>Gets a user by ID.</summary>
        [HttpGet("users/{userId:int}")]
        public async Task<ActionResult<UserRes>> GetUserByIdAsync(int userId)
        {
            var dto = await _service.GetUserByIdAsync(userId);
            if (dto == null)
                throw new ArgumentException($"User with ID {userId} not found.");
            return Ok(_mapper.Map<UserRes>(dto));
        }

        /// <summary>Creates a new user.</summary>
        [HttpPost("users")]
        public async Task<ActionResult<UserRes>> CreateUser([FromBody] UserReq req)
        {
            var dto = _mapper.Map<UserDto>(req);
            var added = await _service.AddUserAsync(dto);
            return Ok(_mapper.Map<UserRes>(added));
        }

        /// <summary>Updates an existing user.</summary>
        [HttpPut("users")]
        public async Task<ActionResult<UserRes>> UpdateUser([FromBody] UserReq req)
        {
            var dto = _mapper.Map<UserDto>(req);
            var updated = await _service.UpdateUserAsync(dto);
            return Ok(_mapper.Map<UserRes>(updated));
        }

        /// <summary>Deletes a user and all associated permissions.</summary>
        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            var deleted = await _service.DeleteUserAsync(userId);
            if (!deleted)
                throw new ArgumentException($"User with ID {userId} not found for deletion.");
            return Ok(deleted);
        }

        /// <summary>Gets all permissions for a specific user.</summary>
        [HttpGet("{userId:int}/permissions")]
        public async Task<ActionResult<UserPermissionRes>> GetUserPermissionsAsync(int userId)
        {
            var dto = await _service.GetUserPermissionsAsync(userId);
            return Ok(_mapper.Map<UserPermissionRes>(dto));
        }

        /// <summary>Saves all permissions for a specific user (replace-all strategy).</summary>
        [HttpPut("{userId:int}/permissions")]
        public async Task<IActionResult> SaveUserPermissionsAsync(int userId, [FromBody] UserPermissionReq req)
        {
            var dto = _mapper.Map<UserPermissionDto>(req);
            dto.UserId = userId;
            await _service.SaveUserPermissionsAsync(dto);
            return Ok(true);
        }

        /// <summary>Gets all available permission option lists.</summary>
        [HttpGet("options")]
        public async Task<ActionResult<PermissionOptionsRes>> GetPermissionOptionsAsync()
        {
            var dto = await _service.GetPermissionOptionsAsync();
            return Ok(_mapper.Map<PermissionOptionsRes>(dto));
        }
    }
}
