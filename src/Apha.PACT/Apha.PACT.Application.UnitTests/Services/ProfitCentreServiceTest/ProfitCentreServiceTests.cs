using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Services;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Application.UnitTests.Services.ProfitCentreServiceTest
{
    public class ProfitCentreServiceTests
    {
        private readonly IProfitCentreRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ProfitCentreService _sut;

        public ProfitCentreServiceTests()
        {
            _mockRepository = Substitute.For<IProfitCentreRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ProfitCentreService(_mockRepository, _mockMapper);
        }

        #region GetAllProfitCentresAsync

        [Fact]
        public async Task GetAllProfitCentresAsync_WithData_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One", Timesheet = -1 },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two", Timesheet = 0  }
            };
            var dtos = new List<ProfitCentreSettingsDto>
            {
                new() { ProfitCentre = "PC001", ProfitCentreName = "Centre One", Timesheet = -1 },
                new() { ProfitCentre = "PC002", ProfitCentreName = "Centre Two", Timesheet = 0  }
            };

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreSettingsDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEquivalentTo(dtos);
            await _mockRepository.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_EmptyResult_ReturnsEmptyCollection()
        {
            // Arrange
            var entities = new List<PactProfitCentreView>();
            var dtos = new List<ProfitCentreSettingsDto>();

            _mockRepository.GetAllProfitCentresAsync().Returns(entities);
            _mockMapper.Map<IEnumerable<ProfitCentreSettingsDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetAllProfitCentresAsync();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllProfitCentresAsync().ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetAllProfitCentresAsync());
        }

        #endregion

        #region GetProfitCentreSettingsAsync

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WithExistingProfitCentre_ReturnsMappedDto()
        {
            // Arrange
            const string profitCentre = "PC001";
            var entity = new PactProfitCentreView
            {
                ProfitCentre    = profitCentre,
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            };
            var dto = new ProfitCentreSettingsDto
            {
                ProfitCentre    = profitCentre,
                Timesheet       = -1,
                Outputsheet     = 0,
                TimesheetLayout = 1
            };

            _mockRepository.GetProfitCentreSettingsAsync(profitCentre).Returns(entity);
            _mockMapper.Map<ProfitCentreSettingsDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetProfitCentreSettingsAsync(profitCentre);

            // Assert
            result.Should().BeEquivalentTo(dto);
            await _mockRepository.Received(1).GetProfitCentreSettingsAsync(profitCentre);
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_WithNonExistentProfitCentre_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetProfitCentreSettingsAsync("MISSING").Returns((PactProfitCentreView?)null);

            // Act
            var result = await _sut.GetProfitCentreSettingsAsync("MISSING");

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<ProfitCentreSettingsDto>(Arg.Any<PactProfitCentreView>());
        }

        [Fact]
        public async Task GetProfitCentreSettingsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProfitCentreSettingsAsync(Arg.Any<string>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetProfitCentreSettingsAsync("PC001"));
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidArgs_DelegatesAndReturnsTrue()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1).Returns(true);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1);
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_RepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .Returns(false);

            // Act
            var result = await _sut.UpdateProfitCentreSettingsAsync("PC001", 0, 0, 2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.UpdateProfitCentreSettingsAsync("PC001", -1, -1, 1));
        }

        #endregion
    }
}
