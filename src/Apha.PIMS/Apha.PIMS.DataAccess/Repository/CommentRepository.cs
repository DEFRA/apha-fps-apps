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

        public async Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query)
        {
            IQueryable<Comment> baseQuery = _dbContext.Comments
                .Where(c => c.Project == project);

            if (year.HasValue)
                baseQuery = baseQuery.Where(c => c.Year == year.Value);

            baseQuery = (IQueryable<Comment>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            List<Comment> result = await baseQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Comment?> GetByIdAsync(int commentno)
        {
            return await _dbContext.Comments
                .FirstOrDefaultAsync(c => c.Commentno == commentno);
        }

        public async Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentno = null)
        {
            IQueryable<Comment> query = _dbContext.Comments
                .Where(c => c.Project == project && c.Year == year && c.Topic == topic);

            if (excludeCommentno.HasValue)
                query = query.Where(c => c.Commentno != excludeCommentno.Value);

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

        public async Task<bool> DeleteAsync(int commentno)
        {
            Comment? entity = await _dbContext.Comments.FindAsync(commentno);
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
                "commentno" => ApplyOrder(query, c => c.Commentno, descending),
                "project" => ApplyOrder(query, c => c.Project, descending),
                "year" => ApplyOrder(query, c => c.Year, descending),
                "topic" => ApplyOrder(query, c => c.Topic, descending),
                "dateentered" => ApplyOrder(query, c => c.Dateentered, descending),
                "madeby" => ApplyOrder(query, c => c.Madeby, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Comment> query, Expression<Func<Comment, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
