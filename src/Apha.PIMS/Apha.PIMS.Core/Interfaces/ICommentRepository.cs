/*
 * TRANSFORMENGINE MIGRATION — ICommentRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access DAO / ADODB named-query access pattern → async repository interface (Core layer)
 *   - GetCommentsByProjectAsync: replaces Access form RecordSource filter (project + year filter)
 *   - GetByIdAsync: replaces single-record DAO lookup by primary key
 *   - AddAsync / UpdateAsync / DeleteAsync: replaces DAO Insert/Edit/Delete operations
 *   - ExistsAsync: enforces unique constraint (project, year, topic) that mirrors ix_tblcomments in DDL
 *   - GetCommentTopicsAsync: replaces RowSource query on Topic combo-box from form
 *
 * PRESERVED:
 *   - All method signatures required by CommentService (Application layer)
 *   - PaginationParameters<string> typed filter matches downstream controller binding
 *   - ExistsAsync optional excludeCommentNo parameter supports Update-path duplicate check
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated. Topic filter parameter added in Phase 4 as planned.
 */
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: Interface contracts derive from frmtblComments.frm form operations and tblcomments DDL
    public interface ICommentRepository
    {
        // TRANSFORMENGINE: optional topic parameter added (Phase 4) for standalone Comments page filter
        Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query, string? topic = null);
        Task<Comment?> GetByIdAsync(int commentNo);
        Task<Comment> AddAsync(Comment entity);
        Task<Comment> UpdateAsync(Comment entity);
        Task<bool> DeleteAsync(int commentNo);
        Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentNo = null);
        Task<IEnumerable<CommentTopic>> GetCommentTopicsAsync();
    }
}
