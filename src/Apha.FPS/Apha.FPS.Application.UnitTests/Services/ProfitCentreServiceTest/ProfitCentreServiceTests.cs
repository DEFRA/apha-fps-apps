using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IProfitCentreRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProfitCentreService _sut;

        public ProfitCentreServiceTests()
        {
            _mockRepository = Substitute.For<IProfitCentreRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new ProfitCentreService(_mockRepository, _mockMapper);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithValidData_ReturnsMappedList()
        {
            // Arrange
            var entities = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two", Division = "DIV1" }
            };
            var expected = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };

            _mockRepository.GetProfitCentresAsync().Returns(entities);
            _mockMapper.Map<List<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetProfitCentresAsync();
            _mockMapper.Received(1).Map<List<ProfitCentreDto>>(entities);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            var entities = new List<ProfitCentreView>();
            var expected = new List<ProfitCentreDto>();

            _mockRepository.GetProfitCentresAsync().Returns(entities);
            _mockMapper.Map<List<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.GetProfitCentresAsync());
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        [Fact]
        public async Task GetAllProfitCentresAsync_WithValidData_ReturnsMappedEnumerable()
        {
            // Arrange
            var entities = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two", Division = "DIV1" }
            };
            var expected = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithEmptyRepository_ReturnsEmptyEnumerable()
        {
            // Arrange
            var entities = new List<ProfitCentre>();
            var expected = new List<ProfitCentreDto>();

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreDto>>(entities).Returns(expected);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetAllProfitCentresAsync());
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithExistingId_ReturnsMappedDto()
        {
            // Arrange
            var entity   = new ProfitCentre { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" };
            var expected = new ProfitCentreDto { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" };

            _mockRepository.GetProfitCentreByIdAsync("PC01").Returns(entity);
            _mockMapper.Map<ProfitCentreDto>(entity).Returns(expected);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC01");

            // Assert
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetProfitCentreByIdAsync("PC_MISSING").Returns((ProfitCentre?)null);

            // Act
            var result = await _sut.GetProfitCentreByIdAsync("PC_MISSING");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProfitCentreByIdAsync(Arg.Any<string>())
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetProfitCentreByIdAsync("PC01"));
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidData_ReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(true);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new InvalidOperationException("DB failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _sut.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1));
        }

        #endregion
    }
}
