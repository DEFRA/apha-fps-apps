/*
 * TRANSFORMENGINE MIGRATION — ICommentService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access form operations (frmtblComments RecordSource + DAO CRUD) → typed async service interface
 *   - GetCommentsByProjectAsync: optional `string? topic` parameter added to support standalone Comments page topic filter
 *   - GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync: map to MS Access form record navigation + DAO Save/Delete
 *   - GetCommentTopicsAsync: replaces RowSource named query on Topic combo-box
 *
 * PRESERVED:
 *   - All 6 original method signatures visible to backend controller callers
 *   - PaginatedResult<CommentDto> / QueryParameters<string> typed contracts maintained
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Interfaces
{
    public interface ICommentService
    {
        // TRANSFORMENGINE: optional topic parameter added — supports standalone Comments page filter (Interface changes log)
        Task<PaginatedResult<CommentDto>> GetCommentsByProjectAsync(string project, int? year, QueryParameters<string> query, string? topic = null);
        Task<CommentDto?> GetByIdAsync(int commentno);
        Task<CommentDto> AddAsync(CommentDto dto);
        Task<CommentDto> UpdateAsync(CommentDto dto);
        Task<bool> DeleteAsync(int commentno);
        Task<IEnumerable<CommentTopicDto>> GetCommentTopicsAsync();
    }
}
