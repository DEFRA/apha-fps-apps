using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectProfileServiceTest
{
    public class ProjectProfileServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectProfileApiClient _pactProjectProfileApiClient;
        private readonly ProjectProfileService _service;

        public ProjectProfileServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProjectProfileApiClient = Substitute.For<IPactProjectProfileApiClient>();
            _pactClient.PactProjectProfile.Returns(_pactProjectProfileApiClient);
            _service = new ProjectProfileService(_pactClient);
        }

        #region GetProfileGraphDataAsync Tests

        [Fact]
        public async Task GetProfileGraphDataAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var graphData = new List<ProjectProfileGraphDto>
            {
                new ProjectProfileGraphDto { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new ProjectProfileGraphDto { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfileGraphDto>>.SuccessResponse(graphData);
            _pactProjectProfileApiClient.GetProfileGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].Profile);
            Assert.Equal(200m, result.Data[0].TotalCost);
            await _pactProjectProfileApiClient.Received(1).GetProfileGraphDataAsync(project);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedResponse = ApiResponseDto<List<ProjectProfileGraphDto>>.SuccessResponse(new List<ProjectProfileGraphDto>());
            _pactProjectProfileApiClient.GetProfileGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactProjectProfileApiClient.Received(1).GetProfileGraphDataAsync(project);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectProfileGraphDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectProfileApiClient.GetProfileGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
        }

        #endregion

        #region GetCumulativeGraphDataAsync Tests

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var cumulativeData = new List<ProjectProfileCumulativeGraphDto>
            {
                new ProjectProfileCumulativeGraphDto { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new ProjectProfileCumulativeGraphDto { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.SuccessResponse(cumulativeData);
            _pactProjectProfileApiClient.GetCumulativeGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].CumulativeProfile);
            Assert.Equal(200m, result.Data[0].CumulativeCost);
            await _pactProjectProfileApiClient.Received(1).GetCumulativeGraphDataAsync(project);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.SuccessResponse(new List<ProjectProfileCumulativeGraphDto>());
            _pactProjectProfileApiClient.GetCumulativeGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactProjectProfileApiClient.Received(1).GetCumulativeGraphDataAsync(project);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectProfileApiClient.GetCumulativeGraphDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
        }

        #endregion
    }
}
