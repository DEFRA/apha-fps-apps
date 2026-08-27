using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class BulkRatesService : IBulkRatesService
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IS3StorageService _s3StorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BulkRatesService> _logger;

        public BulkRatesService(
            IFpsApiClient fpsClient,
            IS3StorageService s3StorageService,
            IConfiguration configuration,
            ILogger<BulkRatesService> logger)
        {
            _fpsClient = fpsClient;
            _s3StorageService = s3StorageService;
            _configuration = configuration;
            _logger = logger;
        }

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> CreateRequestAsync(string jobName, int fpsYear)
            => _fpsClient.FpsBulkRates.CreateRequestAsync(jobName, fpsYear);

        // Main upload/validation/staging is the authoritative operation, owned entirely by FPS
        // API. S3 retention here is a best-effort audit copy only — attempted after the main
        // operation succeeds, never able to fail or roll it back. Mirrors PACT's existing
        // MonthlyOutput/MonthlyTime audit-copy pattern (PactMonthlyOutputService.UploadAuditFileAsync).
        public async Task<ApiResponseDto<BulkRatesUploadResultDto>> UploadFileAsync(Guid jobExecutionId, byte[] fileBytes, string fileName)
        {
            var response = await _fpsClient.FpsBulkRates.UploadFileAsync(jobExecutionId, fileBytes, fileName);

            if (response.Success)
            {
                try
                {
                    using var stream = new MemoryStream(fileBytes);
                    var uploadResult = await UploadAuditCopyAsync(jobExecutionId, fileName, stream);
                    if (!uploadResult.Success)
                    {
                        _logger.LogWarning(
                            "Bulk Rates upload succeeded but S3 audit upload failed. JobExecutionId: {JobExecutionId}, FileName: {FileName}, ErrorCode: {ErrorCode}, Message: {Message}",
                            jobExecutionId, fileName, uploadResult.ErrorCode, uploadResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Bulk Rates upload succeeded but S3 audit upload threw an exception. JobExecutionId: {JobExecutionId}, FileName: {FileName}",
                        jobExecutionId, fileName);
                }
            }

            return response;
        }

        private Task<S3UploadResult> UploadAuditCopyAsync(Guid jobExecutionId, string fileName, Stream fileStream)
        {
            var sourceFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(sourceFileName))
                sourceFileName = "bulk-rates-upload.xlsx";

            var timestamp = DateTime.UtcNow;
            var originalName = Path.GetFileNameWithoutExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(originalName))
                originalName = "bulk-rates-upload";

            var extension = Path.GetExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".xlsx";

            var auditFileName = $"{originalName}_{timestamp:yyyyMMddHHmmss}{extension}";
            var folderPath = $"BulkRates/{jobExecutionId}";

            var bucket = _configuration["S3Storage:BucketName"]
                ?? throw new InvalidOperationException("S3Storage:BucketName is not configured.");

            return _s3StorageService.UploadFileAsync(
                fileStream, bucket, folderPath, auditFileName,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        public Task<ApiResponseDto<BulkRatesUploadResultDto>> GetValidationResultsAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetValidationResultsAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ReleaseForApprovalAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.ReleaseForApprovalAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> ApproveAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.ApproveAsync(jobExecutionId);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> RejectAsync(Guid jobExecutionId, string reason)
            => _fpsClient.FpsBulkRates.RejectAsync(jobExecutionId, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto>> CancelAsync(Guid jobExecutionId, string? reason)
            => _fpsClient.FpsBulkRates.CancelAsync(jobExecutionId, reason);

        public Task<ApiResponseDto<BulkRatesRequestDetailDto?>> GetRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetRequestAsync(jobExecutionId);

        public Task<ApiResponseDto<List<BulkRatesQueueEntryDto>>> GetRequestsAsync(
            QueryParameters<string> query, string? jobName = null, int? fpsYear = null, string? status = null)
            => _fpsClient.FpsBulkRates.GetRequestsAsync(query, jobName, fpsYear, status);

        public Task<byte[]> DownloadFecTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadFecTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadStaffTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadStaffTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadAnimalTestDataForRequestAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadAnimalTestDataForRequestAsync(jobExecutionId);

        public Task<byte[]> DownloadFecTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadFecTestDataAsync(fpsYear);

        public Task<byte[]> DownloadStaffTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadStaffTestDataAsync(fpsYear);

        public Task<byte[]> DownloadAnimalTestDataAsync(int fpsYear)
            => _fpsClient.FpsBulkRates.DownloadAnimalTestDataAsync(fpsYear);

        public Task<ApiResponseDto<bool>> CanInitiateRequestAsync(string jobName)
            => _fpsClient.FpsBulkRates.CanInitiateRequestAsync(jobName);

        public Task<ApiResponseDto<BulkRatesStagingDataDto>> GetStagingDataAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.GetStagingDataAsync(jobExecutionId);

        public Task<byte[]> DownloadStagingDataAsync(Guid jobExecutionId)
            => _fpsClient.FpsBulkRates.DownloadStagingDataAsync(jobExecutionId);
    }
}
