using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactWorkGroupApiClient _pactWorkGroupApiClient;
        private readonly WorkGroupService _service;

        public WorkGroupServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactWorkGroupApiClient = Substitute.For<IPactWorkGroupApiClient>();
            _pactClient.PactWorkGroup.Returns(_pactWorkGroupApiClient);
            _service = new WorkGroupService(_pactClient);
        }

        #region GetAllWorkGroupsAsync Tests

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithSuccessResponse_ReturnsWorkGroupList()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = "PC001" },
                new() { WorkGroupName = "WG002", ProfitCentre = "PC001" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups);
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupApiClient.Received(1).GetAllWorkGroupsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetAllWorkGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetWorkGroupsByProfitCentreAsync Tests

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithValidInput_ReturnsPagedWorkGroups()
        {
            // Arrange
            const string profitCentre = "PC001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = profitCentre },
                new() { WorkGroupName = "WG002", ProfitCentre = profitCentre }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups);
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactWorkGroupApiClient.Received(1).GetWorkGroupsByProfitCentreAsync(query, profitCentre);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithNoMatchingWorkGroups_ReturnsEmptyList()
        {
            // Arrange
            const string profitCentre = "PC_NONE";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, profitCentre).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.GetWorkGroupsByProfitCentreAsync(query, Arg.Any<string>()).Returns(expectedResponse);

            // Act
            var result = await _service.GetWorkGroupsByProfitCentreAsync(query, "PC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithValidInput_ReturnsSuccessTrue()
        {
            // Arrange
            const string profitCentre = "PC001";
            const short flag = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient
                .SetSendEmailForProfitCentreWorkGroupsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithClearFlag_ReturnsSuccessTrue()
        {
            // Arrange
            const short flag = 0;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.SetSendEmailForAllWorkGroupsAsync(flag).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForAllWorkGroupsAsync(flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).SetSendEmailForAllWorkGroupsAsync(flag);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient.SetSendEmailForAllWorkGroupsAsync(Arg.Any<short>()).Returns(expectedResponse);

            // Act
            var result = await _service.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region UpdateWorkGroupEmailAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithValidInput_ReturnsSuccessTrue()
        {
            // Arrange
            const string workGroupName = "WG001";
            const short sendEmail = 1;
            const string emailRecipient = "test@example.com";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactWorkGroupApiClient.Received(1).UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithNullEmailRecipient_ReturnsSuccessTrue()
        {
            // Arrange
            const string workGroupName = "WG001";
            const short sendEmail = 0;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactWorkGroupApiClient.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pactWorkGroupApiClient.Received(1).UpdateWorkGroupEmailAsync(workGroupName, sendEmail, null);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactWorkGroupApiClient
                .UpdateWorkGroupEmailAsync(Arg.Any<string>(), Arg.Any<short>(), Arg.Any<string?>())
                .Returns(expectedResponse);

            // Act
            var result = await _service.UpdateWorkGroupEmailAsync("WG001", 1, "test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
