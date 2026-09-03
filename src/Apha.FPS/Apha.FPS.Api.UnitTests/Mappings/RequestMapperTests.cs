using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Mappings;
using Apha.FPS.Application.Dtos.BulkRates;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.FPS.Api.UnitTests.Mappings
{
    /// <summary>
    /// Guards the Application DTO -> Common Res boundary (Apha.FPS.Api side of the shared-contract
    /// migration). <see cref="IConfigurationProvider.AssertConfigurationIsValid"/> proves every
    /// registered map is structurally possible (destination properties are resolvable); it does
    /// not prove the values actually reach their destination correctly, so the populated-instance
    /// tests below exist alongside it — mirroring the reasoning behind
    /// BulkRatesRequestServiceTests.GetRequest_MapsEveryEntryLogAndMetadataFieldToTheApiDto.
    ///
    /// <see cref="Configuration_IsValid"/> deliberately validates only the Bulk Rates maps, not
    /// the full <see cref="RequestMapper"/> profile — running AssertConfigurationIsValid against
    /// the whole profile surfaces pre-existing, unrelated gaps (e.g. StaffJobDto -> StaffJobRes,
    /// AnimalReq -> AnimalDto) that predate this migration and are not this test's concern.
    /// </summary>
    public class RequestMapperTests
    {
        private readonly IMapper _mapper;

        public RequestMapperTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(RequestMapper)));
            _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BulkRatesQueueEntryDto, BulkRatesQueueEntryRes>();
                cfg.CreateMap<BulkRatesUploadMetadataDto, BulkRatesUploadMetadataRes>();
                cfg.CreateMap<BulkRatesRowCountsDto, BulkRatesRowCountsRes>();
                cfg.CreateMap<BulkRatesQueueLogDto, BulkRatesQueueLogRes>();
                cfg.CreateMap<BulkRatesValidationErrorDto, BulkRatesValidationErrorRes>();
                cfg.CreateMap<BulkRatesFecStagingRowDto, BulkRatesFecStagingRowRes>();
                cfg.CreateMap<BulkRatesAgrupStagingRowDto, BulkRatesAgrupStagingRowRes>();
                cfg.CreateMap<BulkRatesAnimalStagingRowDto, BulkRatesAnimalStagingRowRes>();
                cfg.CreateMap<BulkRatesStaffStagingRowDto, BulkRatesStaffStagingRowRes>();
                cfg.CreateMap<BulkRatesRequestDto, BulkRatesRequestDetailRes>();
                cfg.CreateMap<BulkRatesUploadResultDto, BulkRatesUploadResultRes>();
                cfg.CreateMap<BulkRatesStagingDataDto, BulkRatesStagingDataRes>();
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            config.Invoking(c => c.AssertConfigurationIsValid()).Should().NotThrow();
        }

        [Fact]
        public void BulkRatesRequestDto_MapsToRequestDetailRes_WithEveryFieldPreserved()
        {
            var source = new BulkRatesRequestDto
            {
                Entry = new BulkRatesQueueEntryDto
                {
                    JobQueueId = Guid.NewGuid(),
                    JobId = 6,
                    JobName = "BulkTestRatesUpdate",
                    StatusId = 42,
                    Status = "Initiated",
                    JobExecutionId = Guid.NewGuid(),
                    RequestedBy = "alice@test.com",
                    RequestedAtUtc = new DateTime(2027, 1, 1, 9, 0, 0, DateTimeKind.Utc),
                    FpsYear = 2027,
                    ActiveDownloadVersion = 7,
                },
                UploadMetadata = new BulkRatesUploadMetadataDto
                {
                    Filename = "rates.xlsx",
                    UploadVersion = 3,
                    RowCounts = new BulkRatesRowCountsDto { Total = 5, Valid = 4, Invalid = 1, Insert = 2, Update = 2, Unchanged = 1 },
                },
                Log =
                [
                    new BulkRatesQueueLogDto { LogId = 99, Note = "log note", Actor = "eve@test.com" },
                ],
                ErrorCount = 1,
                WarningCount = 2,
            };

            var res = _mapper.Map<BulkRatesRequestDetailRes>(source);

            res.Entry.JobQueueId.Should().Be(source.Entry.JobQueueId);
            res.Entry.JobName.Should().Be("BulkTestRatesUpdate");
            res.Entry.Status.Should().Be("Initiated");
            res.Entry.ActiveDownloadVersion.Should().Be(7);
            res.UploadMetadata.Should().NotBeNull();
            res.UploadMetadata!.Filename.Should().Be("rates.xlsx");
            res.UploadMetadata.RowCounts.Total.Should().Be(5);
            res.UploadMetadata.RowCounts.Unchanged.Should().Be(1);
            res.Log.Should().ContainSingle(l => l.LogId == 99 && l.Note == "log note" && l.Actor == "eve@test.com");
            res.ErrorCount.Should().Be(1);
            res.WarningCount.Should().Be(2);
        }

        [Fact]
        public void BulkRatesUploadResultDto_MapsToUploadResultRes_WithValidationErrorsPreserved()
        {
            var source = new BulkRatesUploadResultDto
            {
                JobQueueId = Guid.NewGuid(),
                Status = "Initiated",
                UploadVersion = 3,
                Filename = "rates.xlsx",
                RowCounts = new BulkRatesRowCountsDto { Total = 5 },
                ValidationErrors =
                [
                    new BulkRatesValidationErrorDto
                    {
                        Id = 55,
                        SourceRowNumber = 2,
                        Severity = "Error",
                        ValidationMessage = "Negative rates are not permitted.",
                        IsRequestLevel = false,
                    },
                ],
            };

            var res = _mapper.Map<BulkRatesUploadResultRes>(source);

            res.Status.Should().Be("Initiated");
            res.RowCounts.Total.Should().Be(5);
            res.ValidationErrors.Should().ContainSingle(e =>
                e.Id == 55 && e.SourceRowNumber == 2 && e.Severity == "Error" &&
                e.ValidationMessage == "Negative rates are not permitted." && !e.IsRequestLevel);
        }

        [Fact]
        public void BulkRatesStagingDataDto_MapsToStagingDataRes_WithAllFourRowSetsPreserved()
        {
            var source = new BulkRatesStagingDataDto
            {
                FecRows = [new BulkRatesFecStagingRowDto { TestCode = "T1", Status = "Updated" }],
                AgrupRows = [new BulkRatesAgrupStagingRowDto { TestCode = "T1", Buyer = "B1" }],
                StaffRows = [new BulkRatesStaffStagingRowDto { PcGrade = "G1" }],
                AnimalRows = [new BulkRatesAnimalStagingRowDto { AnimalType = "A1" }],
            };

            var res = _mapper.Map<BulkRatesStagingDataRes>(source);

            res.FecRows.Should().ContainSingle(r => r.TestCode == "T1" && r.Status == "Updated");
            res.AgrupRows.Should().ContainSingle(r => r.TestCode == "T1" && r.Buyer == "B1");
            res.StaffRows.Should().ContainSingle(r => r.PcGrade == "G1");
            res.AnimalRows.Should().ContainSingle(r => r.AnimalType == "A1");
        }
    }
}
