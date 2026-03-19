using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.StatusServiceTest
{
    public class StatusServiceTests
    {
        private readonly IStatusRepository _mockRepository;
        private readonly StatusService _sut;

        public StatusServiceTests()
        {
            _mockRepository = Substitute.For<IStatusRepository>();
            _sut = new StatusService(_mockRepository);
        }

        [Fact]
        public async Task GetAllStatusesAsync_WithValidData_ReturnsStatusValueList()
        {
            // Arrange
            var statusEntities = new List<Status>
            {
                new Status { StatusValue = "Active" },
                new Status { StatusValue = "Inactive" }
            };

            _mockRepository.GetAllStatusesAsync()
                .Returns(Task.FromResult<IEnumerable<Status>>(statusEntities));

            // Act
            var result = await _sut.GetAllStatusesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainInOrder("Active", "Inactive");

            await _mockRepository.Received(1).GetAllStatusesAsync();
        }

        [Fact]
        public async Task GetAllStatusesAsync_WithEmptyList_ReturnsEmptyStringList()
        {
            // Arrange
            _mockRepository.GetAllStatusesAsync()
                .Returns(Task.FromResult<IEnumerable<Status>>(new List<Status>()));

            // Act
            var result = await _sut.GetAllStatusesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllStatusesAsync();
        }

        [Fact]
        public async Task GetAllStatusesAsync_ProjectsOnlyStatusValue_ExcludesOtherFields()
        {
            // Arrange
            var statusEntities = new List<Status>
            {
                new Status { StatusValue = "Pending" },
                new Status { StatusValue = "Closed" }
            };

            _mockRepository.GetAllStatusesAsync()
                .Returns(Task.FromResult<IEnumerable<Status>>(statusEntities));

            // Act
            var result = await _sut.GetAllStatusesAsync();

            // Assert
            result.Should().BeEquivalentTo(new[] { "Pending", "Closed" });

            await _mockRepository.Received(1).GetAllStatusesAsync();
        }

        [Fact]
        public async Task GetAllStatusesAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllStatusesAsync()
                .Returns(Task.FromException<IEnumerable<Status>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllStatusesAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllStatusesAsync();
        }
    }
}