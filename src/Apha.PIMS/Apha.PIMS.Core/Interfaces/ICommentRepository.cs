using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Interfaces
{
    public interface ICommentRepository
    {
        Task<PagedData<Comment>> GetCommentsByProjectAsync(string project, int? year, PaginationParameters<string> query);
        Task<Comment?> GetByIdAsync(int commentno);
        Task<Comment> AddAsync(Comment entity);
        Task<Comment> UpdateAsync(Comment entity);
        Task<bool> DeleteAsync(int commentno);
        Task<bool> ExistsAsync(string project, short year, string topic, int? excludeCommentno = null);
        Task<IEnumerable<CommentTopic>> GetCommentTopicsAsync();
    }
}
