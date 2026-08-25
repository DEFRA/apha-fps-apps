using System.Reflection;
using Apha.FPS.Application.Dtos.BulkRates;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Services.BulkRatesServiceTest
{
    /// <summary>
    /// Guards the API-boundary correction: BulkRatesRequestDto/BulkRatesUploadResultDto must
    /// never serialize Core.Entities types directly. Each nested Dto's public shape is asserted
    /// against an explicit, approved property list — NOT against its corresponding Core entity.
    ///
    /// Deliberately not an Entity-vs-Dto equivalence check: that would re-couple the two layers
    /// this refactor separated. A future, legitimate entity-only field (e.g. an internal
    /// processing note never meant for the wire) must be free to exist without failing this
    /// test. Only a change to the Dto itself — an intentional API-contract change — should
    /// require touching the expected list below.
    /// </summary>
    public class BulkRatesApiContractDtoTests
    {
        [Fact]
        public void BulkRatesQueueEntryDto_HasTheApprovedContractShape()
        {
            AssertContract<BulkRatesQueueEntryDto>(new Dictionary<string, Type>
            {
                [nameof(BulkRatesQueueEntryDto.JobQueueId)] = typeof(Guid),
                [nameof(BulkRatesQueueEntryDto.JobId)] = typeof(int),
                [nameof(BulkRatesQueueEntryDto.JobName)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.StatusId)] = typeof(int),
                [nameof(BulkRatesQueueEntryDto.Status)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.JobExecutionId)] = typeof(Guid),
                [nameof(BulkRatesQueueEntryDto.RequestedBy)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.RequestedAtUtc)] = typeof(DateTime),
                [nameof(BulkRatesQueueEntryDto.FpsYear)] = typeof(int),
                [nameof(BulkRatesQueueEntryDto.UploadFilename)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.UploadChecksumSha256)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.UploadVersion)] = typeof(int?),
                [nameof(BulkRatesQueueEntryDto.UploadValidatedAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.UploadRowCountsJson)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.ApprovedBy)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.ApprovedAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.RejectedBy)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.RejectedAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.RejectionReason)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.CancelledBy)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.CancelledAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.CancellationReason)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.TriggeredBy)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.TriggeredAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.StartDateTime)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.EndDateTime)] = typeof(DateTime?),
                [nameof(BulkRatesQueueEntryDto.ErrorMessage)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.FailureReason)] = typeof(string),
                [nameof(BulkRatesQueueEntryDto.ActiveDownloadVersion)] = typeof(int?),
                [nameof(BulkRatesQueueEntryDto.S3ObjectKey)] = typeof(string),
            });
        }

        [Fact]
        public void BulkRatesUploadMetadataDto_HasTheApprovedContractShape()
        {
            AssertContract<BulkRatesUploadMetadataDto>(new Dictionary<string, Type>
            {
                [nameof(BulkRatesUploadMetadataDto.Filename)] = typeof(string),
                [nameof(BulkRatesUploadMetadataDto.ChecksumSha256)] = typeof(string),
                [nameof(BulkRatesUploadMetadataDto.UploadVersion)] = typeof(int),
                [nameof(BulkRatesUploadMetadataDto.ValidationCompletedAtUtc)] = typeof(DateTime?),
                [nameof(BulkRatesUploadMetadataDto.RowCounts)] = typeof(BulkRatesRowCountsDto),
            });
        }

        [Fact]
        public void BulkRatesRowCountsDto_HasTheApprovedContractShape()
        {
            AssertContract<BulkRatesRowCountsDto>(new Dictionary<string, Type>
            {
                [nameof(BulkRatesRowCountsDto.Total)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.Valid)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.Invalid)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.Insert)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.Update)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.Unchanged)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.FecTotal)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.FecInsert)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.FecUpdate)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.FecUnchanged)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.FecInvalid)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.AgrupTotal)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.AgrupInsert)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.AgrupUpdate)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.AgrupUnchanged)] = typeof(int),
                [nameof(BulkRatesRowCountsDto.AgrupInvalid)] = typeof(int),
            });
        }

        [Fact]
        public void BulkRatesQueueLogDto_HasTheApprovedContractShape()
        {
            AssertContract<BulkRatesQueueLogDto>(new Dictionary<string, Type>
            {
                [nameof(BulkRatesQueueLogDto.LogId)] = typeof(long),
                [nameof(BulkRatesQueueLogDto.JobQueueId)] = typeof(Guid),
                [nameof(BulkRatesQueueLogDto.Note)] = typeof(string),
                [nameof(BulkRatesQueueLogDto.Actor)] = typeof(string),
                [nameof(BulkRatesQueueLogDto.CreatedAtUtc)] = typeof(DateTime),
            });
        }

        [Fact]
        public void BulkRatesValidationErrorDto_HasTheApprovedContractShape()
        {
            AssertContract<BulkRatesValidationErrorDto>(new Dictionary<string, Type>
            {
                [nameof(BulkRatesValidationErrorDto.Id)] = typeof(long),
                [nameof(BulkRatesValidationErrorDto.JobQueueId)] = typeof(Guid),
                [nameof(BulkRatesValidationErrorDto.UploadVersion)] = typeof(int),
                [nameof(BulkRatesValidationErrorDto.SourceRowNumber)] = typeof(int),
                [nameof(BulkRatesValidationErrorDto.FieldName)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.ValidationCode)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.Severity)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.ValidationMessage)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.SheetName)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.TestCode)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.Buyer)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.CurrentValue)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.ExpectedValue)] = typeof(string),
                [nameof(BulkRatesValidationErrorDto.IsRequestLevel)] = typeof(bool),
            });
        }

        private static void AssertContract<TDto>(Dictionary<string, Type> approved)
        {
            var actual = typeof(TDto).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name, p => p.PropertyType);

            actual.Keys.Should().BeEquivalentTo(approved.Keys,
                $"{typeof(TDto).Name}'s public API contract changed — update the approved list above " +
                "only if this is an intentional contract change");

            foreach (var (name, expectedType) in approved)
                actual[name].Should().Be(expectedType, $"{typeof(TDto).Name}.{name}'s wire type changed");
        }
    }
}
