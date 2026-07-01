using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.WorkGroupReportServiceTest
{
    public class WorkGroupReportServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactWorkGroupReportApiClient _pactWorkGroupReportApiClient;
        private readonly WorkGroupReportService _service;

        public WorkGroupReportServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactWorkGroupReportApiClient = Substitute.For<IPactWorkGroupReportApiClient>();
            _pactClient.PactWorkGroupReport.Returns(_pactWorkGroupReportApiClient);
            _service = new WorkGroupReportService(_pactClient);
        }

        #region SendEmailsAsync Tests

        [Fact]
        public async Task SendEmailsAsync_WithValidInput_ReturnsSendResultList()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            var results = new List<WorkGroupReportEmailResultDto>
            {
                new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                new() { WorkGroupName = "WG002", EmailRecipient = "b@example.com", Status = "Sent" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(results);
            _pactWorkGroupReportApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _service.SendEmailsAsync(profitCentre, monthNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupReportApiClient.Received(1).SendEmailsAsync(profitCentre, monthNumber);
        }

        [Fact]
        public async Task SendEmailsAsync_WithEmptyResultList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 1;
            var expectedResponse = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(
                new List<WorkGroupReportEmailResultDto>());
            _pactWorkGroupReportApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _service.SendEmailsAsync(profitCentre, monthNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SendEmailsAsync_WithPartialFailureInResults_ReturnsAllResults()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 4;
            var results = new List<WorkGroupReportEmailResultDto>
            {
                new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                new() { WorkGroupName = "WG002", EmailRecipient = null,            Status = "Failed", Reason = "No recipient" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(results);
            _pactWorkGroupReportApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _service.SendEmailsAsync(profitCentre, monthNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("Failed", result.Data![1].Status);
            Assert.Equal("No recipient", result.Data![1].Reason);
        }

        [Fact]
        public async Task SendEmailsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            var errors = new List<ApiErrorDto> { new() { Message = "Send Failed", Code = "SEND_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupReportApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _service.SendEmailsAsync(profitCentre, monthNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region ExportCos90sAsync Tests

        [Fact]
        public async Task ExportCos90sAsync_WithValidInput_ReturnsExportResult()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            const short year = 2025;
            const string pactId = "S001";
            var exportResult = new WorkGroupCos90SExportResultDto
            {
                Rows =
                [
                    new WorkGroupCos90SExportRowDto
                    {
                        WorkGroupName = "WG001",
                        ProfitCentre = "PC001",
                        PactId = "S001",
                        StaffName = "John Smith",
                        TimeCode = "TC01",
                        Description = "Time code description",
                        ParentProject = "PP001",
                        GradeCode = "G7",
                        SpNumber = "SP123",
                        Hours = 7.5,
                        Month = 3,
                        Year = 2025
                    }
                ]
            };
            var expectedResponse = ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(exportResult);
            _pactWorkGroupReportApiClient.ExportCos90sAsync(profitCentre, monthNumber, year, pactId).Returns(expectedResponse);

            // Act
            var result = await _service.ExportCos90sAsync(profitCentre, monthNumber, year, pactId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!.Rows);
            Assert.Equal("WG001", result.Data.Rows[0].WorkGroupName);
            Assert.Equal("PC001", result.Data.Rows[0].ProfitCentre);
            Assert.Equal("S001", result.Data.Rows[0].PactId);
            Assert.Equal("John Smith", result.Data.Rows[0].StaffName);
            Assert.Equal("TC01", result.Data.Rows[0].TimeCode);
            Assert.Equal("Time code description", result.Data.Rows[0].Description);
            Assert.Equal("PP001", result.Data.Rows[0].ParentProject);
            Assert.Equal("G7", result.Data.Rows[0].GradeCode);
            Assert.Equal("SP123", result.Data.Rows[0].SpNumber);
            Assert.Equal(7.5, result.Data.Rows[0].Hours);
            Assert.Equal((short)3, result.Data.Rows[0].Month);
            Assert.Equal((short)2025, result.Data.Rows[0].Year);
            await _pactWorkGroupReportApiClient.Received(1).ExportCos90sAsync(profitCentre, monthNumber, year, pactId);
        }

        [Fact]
        public async Task ExportCos90sAsync_WithNoRows_ReturnsSuccessWithEmptyRows()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            const short year = 2025;
            var exportResult = new WorkGroupCos90SExportResultDto { Rows = [] };
            var expectedResponse = ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(exportResult);
            _pactWorkGroupReportApiClient.ExportCos90sAsync(profitCentre, monthNumber, year, null).Returns(expectedResponse);

            // Act
            var result = await _service.ExportCos90sAsync(profitCentre, monthNumber, year, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!.Rows);
        }

        [Fact]
        public async Task ExportCos90sAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 3;
            const short year = 2025;
            var errors = new List<ApiErrorDto> { new() { Message = "Export failed", Code = "EXPORT_ERROR" } };
            var expectedResponse = ApiResponseDto<WorkGroupCos90SExportResultDto>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupReportApiClient.ExportCos90sAsync(profitCentre, monthNumber, year, null).Returns(expectedResponse);

            // Act
            var result = await _service.ExportCos90sAsync(profitCentre, monthNumber, year, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
