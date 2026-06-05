using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.WorkGroupReportEmailServiceTest
{
    public class WorkGroupReportEmailServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactWorkGroupReportEmailApiClient _pactWorkGroupReportEmailApiClient;
        private readonly WorkGroupReportEmailService _service;

        public WorkGroupReportEmailServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactWorkGroupReportEmailApiClient = Substitute.For<IPactWorkGroupReportEmailApiClient>();
            _pactClient.PactWorkGroupReportEmail.Returns(_pactWorkGroupReportEmailApiClient);
            _service = new WorkGroupReportEmailService(_pactClient);
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
            _pactWorkGroupReportEmailApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

            // Act
            var result = await _service.SendEmailsAsync(profitCentre, monthNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupReportEmailApiClient.Received(1).SendEmailsAsync(profitCentre, monthNumber);
        }

        [Fact]
        public async Task SendEmailsAsync_WithEmptyResultList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short monthNumber = 1;
            var expectedResponse = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(
                new List<WorkGroupReportEmailResultDto>());
            _pactWorkGroupReportEmailApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

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
            _pactWorkGroupReportEmailApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

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
            _pactWorkGroupReportEmailApiClient.SendEmailsAsync(profitCentre, monthNumber).Returns(expectedResponse);

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
