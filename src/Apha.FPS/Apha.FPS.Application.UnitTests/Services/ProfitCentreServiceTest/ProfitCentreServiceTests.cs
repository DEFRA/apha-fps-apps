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
            var entities = new List<ProfitCentre>
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
            var entities = new List<ProfitCentre>();
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
    }
}
