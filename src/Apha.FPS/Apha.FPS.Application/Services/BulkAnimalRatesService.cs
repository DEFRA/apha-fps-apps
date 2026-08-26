using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Dtos.BulkRates;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Phase 1 skeleton — structure only, not yet wired into <c>BulkRatesRequestService</c>.
    /// Animal validation, staging, freeze and export/download logic moves here in Phase 4
    /// of the low-risk phase-wise execution plan.
    /// </summary>
    public class BulkAnimalRatesService : IBulkAnimalRatesService
    {
        private readonly IBulkRatesRepository _repository;

        public BulkAnimalRatesService(IBulkRatesRepository repository)
        {
            _repository = repository;
        }

        public Task<BulkRatesValidationResult> ProcessUploadAsync(
            BulkRatesParseResult parseResult, int fpsYear, int uploadVersion,
            CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task PrepareForReleaseAsync(
            Guid jobQueueId, int fpsYear, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<byte[]> ExportTestDataAsync(int fpsYear, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<byte[]> DownloadTestDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<BulkRatesStagingDataDto> GetStagingDataAsync(BulkRatesQueueRow entry, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<byte[]> ExportStagingDataAsync(Guid jobQueueId, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
