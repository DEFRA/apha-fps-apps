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
    }
}
