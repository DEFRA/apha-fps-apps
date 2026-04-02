using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.UnitTests.Services
{
    public class ProgramAnimalPlanServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsAnimalPlanApiClient _mockAnimalPlanApiClient;
        private readonly ProgramAnimalPlanService _sut;

        public ProgramAnimalPlanServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockAnimalPlanApiClient = Substitute.For<IFpsAnimalPlanApiClient>();
            _mockFpsClient.FpsAnimalPlan.Returns(_mockAnimalPlanApiClient);
            _sut = new ProgramAnimalPlanService(_mockFpsClient);
        }

        #region GetAllAnimalCostAsync

        [Fact]
        public async Task GetAllAnimalCostAsync_ReturnsData_WhenSuccessful()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = new ApiResponseDto<List<AnimalCostViewDto>>
            {
                Success = true,
                Data = new List<AnimalCostViewDto>
                {
                    new() { IndCounter = 1, JobCode = "JOB001", AnimalType = "CAT", AnimalCost = 100m }
                }
            };
            _mockAnimalPlanApiClient.GetAllAnimalCostAsync(query, "JOB001").Returns(expected);

            // Act
            var result = await _sut.GetAllAnimalCostAsync(query, "JOB001");

            // Assert
            result.Should().Be(expected);
            await _mockAnimalPlanApiClient.Received(1).GetAllAnimalCostAsync(query, "JOB001");
        }

        [Fact]
        public async Task GetAllAnimalCostAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var expected = new ApiResponseDto<List<AnimalCostViewDto>>
            {
                Success = true,
                Data = new List<AnimalCostViewDto>()
            };
            _mockAnimalPlanApiClient.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(expected);

            // Act
            var result = await _sut.GetAllAnimalCostAsync(query, "EMPTY");

            // Assert
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _mockAnimalPlanApiClient.GetAllAnimalCostAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAnimalCostAsync(query, "JOB001"));
            exception.Message.Should().Be("API error");
        }

        #endregion

        #region GetAnimalLookupAsync

        [Fact]
        public async Task GetAnimalLookupAsync_ReturnsList_WhenSuccessful()
        {
            // Arrange
            var expected = new ApiResponseDto<List<AnimalDto>>
            {
                Success = true,
                Data = new List<AnimalDto> { new() { AnimalType = "CAT", DailyRate = 10m } }
            };
            _mockAnimalPlanApiClient.GetAnimalLookupAsync().Returns(expected);

            // Act
            var result = await _sut.GetAnimalLookupAsync();

            // Assert
            result.Should().Be(expected);
            result.Data.Should().HaveCount(1);
            await _mockAnimalPlanApiClient.Received(1).GetAnimalLookupAsync();
        }

        [Fact]
        public async Task GetAnimalLookupAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.GetAnimalLookupAsync()
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAnimalLookupAsync());
        }

        #endregion

        #region GetAnimalRateAsync

        [Fact]
        public async Task GetAnimalRateAsync_ReturnsRate_WhenFound()
        {
            // Arrange
            var expected = new ApiResponseDto<decimal?> { Success = true, Data = 75.50m };
            _mockAnimalPlanApiClient.GetAnimalRateAsync("CAT").Returns(expected);

            // Act
            var result = await _sut.GetAnimalRateAsync("CAT");

            // Assert
            result.Should().Be(expected);
            result.Data.Should().Be(75.50m);
            await _mockAnimalPlanApiClient.Received(1).GetAnimalRateAsync("CAT");
        }

        [Fact]
        public async Task GetAnimalRateAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var expected = new ApiResponseDto<decimal?> { Success = true, Data = null };
            _mockAnimalPlanApiClient.GetAnimalRateAsync("UNKNOWN").Returns(expected);

            // Act
            var result = await _sut.GetAnimalRateAsync("UNKNOWN");

            // Assert
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetAnimalRateAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.GetAnimalRateAsync(Arg.Any<string>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAnimalRateAsync("CAT"));
        }

        #endregion

        #region GetTotalAnimalCostAsync

        [Fact]
        public async Task GetTotalAnimalCostAsync_ReturnsTotal_WhenSuccessful()
        {
            // Arrange
            var expected = new ApiResponseDto<decimal> { Success = true, Data = 500m };
            _mockAnimalPlanApiClient.GetTotalAnimalCostAsync("JOB001").Returns(expected);

            // Act
            var result = await _sut.GetTotalAnimalCostAsync("JOB001");

            // Assert
            result.Should().Be(expected);
            result.Data.Should().Be(500m);
            await _mockAnimalPlanApiClient.Received(1).GetTotalAnimalCostAsync("JOB001");
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_ReturnsZero_WhenNoData()
        {
            // Arrange
            var expected = new ApiResponseDto<decimal> { Success = true, Data = 0m };
            _mockAnimalPlanApiClient.GetTotalAnimalCostAsync("EMPTY").Returns(expected);

            // Act
            var result = await _sut.GetTotalAnimalCostAsync("EMPTY");

            // Assert
            result.Data.Should().Be(0m);
        }

        [Fact]
        public async Task GetTotalAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.GetTotalAnimalCostAsync(Arg.Any<string>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetTotalAnimalCostAsync("JOB001"));
        }

        #endregion

        #region GetAnimalCostViewByIdAsync

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsDto_WhenFound()
        {
            // Arrange
            var dto = new AnimalCostViewDto { IndCounter = 1, JobCode = "JOB001", AnimalCost = 100m };
            var expected = new ApiResponseDto<AnimalCostViewDto?> { Success = true, Data = dto };
            _mockAnimalPlanApiClient.GetAnimalCostViewByIdAsync(1, "JOB001").Returns(expected);

            // Act
            var result = await _sut.GetAnimalCostViewByIdAsync(1, "JOB001");

            // Assert
            result.Should().Be(expected);
            result.Data.Should().Be(dto);
            await _mockAnimalPlanApiClient.Received(1).GetAnimalCostViewByIdAsync(1, "JOB001");
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var expected = new ApiResponseDto<AnimalCostViewDto?> { Success = true, Data = null };
            _mockAnimalPlanApiClient.GetAnimalCostViewByIdAsync(999, "JOB001").Returns(expected);

            // Act
            var result = await _sut.GetAnimalCostViewByIdAsync(999, "JOB001");

            // Assert
            result.Data.Should().BeNull();
        }

        [Fact]
        public async Task GetAnimalCostViewByIdAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.GetAnimalCostViewByIdAsync(Arg.Any<int>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAnimalCostViewByIdAsync(1, "JOB001"));
        }

        #endregion

        #region CreateAnimalCostAsync

        [Fact]
        public async Task CreateAnimalCostAsync_ReturnsCreatedDto_WhenSuccessful()
        {
            // Arrange
            var dto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT" };
            var expected = new ApiResponseDto<AnimalRequestDto> { Success = true, Data = dto };
            _mockAnimalPlanApiClient.CreateAnimalCostAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAnimalCostAsync(dto);

            // Assert
            result.Should().Be(expected);
            await _mockAnimalPlanApiClient.Received(1).CreateAnimalCostAsync(dto);
        }

        [Fact]
        public async Task CreateAnimalCostAsync_ReturnsFailure_WhenServiceFails()
        {
            // Arrange
            var dto = new AnimalRequestDto { JobCode = "JOB001", AnimalType = "CAT" };
            var expected = new ApiResponseDto<AnimalRequestDto> { Success = false };
            _mockAnimalPlanApiClient.CreateAnimalCostAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAnimalCostAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task CreateAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.CreateAnimalCostAsync(Arg.Any<AnimalRequestDto>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CreateAnimalCostAsync(new AnimalRequestDto()));
        }

        #endregion

        #region UpdateAnimalCostAsync

        [Fact]
        public async Task UpdateAnimalCostAsync_ReturnsUpdatedDto_WhenSuccessful()
        {
            // Arrange
            var dto = new AnimalRequestDto { IndCounter = 1, JobCode = "JOB001", AnimalType = "DOG" };
            var expected = new ApiResponseDto<AnimalRequestDto> { Success = true, Data = dto };
            _mockAnimalPlanApiClient.UpdateAnimalCostAsync(dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAnimalCostAsync(dto);

            // Assert
            result.Should().Be(expected);
            await _mockAnimalPlanApiClient.Received(1).UpdateAnimalCostAsync(dto);
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ReturnsFailure_WhenServiceFails()
        {
            // Arrange
            var dto = new AnimalRequestDto { IndCounter = 999 };
            var expected = new ApiResponseDto<AnimalRequestDto> { Success = false };
            _mockAnimalPlanApiClient.UpdateAnimalCostAsync(dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAnimalCostAsync(dto);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.UpdateAnimalCostAsync(Arg.Any<AnimalRequestDto>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAnimalCostAsync(new AnimalRequestDto()));
        }

        #endregion

        #region DeleteAnimalCostAsync

        [Fact]
        public async Task DeleteAnimalCostAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var expected = new ApiResponseDto<bool> { Success = true, Data = true };
            _mockAnimalPlanApiClient.DeleteAnimalCostAsync(1).Returns(expected);

            // Act
            var result = await _sut.DeleteAnimalCostAsync(1);

            // Assert
            result.Should().Be(expected);
            result.Data.Should().BeTrue();
            await _mockAnimalPlanApiClient.Received(1).DeleteAnimalCostAsync(1);
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var expected = new ApiResponseDto<bool> { Success = false, Data = false };
            _mockAnimalPlanApiClient.DeleteAnimalCostAsync(999).Returns(expected);

            // Act
            var result = await _sut.DeleteAnimalCostAsync(999);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAnimalCostAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _mockAnimalPlanApiClient.DeleteAnimalCostAsync(Arg.Any<int>())
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAnimalCostAsync(1));
        }

        #endregion
    }
}
