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

        #region GetProfileDataAsync Tests

        [Fact]
        public async Task GetProfileDataAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var profileData = new List<ProjectProfileDto>
            {
                new ProjectProfileDto { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new ProjectProfileDto { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(profileData);
            _pactProjectProfileApiClient.GetProfileDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].Profile);
            Assert.Equal(200m, result.Data[0].TotalCost);
            await _pactProjectProfileApiClient.Received(1).GetProfileDataAsync(project);
        }

        [Fact]
        public async Task GetProfileDataAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedResponse = ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(new List<ProjectProfileDto>());
            _pactProjectProfileApiClient.GetProfileDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactProjectProfileApiClient.Received(1).GetProfileDataAsync(project);
        }

        [Fact]
        public async Task GetProfileDataAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectProfileDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectProfileApiClient.GetProfileDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
        }

        #endregion

        #region GetCumulativeDataAsync Tests

        [Fact]
        public async Task GetCumulativeDataAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var cumulativeData = new List<ProjectProfileCumulativeDto>
            {
                new ProjectProfileCumulativeDto { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new ProjectProfileCumulativeDto { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(cumulativeData);
            _pactProjectProfileApiClient.GetCumulativeDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].CumulativeProfile);
            Assert.Equal(200m, result.Data[0].CumulativeCost);
            await _pactProjectProfileApiClient.Received(1).GetCumulativeDataAsync(project);
        }

        [Fact]
        public async Task GetCumulativeDataAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(new List<ProjectProfileCumulativeDto>());
            _pactProjectProfileApiClient.GetCumulativeDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactProjectProfileApiClient.Received(1).GetCumulativeDataAsync(project);
        }

        [Fact]
        public async Task GetCumulativeDataAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectProfileCumulativeDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectProfileApiClient.GetCumulativeDataAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetCumulativeDataAsync(project);

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
