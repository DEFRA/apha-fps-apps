using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.WorkgroupServiceTest
{
    public class WorkgroupServiceTests
    {
        private readonly IWorkgroupRepository _mockRepository;
        private readonly WorkgroupService _sut;

        public WorkgroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkgroupRepository>();
            _sut            = new WorkgroupService(_mockRepository);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupService(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsWorkgroupNames()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02", "WG03" };
            _mockRepository.GetAllWorkgroupNamesAsync().Returns(names);

            // Act
            var result = await _sut.GetAllWorkgroupNamesAsync();

            // Assert
            Assert.Equal(names, result);
            await _mockRepository.Received(1).GetAllWorkgroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAllWorkgroupNamesAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetAllWorkgroupNamesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllWorkgroupNamesAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetAllWorkgroupNamesAsync());
        }
    }
}
