using Apha.FPS.Application.Dtos;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using ContractServiceUnderTest = Apha.FPS.Application.Services.ContractService;

namespace Apha.FPS.Application.UnitTests.Services.ContractServiceTest
{
    public class ContractServiceTests
    {
        private readonly IContractRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ContractServiceUnderTest _sut;

        public ContractServiceTests()
        {
            _mockRepository = Substitute.For<IContractRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ContractServiceUnderTest(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllContractsAsync_WithValidData_ReturnsMappedDtoList()
        {
            // Arrange
            var contractEntities = new List<Contract>
            {
                new Contract { ContractNo = "CON001", Category = "A", Manager = "Alice", Customer = "CustomerA" },
                new Contract { ContractNo = "CON002", Category = "B", Manager = "Bob",   Customer = "CustomerB" }
            };

            var expectedDtos = new List<ContractDto>
            {
                new ContractDto { Contractno = "CON001", Category = "A", Manager = "Alice", Customer = "CustomerA" },
                new ContractDto { Contractno = "CON002", Category = "B", Manager = "Bob",   Customer = "CustomerB" }
            };

            _mockRepository.GetAllContractsAsync()
                .Returns(Task.FromResult<IEnumerable<Contract>>(contractEntities));

            _mockMapper.Map<IEnumerable<ContractDto>>(contractEntities)
                .Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllContractsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Contractno.Should().Be("CON001");
            result.First().Manager.Should().Be("Alice");

            await _mockRepository.Received(1).GetAllContractsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ContractDto>>(contractEntities);
        }

        [Fact]
        public async Task GetAllContractsAsync_WithEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            var emptyEntities = new List<Contract>();
            var emptyDtos = new List<ContractDto>();

            _mockRepository.GetAllContractsAsync()
                .Returns(Task.FromResult<IEnumerable<Contract>>(emptyEntities));

            _mockMapper.Map<IEnumerable<ContractDto>>(emptyEntities)
                .Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllContractsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllContractsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ContractDto>>(emptyEntities);
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenRepositoryReturnsNull_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetAllContractsAsync()
                .Returns(Task.FromResult<IEnumerable<Contract>>(null!));

            _mockMapper.Map<IEnumerable<ContractDto>>(null)
                .Returns((IEnumerable<ContractDto>?)null);

            // Act
            var result = await _sut.GetAllContractsAsync();

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetAllContractsAsync();
            _mockMapper.Received(1).Map<IEnumerable<ContractDto>>(null);
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllContractsAsync()
                .Returns(Task.FromException<IEnumerable<Contract>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllContractsAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllContractsAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ContractDto>>(Arg.Any<IEnumerable<Contract>>());
        }
    }
}