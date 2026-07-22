/*
 * TRANSFORMENGINE MIGRATION — CommentRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - LINQ-first repository implementing ICommentRepository for mabarchive.tblcomments
 *   - GetCommentsByProjectAsync: optional `string? topic` parameter added and applied as WHERE c.Topic == topic
 *     This completes the Phase 3 deferred forwarding from CommentService → ICommentRepository
 *   - All reads use AsNoTracking via base class (BaseRepository.ApplyPaging)
 *   - ExistsAsync uses AnyAsync guard for duplicate detection (unique index ix_tblcomments)
 *   - AddAsync / UpdateAsync / DeleteAsync use SaveChangesAsync with tracked entity
 *   - GetCommentTopicsAsync returns full tlkpcommenttopics lookup table
 *
 * PRESERVED:
 *   - All 6 public methods: GetCommentsByProjectAsync, GetByIdAsync, ExistsAsync, AddAsync, UpdateAsync, DeleteAsync, GetCommentTopicsAsync
 *   - All 3 private sort helpers: ApplySorting, ApplySortingByProperty, ApplyOrder<T>
 *   - All sort property branches: commentno, project, year, topic, dateentered, madeby
 *   - ExistsAsync optional excludeCommentNo parameter for update-path duplicate check
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Apha.PIMS.DataAccess.Repository
{
    public class CommentRepository:BaseRepository, ICommentRepository
    {
        private readonly PimsDbContext _dbContext;

        public CommentRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // TRANSFORMENGINE: optional topic parameter added — Phase 4 completion of Phase 3 deferred forwarding
        public async Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query, string? topic = null)
        {
            IQueryable<Comment> baseQuery = _dbContext.Comments
                .Where(c => c.Project == project);

            if (year.HasValue)
                baseQuery = baseQuery.Where(c => c.Year == year.Value);

            // TRANSFORMENGINE: topic filter for standalone Comments page — applied only when topic is provided
            if (!string.IsNullOrEmpty(topic))
                baseQuery = baseQuery.Where(c => c.Topic == topic);

            baseQuery = (IQueryable<Comment>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await base.ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<Comment?> GetByIdAsync(int commentNo)
        {
            return await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.CommentNo == commentNo);
        }

        public async Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentNo = null)
        {
            IQueryable<Comment> query = _dbContext.Comments
                .Where(c => c.Project == project && c.Year == year && c.Topic == topic);

            if (excludeCommentNo.HasValue)
                query = query.Where(c => c.CommentNo != excludeCommentNo.Value);

            return await query.AnyAsync();
        }

        public async Task<Comment> AddAsync(Comment entity)
        {
            _dbContext.Comments.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Comment> UpdateAsync(Comment entity)
        {
            _dbContext.Comments.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int commentNo)
        {
            Comment? entity = await _dbContext.Comments.FindAsync(commentNo);
            if (entity is null) return false;
            _dbContext.Comments.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CommentTopic>> GetCommentTopicsAsync()
        { 
            return await _dbContext.CommentTopics.ToListAsync();
        }

        private static IQueryable ApplySorting(IQueryable<Comment> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<Comment> query, string property, bool descending)
        {
            return property switch
            {
                "commentno" => ApplyOrder(query, c => c.CommentNo, descending),
                "project" => ApplyOrder(query, c => c.Project, descending),
                "year" => ApplyOrder(query, c => c.Year, descending),
                "topic" => ApplyOrder(query, c => c.Topic, descending),
                "dateentered" => ApplyOrder(query, c => c.DateEntered, descending),
                "madeby" => ApplyOrder(query, c => c.MadeBy, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Comment> query, Expression<Func<Comment, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
