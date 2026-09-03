using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMonthHourService
    {
        Task<PaginatedResult<MonthHourDto>> GetAllMonthHourAsync(QueryParameters<string> query);
        Task<IEnumerable<MonthHourDto>> GetMonthHoursByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();

        /// <summary>
        /// Legacy read: current/Open + Planned (YearMasters-status-driven) year-end month hours.
        /// Unchanged by the planned-year staging design — kept for callers that don't yet supply a
        /// JobExecutionId (FPSApps page-load path; Workstream 8 will migrate/remove this).
        /// </summary>
        Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync();

        /// <summary>
        /// Grid read path (planned-year staging design): resolves <paramref name="jobExecutionId"/>
        /// to its request, then returns current/Open-year values overlaid with that request's staged
        /// rows. Throws if <paramref name="jobExecutionId"/> doesn't resolve to a Year End Data Setup
        /// request — never falls back to "whichever request is currently active".
        /// </summary>
        Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync(Guid jobExecutionId);

        /// <summary>
        /// Confirm (planned-year staging design): resolves <paramref name="jobExecutionId"/> to its
        /// Year End Data Setup request, requires it to be Initiated (staging is immutable once
        /// Approved/Running/Completed/Failed/Rejected), then upserts a staged row — never writes
        /// fps.tlkpmonthhours directly. JobExecutionId is required, not optional: a write with no
        /// resolvable request identity would undermine the whole staging design.
        /// </summary>
        Task<MonthHourDto> SaveMonthHourAsync(Guid jobExecutionId, MonthHourDto dto);
    }
}
