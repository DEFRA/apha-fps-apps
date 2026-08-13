using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Infrastructure.Mappings;
using AutoMapper;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Mappings
{
    /// <summary>
    /// Guards the Common Res -> Web DTO boundary (Apha.FPSApps.Infrastructure side of the
    /// shared-contract migration) — the FPSApps counterpart to Apha.FPS.Api.UnitTests'
    /// RequestMapperTests. <see cref="Configuration_IsValid"/> is deliberately scoped to only the
    /// Bulk Rates maps (not the full <see cref="FpsApiDtoMapper"/> profile), for the same reason
    /// as the API-side test: the whole profile has pre-existing, unrelated gaps.
    /// </summary>
    public class FpsApiDtoMapperTests
    {
        private readonly IMapper _mapper;

        public FpsApiDtoMapperTests()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BulkRatesQueueEntryRes, BulkRatesQueueEntryDto>();
                cfg.CreateMap<BulkRatesUploadMetadataRes, BulkRatesUploadMetadataDto>();
                cfg.CreateMap<BulkRatesRowCountsRes, BulkRatesRowCountsDto>();
                cfg.CreateMap<BulkRatesQueueLogRes, BulkRatesQueueLogDto>();
                cfg.CreateMap<BulkRatesValidationErrorRes, BulkRatesValidationErrorDto>();
                cfg.CreateMap<BulkRatesFecStagingRowRes, BulkRatesFecStagingRowDto>();
                cfg.CreateMap<BulkRatesAgrupStagingRowRes, BulkRatesAgrupStagingRowDto>();
                cfg.CreateMap<BulkRatesAnimalStagingRowRes, BulkRatesAnimalStagingRowDto>();
                cfg.CreateMap<BulkRatesStaffStagingRowRes, BulkRatesStaffStagingRowDto>();
                cfg.CreateMap<BulkRatesRequestDetailRes, BulkRatesRequestDetailDto>();
                cfg.CreateMap<BulkRatesUploadResultRes, BulkRatesUploadResultDto>();
                cfg.CreateMap<BulkRatesStagingDataRes, BulkRatesStagingDataDto>();
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<BulkRatesQueueEntryRes, BulkRatesQueueEntryDto>();
                cfg.CreateMap<BulkRatesUploadMetadataRes, BulkRatesUploadMetadataDto>();
                cfg.CreateMap<BulkRatesRowCountsRes, BulkRatesRowCountsDto>();
                cfg.CreateMap<BulkRatesQueueLogRes, BulkRatesQueueLogDto>();
                cfg.CreateMap<BulkRatesValidationErrorRes, BulkRatesValidationErrorDto>();
                cfg.CreateMap<BulkRatesFecStagingRowRes, BulkRatesFecStagingRowDto>();
                cfg.CreateMap<BulkRatesAgrupStagingRowRes, BulkRatesAgrupStagingRowDto>();
                cfg.CreateMap<BulkRatesAnimalStagingRowRes, BulkRatesAnimalStagingRowDto>();
                cfg.CreateMap<BulkRatesStaffStagingRowRes, BulkRatesStaffStagingRowDto>();
                cfg.CreateMap<BulkRatesRequestDetailRes, BulkRatesRequestDetailDto>();
                cfg.CreateMap<BulkRatesUploadResultRes, BulkRatesUploadResultDto>();
                cfg.CreateMap<BulkRatesStagingDataRes, BulkRatesStagingDataDto>();
            }, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void BulkRatesRequestDetailRes_MapsToWebDto_WithActiveDownloadVersionPreserved()
        {
            var source = new BulkRatesRequestDetailRes
            {
                Entry = new BulkRatesQueueEntryRes
                {
                    JobQueueId = Guid.NewGuid(),
                    Status = "Initiated",
                    ActiveDownloadVersion = 7,
                },
                UploadMetadata = new BulkRatesUploadMetadataRes
                {
                    Filename = "rates.xlsx",
                    RowCounts = new BulkRatesRowCountsRes { Total = 5, Unchanged = 1 },
                },
                Log = [new BulkRatesQueueLogRes { LogId = 99, Note = "log note" }],
                ErrorCount = 1,
                WarningCount = 2,
            };

            var dto = _mapper.Map<BulkRatesRequestDetailDto>(source);

            Assert.Equal(source.Entry.JobQueueId, dto.Entry.JobQueueId);
            Assert.Equal("Initiated", dto.Entry.Status);
            Assert.Equal(7, dto.Entry.ActiveDownloadVersion);
            Assert.NotNull(dto.UploadMetadata);
            Assert.Equal("rates.xlsx", dto.UploadMetadata!.Filename);
            Assert.Equal(5, dto.UploadMetadata.RowCounts.Total);
            Assert.Single(dto.Log);
            Assert.Equal(99, dto.Log[0].LogId);
            Assert.Equal(1, dto.ErrorCount);
            Assert.Equal(2, dto.WarningCount);
        }

        [Fact]
        public void BulkRatesUploadResultRes_MapsToWebDto_DroppingIdJobQueueIdAndUploadVersionFromErrors()
        {
            var source = new BulkRatesUploadResultRes
            {
                Status = "Initiated",
                RowCounts = new BulkRatesRowCountsRes { Total = 5 },
                ValidationErrors =
                [
                    new BulkRatesValidationErrorRes
                    {
                        Id = 55,
                        JobQueueId = Guid.NewGuid(),
                        UploadVersion = 3,
                        SourceRowNumber = 2,
                        Severity = "Error",
                        ValidationMessage = "bad row",
                    },
                ],
            };

            var dto = _mapper.Map<BulkRatesUploadResultDto>(source);

            Assert.Equal("Initiated", dto.Status);
            Assert.Equal(5, dto.RowCounts.Total);
            var error = Assert.Single(dto.ValidationErrors);
            Assert.Equal(2, error.SourceRowNumber);
            Assert.Equal("bad row", error.ValidationMessage);
        }

        [Fact]
        public void BulkRatesStagingDataRes_MapsToWebDto_WithAllFourRowSetsPreserved()
        {
            var source = new BulkRatesStagingDataRes
            {
                FecRows = [new BulkRatesFecStagingRowRes { TestCode = "T1", Status = "Updated" }],
                AgrupRows = [new BulkRatesAgrupStagingRowRes { TestCode = "T1", Buyer = "B1" }],
                StaffRows = [new BulkRatesStaffStagingRowRes { PcGrade = "G1" }],
                AnimalRows = [new BulkRatesAnimalStagingRowRes { AnimalType = "A1" }],
            };

            var dto = _mapper.Map<BulkRatesStagingDataDto>(source);

            Assert.Single(dto.FecRows);
            Assert.Equal("T1", dto.FecRows[0].TestCode);
            Assert.Single(dto.AgrupRows);
            Assert.Single(dto.StaffRows);
            Assert.Single(dto.AnimalRows);
        }
    }
}
