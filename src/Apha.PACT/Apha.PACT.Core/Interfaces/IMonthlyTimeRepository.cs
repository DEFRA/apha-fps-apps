using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject);

        Task<PagedData<MonthlyTime>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month);

        Task<MonthlyTime?> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<MonthlyTime> UpdateLiveAsync(MonthlyTime monthlyTime);
        Task<bool> DeleteLiveAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<bool> ExistsAsync(string pactStaffId, string timeCode, double month, string parentProject);

        Task<PagedData<StagingMonthlyTime>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed);

        Task<StagingMonthlyTime?> GetStagingByIdAsync(int id, string importedBy);
        Task<StagingMonthlyTime> CreateStagingAsync(StagingMonthlyTime stagingMonthlyTime);
        Task<StagingMonthlyTime> UpdateStagingAsync(StagingMonthlyTime stagingMonthlyTime, string importedBy);
        Task<bool> DeleteStagingAsync(int id, string importedBy);
        Task<int> DeleteAllStagingByUserAsync(string importedBy);
        Task<int> DeleteFailedStagingByUserAsync(string importedBy);
        Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyTime> stagingRows);
        Task<int> RemoveZeroAndNullHourRecordsAsync(string importedBy);
        Task<List<StagingMonthlyTime>> GetStagingRecordsForValidationAsync(string importedBy);
        Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyTime> records);
        Task<HashSet<string>> GetExistingLiveKeysAsync();
        Task<(int PassedCount, int FailedCount)> ValidateStagingAsync(string importedBy);
        Task<bool> HasFailedStagingAsync(string importedBy);
        Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy);

        Task<PagedData<MonthlyTimeLog>> SearchAsync(
            PaginationParameters<string> query,
            MonthlyTimeLogFilter monthlyTimeLogFilter);
    }
}
