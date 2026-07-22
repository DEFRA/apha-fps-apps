/*
 * TRANSFORMENGINE MIGRATION — ProjectCommentController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - GetCommentsByProject: added [FromQuery] string? topic optional parameter to support standalone
 *     Comments page topic filter
 *   - topic forwarded to ICommentService.GetCommentsByProjectAsync (interface updated in Phase 4)
 *   - Added XML summary doc comments on all public actions
 *
 * PRESERVED:
 *   - All existing CRUD endpoints: GetCommentsByProject, GetById, Create, Update, Delete
 *   - GetCommentTopics lookup endpoint
 *   - [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")] on controller
 *   - ApiVersion("1.0") and route convention api/v{version:apiVersion}/projectcomment
 *   - CreatedAtAction pattern on Create
 *   - KeyNotFoundException throw pattern on GetById (mapped by ExceptionMiddleware)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.PIMS.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "API-PIMSUser,API-PIMSAdmin")]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/projectcomment")]
    public class ProjectCommentController : ControllerBase
    {
        private readonly ICommentService _service;
        private readonly IMapper _mapper;

        public ProjectCommentController(ICommentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns a paginated list of comments for a project, optionally filtered by year and topic.
        /// </summary>
        /// <param name="project">Project code.</param>
        /// <param name="year">Optional fiscal year filter.</param>
        /// <param name="topic">Optional comment topic filter.</param>
        /// <param name="query">Pagination and sort parameters.</param>
        // TRANSFORMENGINE: topic parameter added — forwarded to service to support standalone Comments page filter
        [HttpGet]
        public async Task<IActionResult> GetCommentsByProject(
            [FromQuery] string project,
            [FromQuery] int? year,
            [FromQuery] string? topic,
            [FromQuery] PaginationReq<string> query)
        {
            QueryParameters<string> filter = _mapper.Map<QueryParameters<string>>(query);
            PaginatedResult<CommentDto> result = await _service.GetCommentsByProjectAsync(project, year, filter, topic);
            return Ok(_mapper.Map<PaginationRes<CommentRes>>(result));
        }

        /// <summary>
        /// Returns a single comment by its comment number.
        /// </summary>
        /// <param name="commentno">Comment record identifier.</param>
        [HttpGet("{commentno:int}")]
        public async Task<IActionResult> GetById(int commentno)
        {
            CommentDto? result = await _service.GetByIdAsync(commentno);
            if (result is null)
                throw new KeyNotFoundException($"Comment {commentno} not found.");
            return Ok(_mapper.Map<CommentRes>(result));
        }

        /// <summary>
        /// Creates a new project comment.
        /// </summary>
        /// <param name="request">Comment creation request.</param>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentReq request)
        {
            CommentDto dto = _mapper.Map<CommentDto>(request);
            CommentDto result = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { commentno = result.CommentNo }, _mapper.Map<CommentRes>(result));
        }

        /// <summary>
        /// Updates an existing project comment.
        /// </summary>
        /// <param name="commentno">Comment record identifier from route.</param>
        /// <param name="request">Updated comment payload.</param>
        [HttpPut("{commentno:int}")]
        public async Task<IActionResult> Update(int commentno, [FromBody] CommentReq request)
        {
            CommentDto dto = _mapper.Map<CommentDto>(request);
            dto.CommentNo = commentno;
            CommentDto result = await _service.UpdateAsync(dto);
            return Ok(_mapper.Map<CommentRes>(result));
        }

        /// <summary>
        /// Deletes a project comment by its comment number.
        /// </summary>
        /// <param name="commentno">Comment record identifier.</param>
        [HttpDelete("{commentno:int}")]
        public async Task<IActionResult> Delete(int commentno)
        {
            bool deleted = await _service.DeleteAsync(commentno);
            return Ok(deleted);
        }

        /// <summary>
        /// Returns all available comment topics for use in filter dropdowns.
        /// </summary>
        [HttpGet("commenttopics")]
        public async Task<IActionResult> GetCommentTopics()
        {
            IEnumerable<CommentTopicDto> topics = await _service.GetCommentTopicsAsync();
            return Ok(_mapper.Map<IEnumerable<CommentTopicRes>>(topics));
        }
    }
}
