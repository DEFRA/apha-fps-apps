using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Owns the FEC/AGRUP ("Test") Bulk Rates process end to end: upload validation and
    /// staging, release-time revalidation and freeze, and staging/export/download
    /// presentation. Target of the Bulk Rates service/validation refactor
    /// (see docs/bulk-rates-service-and-validation-refactor-implementation-plan-2026-08-26.md);
    /// not yet wired into <c>BulkRatesRequestService</c>.
    /// </summary>
    public interface IBulkTestRatesService
    {
        Task<BulkRatesValidationResult> ProcessUploadAsync(
            BulkRatesParseResult parseResult, int fpsYear, int uploadVersion, int? downloadVersion,
            CancellationToken ct = default);

        Task PrepareForReleaseAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion,
            CancellationToken ct = default);

        Task<byte[]> ExportTestDataAsync(int fpsYear, CancellationToken ct = default);

        Task<byte[]> DownloadTestDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default);

        Task<BulkRatesStagingDataDto> GetStagingDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default);

        Task<byte[]> ExportStagingDataAsync(Guid jobQueueId, CancellationToken ct = default);
    }
}
