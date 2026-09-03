using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMonthHourRepository
    {
        Task<PagedData<MonthHour>> GetAllAsync(PaginationParameters<string> query);
        Task<IEnumerable<MonthHour>> GetByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();

        /// <summary>
        /// Legacy read: current/Open + Planned (YearMasters-status-driven) year-end month hours.
        /// Still used by YearEndService's Approve-time ValidateConfiguration, unchanged by the
        /// planned-year staging design — do not repurpose this overload for the grid read path.
        /// </summary>
        Task<List<YearEndMonthHour>> GetYearEndMonthHoursAsync();

        /// <summary>
        /// Grid read path (planned-year staging design): current/Open-year real values overlaid
        /// with staged rows for <paramref name="request"/>'s JobQueueId. ExistsForPlannedYear
        /// reflects "has a staged row", not "a real target-year row exists" — there isn't one
        /// pre-Approval.
        /// </summary>
        Task<List<YearEndMonthHour>> GetYearEndMonthHoursAsync(YearEndRequestSummary request);

        Task<MonthHour> SaveAsync(MonthHour monthHour);
    }
}
