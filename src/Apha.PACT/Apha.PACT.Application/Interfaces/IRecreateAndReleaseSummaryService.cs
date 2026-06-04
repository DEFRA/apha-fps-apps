using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IRecreateAndReleaseSummaryService
    {
        Task<PaginatedResult<RecreateSummaryLogDto>> GetRecreateSummaryLogAsync(QueryParameters<string> query);
    }
    
}
