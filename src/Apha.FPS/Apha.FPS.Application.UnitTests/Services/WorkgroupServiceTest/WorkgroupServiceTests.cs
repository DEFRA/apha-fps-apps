using Apha.FPS.Application.Services;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.WorkGroupServiceTest
{
    public class WorkGroupServiceTests
    {
        private readonly IWorkGroupRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly WorkGroupService _sut;

        public WorkGroupServiceTests()
        {
            _mockRepository = Substitute.For<IWorkGroupRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new WorkGroupService(_mockRepository, _mockMapper);
        }

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkGroupService(null!, _mockMapper));
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithData_ReturnsWorkgroupNames()
        {
            // Arrange
            var names = new List<string> { "WG01", "WG02", "WG03" };
            _mockRepository.GetAllWorkGroupNamesAsync().Returns(names);

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Equal(names, result);
            await _mockRepository.Received(1).GetAllWorkGroupNamesAsync();
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupNamesAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkgroupNamesAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupNamesAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetAllWorkGroupNamesAsync());
        }
    }
}
