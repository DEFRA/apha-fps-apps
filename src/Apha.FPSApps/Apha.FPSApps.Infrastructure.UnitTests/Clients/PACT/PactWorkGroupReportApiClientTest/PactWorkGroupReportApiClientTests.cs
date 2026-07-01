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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactWorkGroupReportApiClientTest
{
    public class PactWorkGroupReportApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactWorkGroupReportApiClient _client;

        public PactWorkGroupReportApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactWorkGroupReportApiClient(_http, _mapper);
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

        #region ExportCos90sAsync Tests

        [Fact]
        public async Task ExportCos90sAsync_WithSuccessResponse_ReturnsMappedExportResult()
        {
            // Arrange
            var responseData = new WorkGroupCos90SExportRes
            {
                Rows = [new WorkGroupCos90SExportRowRes { WorkGroupName = "WG001", StaffName = "John Smith" }]
            };
            var apiResponse = new ApiResponse<WorkGroupCos90SExportRes> { Success = true, Data = responseData };
            var expectedDto = ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(
                new WorkGroupCos90SExportResultDto
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
                });

            _http.PostAsync<WorkGroupCos90SExportReq, WorkGroupCos90SExportRes>(
                PactApiEndpoints.ExportCos90s, Arg.Any<WorkGroupCos90SExportReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupCos90SExportResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.ExportCos90sAsync("PC001", 3, 2025, "S001");

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
            await _http.Received(1).PostAsync<WorkGroupCos90SExportReq, WorkGroupCos90SExportRes>(
                PactApiEndpoints.ExportCos90s, Arg.Any<WorkGroupCos90SExportReq>());
        }

        [Fact]
        public async Task ExportCos90sAsync_WithNullPactId_SendsCorrectRequestPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<WorkGroupCos90SExportRes>
            {
                Success = true,
                Data = new WorkGroupCos90SExportRes { Rows = [] }
            };
            var expectedDto = ApiResponseDto<WorkGroupCos90SExportResultDto>.SuccessResponse(
                new WorkGroupCos90SExportResultDto { Rows = [] });

            WorkGroupCos90SExportReq? capturedRequest = null;
            _http.PostAsync<WorkGroupCos90SExportReq, WorkGroupCos90SExportRes>(
                Arg.Any<string>(),
                Arg.Do<WorkGroupCos90SExportReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupCos90SExportResultDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.ExportCos90sAsync("PC001", 1, 2025, null);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("PC001", capturedRequest!.ProfitCentre);
            Assert.Equal((short)1, capturedRequest.MonthNumber);
            Assert.Equal((short)2025, capturedRequest.Year);
            Assert.Null(capturedRequest.PactId);
        }

        [Fact]
        public async Task ExportCos90sAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<WorkGroupCos90SExportRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "Export failed", Code = "EXPORT_ERROR" }]
            };
            var mappedResponse = new ApiResponseDto<WorkGroupCos90SExportResultDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Export failed", Code = "EXPORT_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<WorkGroupCos90SExportReq, WorkGroupCos90SExportRes>(
                Arg.Any<string>(), Arg.Any<WorkGroupCos90SExportReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<WorkGroupCos90SExportResultDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.ExportCos90sAsync("PC001", 3, 2025, "S001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
