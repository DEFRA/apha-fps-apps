using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using NSubstitute;
using Xunit;

namespace Apha.Costbook.Application.UnitTests.Services.YearMasterServiceTest
{
    public class YearMasterServiceTests
    {
        private readonly IYearMasterRepository _mockRepository;
        private readonly YearMasterService _yearMasterService;

        public YearMasterServiceTests()
        {
            _mockRepository = Substitute.For<IYearMasterRepository>();
            _yearMasterService = new YearMasterService(_mockRepository);
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsOpenYear()
        {
            // Arrange
            var expectedYear = 2024;
            _mockRepository.GetOpenYearAsync().Returns(expectedYear);

            // Act
            var result = await _yearMasterService.GetOpenYearAsync();

            // Assert
            Assert.Equal(expectedYear, result);
            await _mockRepository.Received(1).GetOpenYearAsync();
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsZero_WhenNoOpenYear()
        {
            // Arrange
            _mockRepository.GetOpenYearAsync().Returns(0);

            // Act
            var result = await _yearMasterService.GetOpenYearAsync();

            // Assert
            Assert.Equal(0, result);
            await _mockRepository.Received(1).GetOpenYearAsync();
        }

        [Fact]
        public async Task GetOpenYearAsync_ReturnsCorrectYear_ForDifferentYears()
        {
            // Arrange
            var expectedYear = 2025;
            _mockRepository.GetOpenYearAsync().Returns(expectedYear);

            // Act
            var result = await _yearMasterService.GetOpenYearAsync();

            // Assert
            Assert.Equal(2025, result);
            await _mockRepository.Received(1).GetOpenYearAsync();
        }
    }
}
