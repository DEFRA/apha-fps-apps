using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.BulkRatesServiceTest;

/// <summary>
/// Covers Point 2 of docs/bulkrates-review-fixes: S3 retention is a best-effort audit copy
/// performed here (Web/Application layer) only after the main FPS API upload/validation/staging
/// operation has already succeeded, mirroring PACT's MonthlyOutput/MonthlyTime audit-copy
/// pattern. It must never be able to fail or alter the main operation's result.
/// </summary>
public class BulkRatesServiceTests
{
    private static readonly Guid JobExecutionId = Guid.NewGuid();
    private const string FileName = "rates.xlsx";
    private static readonly byte[] FileBytes = [1, 2, 3];

    private static (BulkRatesService Service, IFpsBulkRatesApiClient FpsBulkRates, IS3StorageService S3)
        CreateService(ApiResponseDto<BulkRatesUploadResultDto>? mainResponse = null)
    {
        var fpsBulkRates = Substitute.For<IFpsBulkRatesApiClient>();
        fpsBulkRates.UploadFileAsync(Arg.Any<Guid>(), Arg.Any<byte[]>(), Arg.Any<string>())
            .Returns(mainResponse ?? ApiResponseDto<BulkRatesUploadResultDto>.SuccessResponse(new BulkRatesUploadResultDto()));

        var fpsClient = Substitute.For<IFpsApiClient>();
        fpsClient.FpsBulkRates.Returns(fpsBulkRates);

        var s3 = Substitute.For<IS3StorageService>();
        s3.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(S3UploadResult.SuccessResponse("BulkRates/key.xlsx"));

        var config = Substitute.For<IConfiguration>();
        config["S3Storage:BucketName"].Returns("test-bucket");

        var service = new BulkRatesService(
            fpsClient, s3, config, NullLogger<BulkRatesService>.Instance);

        return (service, fpsBulkRates, s3);
    }

    [Fact]
    public async Task UploadFileAsync_WhenMainOperationFails_DoesNotAttemptS3Upload()
    {
        var failure = ApiResponseDto<BulkRatesUploadResultDto>.FailureResponse(
            [new ApiErrorDto { Code = "INVALID_STATUS_FOR_UPLOAD", Message = "wrong status" }],
            new ApiMetaDto());
        var (service, _, s3) = CreateService(failure);

        await service.UploadFileAsync(JobExecutionId, FileBytes, FileName);

        await s3.DidNotReceive().UploadFileAsync(
            Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadFileAsync_WhenMainOperationSucceeds_AttemptsS3UploadAfterward()
    {
        var (service, _, s3) = CreateService();

        await service.UploadFileAsync(JobExecutionId, FileBytes, FileName);

        await s3.Received(1).UploadFileAsync(
            Arg.Any<Stream>(), "test-bucket", Arg.Is<string>(f => f.Contains(JobExecutionId.ToString())),
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadFileAsync_WhenS3UploadFails_StillReturnsTheSuccessfulMainResponse()
    {
        var (service, _, s3) = CreateService();
        s3.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(S3UploadResult.FailureResponse("S3_BUCKET_NOT_FOUND", "Bucket missing"));

        var result = await service.UploadFileAsync(JobExecutionId, FileBytes, FileName);

        // S3 audit failure must not affect the already-successful main operation.
        Assert.True(result.Success);
    }

    [Fact]
    public async Task UploadFileAsync_WhenS3UploadThrows_StillReturnsTheSuccessfulMainResponse()
    {
        var (service, _, s3) = CreateService();
        s3.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("network error"));

        var result = await service.UploadFileAsync(JobExecutionId, FileBytes, FileName);

        // An S3 exception must be swallowed, not surfaced to the caller.
        Assert.True(result.Success);
    }

    [Fact]
    public async Task UploadFileAsync_ReturnsTheMainOperationResponseUnchanged()
    {
        var expected = ApiResponseDto<BulkRatesUploadResultDto>.SuccessResponse(
            new BulkRatesUploadResultDto { JobQueueId = Guid.NewGuid(), Status = "Initiated" });
        var (service, _, _) = CreateService(expected);

        var result = await service.UploadFileAsync(JobExecutionId, FileBytes, FileName);

        Assert.Same(expected, result);
    }
}
