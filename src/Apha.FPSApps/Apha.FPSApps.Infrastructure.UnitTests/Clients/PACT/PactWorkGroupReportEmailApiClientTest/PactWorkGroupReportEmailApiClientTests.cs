using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactWorkGroupReportEmailApiClientTest
{
    public class PactWorkGroupReportEmailApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactWorkGroupReportEmailApiClient _client;

        public PactWorkGroupReportEmailApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactWorkGroupReportEmailApiClient(_http, _mapper);
        }

        #region SendEmailsAsync Tests

        [Fact]
        public async Task SendEmailsAsync_WithSuccessResponse_ReturnsMappedResultList()
        {
            // Arrange
            var resList = new List<WorkGroupReportEmailResultRes>
            {
                new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                new() { WorkGroupName = "WG002", EmailRecipient = "b@example.com", Status = "Sent" }
            };
            var apiResponse = new ApiResponse<List<WorkGroupReportEmailResultRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(
                new List<WorkGroupReportEmailResultDto>
                {
                    new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                    new() { WorkGroupName = "WG002", EmailRecipient = "b@example.com", Status = "Sent" }
                });

            _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                PactApiEndpoints.SendEmails, Arg.Any<WorkGroupReportEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SendEmailsAsync("PC001", 3);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                PactApiEndpoints.SendEmails, Arg.Any<WorkGroupReportEmailReq>());
        }

        [Fact]
        public async Task SendEmailsAsync_SendsCorrectRequestPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupReportEmailResultRes>> { Success = true, Data = new List<WorkGroupReportEmailResultRes>() };
            var expectedDto = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(new List<WorkGroupReportEmailResultDto>());

            WorkGroupReportEmailReq? capturedRequest = null;
            _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                Arg.Any<string>(),
                Arg.Do<WorkGroupReportEmailReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.SendEmailsAsync("PC001", 6);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("PC001", capturedRequest!.ProfitCentre);
            Assert.Equal((short)6, capturedRequest.MonthNumber);
        }

        [Fact]
        public async Task SendEmailsAsync_WithEmptyResultList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupReportEmailResultRes>> { Success = true, Data = new List<WorkGroupReportEmailResultRes>() };
            var expectedDto = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(new List<WorkGroupReportEmailResultDto>());

            _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                Arg.Any<string>(), Arg.Any<WorkGroupReportEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SendEmailsAsync("PC001", 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SendEmailsAsync_WithPartialFailureInResults_ReturnsAllResults()
        {
            // Arrange
            var resList = new List<WorkGroupReportEmailResultRes>
            {
                new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                new() { WorkGroupName = "WG002", EmailRecipient = null,            Status = "Failed", Reason = "No recipient" }
            };
            var apiResponse = new ApiResponse<List<WorkGroupReportEmailResultRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<WorkGroupReportEmailResultDto>>.SuccessResponse(
                new List<WorkGroupReportEmailResultDto>
                {
                    new() { WorkGroupName = "WG001", EmailRecipient = "a@example.com", Status = "Sent" },
                    new() { WorkGroupName = "WG002", EmailRecipient = null,            Status = "Failed", Reason = "No recipient" }
                });

            _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                Arg.Any<string>(), Arg.Any<WorkGroupReportEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SendEmailsAsync("PC001", 4);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("Failed", result.Data![1].Status);
            Assert.Equal("No recipient", result.Data![1].Reason);
        }

        [Fact]
        public async Task SendEmailsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Send Failed", Code = "SEND_ERROR" } };
            var apiResponse = new ApiResponse<List<WorkGroupReportEmailResultRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<WorkGroupReportEmailResultDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Send Failed", Code = "SEND_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<WorkGroupReportEmailReq, List<WorkGroupReportEmailResultRes>>(
                Arg.Any<string>(), Arg.Any<WorkGroupReportEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupReportEmailResultDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SendEmailsAsync("PC001", 3);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
