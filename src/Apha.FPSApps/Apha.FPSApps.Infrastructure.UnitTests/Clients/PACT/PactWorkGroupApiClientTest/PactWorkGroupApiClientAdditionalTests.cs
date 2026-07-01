using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactWorkGroupApiClientTest
{
    public class PactWorkGroupApiClientAdditionalTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactWorkGroupApiClient _client;

        public PactWorkGroupApiClientAdditionalTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactWorkGroupApiClient(_http, _mapper);
        }

        #region GetWorkGroupsByProfitCentreAsync Tests

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithSuccessResponse_ReturnsMappedWorkGroups()
        {
            // Arrange
            const string profitCentre = "PC001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<WorkGroupRes>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = profitCentre },
                new() { WorkGroupName = "WG002", ProfitCentre = profitCentre }
            };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                new List<WorkGroupDto>
                {
                    new() { WorkGroupName = "WG001", ProfitCentre = profitCentre },
                    new() { WorkGroupName = "WG002", ProfitCentre = profitCentre }
                });

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupsByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<WorkGroupRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = new List<WorkGroupRes>() };
            var expectedDto = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupsByProfitCentreAsync(query, "PC001");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupsByProfitCentreAsync(new QueryParameters<string>(), "PC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region SetSendEmailForProfitCentreWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForProfitCentreWorkGroups, Arg.Any<UpdateSendEmailFlagReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForProfitCentreWorkGroups, Arg.Any<UpdateSendEmailFlagReq>());
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_SendsCorrectRequestPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            UpdateSendEmailFlagReq? capturedRequest = null;
            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                Arg.Any<string>(),
                Arg.Do<UpdateSendEmailFlagReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 1);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("PC001", capturedRequest!.ProfitCentre);
            Assert.Equal((short)1, capturedRequest.SendEmail);
        }

        [Fact]
        public async Task SetSendEmailForProfitCentreWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateSendEmailFlagReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SetSendEmailForProfitCentreWorkGroupsAsync("PC001", 0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region SetSendEmailForAllWorkGroupsAsync Tests

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForAllWorkGroups, Arg.Any<UpdateSendEmailFlagReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<UpdateSendEmailFlagReq, bool?>(
                PactApiEndpoints.SetSendEmailForAllWorkGroups, Arg.Any<UpdateSendEmailFlagReq>());
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_SendsCorrectFlagInPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            UpdateSendEmailFlagReq? capturedRequest = null;
            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(
                Arg.Any<string>(),
                Arg.Do<UpdateSendEmailFlagReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal((short)0, capturedRequest!.SendEmail);
            Assert.Null(capturedRequest.ProfitCentre);
        }

        [Fact]
        public async Task SetSendEmailForAllWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<UpdateSendEmailFlagReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateSendEmailFlagReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SetSendEmailForAllWorkGroupsAsync(0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateWorkGroupEmailAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateWorkGroupEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateWorkGroupEmailAsync("WG001", 1, "test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<UpdateWorkGroupEmailReq, bool?>(
                Arg.Any<string>(), Arg.Any<UpdateWorkGroupEmailReq>());
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_SendsCorrectRequestPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            UpdateWorkGroupEmailReq? capturedRequest = null;
            _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(
                Arg.Any<string>(),
                Arg.Do<UpdateWorkGroupEmailReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateWorkGroupEmailAsync("WG001", 1, "test@example.com");

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Equal("WG001", capturedRequest!.WorkGroupName);
            Assert.Equal((short)1, capturedRequest.SendEmail);
            Assert.Equal("test@example.com", capturedRequest.EmailRecipient);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WithNullEmailRecipient_SendsNullInPayload()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            UpdateWorkGroupEmailReq? capturedRequest = null;
            _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(
                Arg.Any<string>(),
                Arg.Do<UpdateWorkGroupEmailReq>(r => capturedRequest = r))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateWorkGroupEmailAsync("WG001", 0, null);

            // Assert
            Assert.NotNull(capturedRequest);
            Assert.Null(capturedRequest!.EmailRecipient);
        }

        [Fact]
        public async Task UpdateWorkGroupEmailAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update Failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<UpdateWorkGroupEmailReq, bool?>(Arg.Any<string>(), Arg.Any<UpdateWorkGroupEmailReq>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateWorkGroupEmailAsync("WG001", 1, "test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region Cos90 Methods Tests

        [Fact]
        public async Task GetWorkGroupsFlaggedForCos90Async_WithSuccessResponse_ReturnsMappedWorkGroups()
        {
            // Arrange
            var resList = new List<WorkGroupRes>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = "PC001" }
            };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [new WorkGroupDto { WorkGroupName = "WG001", ProfitCentre = "PC001" }]);

            _http.GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetWorkGroupsFlaggedForCos90).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupsFlaggedForCos90Async();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<WorkGroupRes>>(PactApiEndpoints.GetWorkGroupsFlaggedForCos90);
        }

        [Fact]
        public async Task GetWorkGroupsFlaggedForCos90Async_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = false, Errors = [new ApiError { Message = "API Error", Code = "API_ERROR" }] };
            var mappedResponse = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupsFlaggedForCos90Async();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task SetCos90ForProfitCentreWorkGroupsAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            // Arrange
            const string profitCentre = "PC 001";
            const short flag = 1;
            var expectedUrl = string.Format(PactApiEndpoints.SetCos90ForProfitCentreWorkGroups, Uri.EscapeDataString(profitCentre), flag);
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<object, bool?>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SetCos90ForProfitCentreWorkGroupsAsync(profitCentre, flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PutAsync<object, bool?>(expectedUrl, Arg.Any<object>());
        }

        [Fact]
        public async Task SetCos90ForProfitCentreWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Failed", Code = "FAILED" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Failed", Code = "FAILED" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<object, bool?>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SetCos90ForProfitCentreWorkGroupsAsync("PC001", 0);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task SetCos90ForAllWorkGroupsAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            // Arrange
            const short flag = 1;
            var expectedUrl = string.Format(PactApiEndpoints.SetCos90ForAllWorkGroups, flag);
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<object, bool?>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SetCos90ForAllWorkGroupsAsync(flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PutAsync<object, bool?>(expectedUrl, Arg.Any<object>());
        }

        [Fact]
        public async Task SetCos90ForAllWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Failed", Code = "FAILED" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Failed", Code = "FAILED" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<object, bool?>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SetCos90ForAllWorkGroupsAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task SetCos90ForWorkGroupAsync_WithSuccessResponse_ReturnsMappedResult()
        {
            // Arrange
            const string profitCentre = "PC 001";
            const string workGroupName = "WG 001";
            const short flag = 1;
            var expectedUrl = string.Format(PactApiEndpoints.SetCos90ForWorkGroup,
                Uri.EscapeDataString(profitCentre), Uri.EscapeDataString(workGroupName), flag);
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PutAsync<object, bool?>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SetCos90ForWorkGroupAsync(profitCentre, workGroupName, flag);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PutAsync<object, bool?>(expectedUrl, Arg.Any<object>());
        }

        [Fact]
        public async Task SetCos90ForWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = [new ApiError { Message = "Failed", Code = "FAILED" }] };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Failed", Code = "FAILED" }],
                Meta = new ApiMetaDto()
            };

            _http.PutAsync<object, bool?>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.SetCos90ForWorkGroupAsync("PC001", "WG001", 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
