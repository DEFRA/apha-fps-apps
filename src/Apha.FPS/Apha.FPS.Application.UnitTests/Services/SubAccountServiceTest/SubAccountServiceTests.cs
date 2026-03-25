using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;

namespace Apha.FPS.Application.UnitTests.Services.SubAccountServiceTest
{
    public class SubAccountServiceTests
    {
        private readonly ISubAccountRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly SubAccountService _sut;

        public SubAccountServiceTests()
        {
            _mockRepository = Substitute.For<ISubAccountRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new SubAccountService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var subAccountEntities = new List<SubAccount>
            {
                new SubAccount { SubAccountCode = "SA001", SubAccountName = "Field Operations" },
                new SubAccount { SubAccountCode = "SA002", SubAccountName = "Lab Services" }
            };

            var expectedDtos = new List<SubAccountDto>
            {
                new SubAccountDto { SubAccountCode = "SA001", SubAccountName = "Field Operations" },
                new SubAccountDto { SubAccountCode = "SA002", SubAccountName = "Lab Services" }
            };

            _mockRepository.GetAllSubAccountsAsync()
                .Returns(Task.FromResult<IEnumerable<SubAccount>>(subAccountEntities));

            _mockMapper.Map<IEnumerable<SubAccountDto>>(subAccountEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllSubAccountsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().SubAccountCode.Should().Be("SA001");
            result.First().SubAccountName.Should().Be("Field Operations");

            await _mockRepository.Received(1).GetAllSubAccountsAsync();
            _mockMapper.Received(1).Map<IEnumerable<SubAccountDto>>(subAccountEntities);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<SubAccount>();
            var emptyDtos = new List<SubAccountDto>();

            _mockRepository.GetAllSubAccountsAsync()
                .Returns(Task.FromResult<IEnumerable<SubAccount>>(emptyEntities));

            _mockMapper.Map<IEnumerable<SubAccountDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllSubAccountsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllSubAccountsAsync();
            _mockMapper.Received(1).Map<IEnumerable<SubAccountDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllSubAccountsAsync()
                .Returns(Task.FromResult<IEnumerable<SubAccount>>(null!));

            _mockMapper.Map<IEnumerable<SubAccountDto>>(null)
                .Returns((IEnumerable<SubAccountDto>?)null);

            // Act
            var result = await _sut.GetAllSubAccountsAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllSubAccountsAsync();
            _mockMapper.Received(1).Map<IEnumerable<SubAccountDto>>(null);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllSubAccountsAsync()
                .Returns(Task.FromException<IEnumerable<SubAccount>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllSubAccountsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllSubAccountsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<SubAccountDto>>(Arg.Any<IEnumerable<SubAccount>>());
        }
    }
}