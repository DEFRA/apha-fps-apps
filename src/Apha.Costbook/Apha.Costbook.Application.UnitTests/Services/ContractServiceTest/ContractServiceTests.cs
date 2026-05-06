using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using NSubstitute;
using Xunit;

namespace Apha.Costbook.Application.UnitTests.Services.ContractServiceTest
{
    public class ContractServiceTests
    {
        private readonly IContractRepository _mockRepository;
        private readonly ContractService _contractService;

        public ContractServiceTests()
        {
            _mockRepository = Substitute.For<IContractRepository>();
            _contractService = new ContractService(_mockRepository);
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_ReturnsContractNumbers()
        {
            // Arrange
            var contractNumbers = new List<string> { "CONTRACT001", "CONTRACT002", "CONTRACT003" };
            _mockRepository.GetAllContractNumbersAsync().Returns(contractNumbers);

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("CONTRACT001", result[0]);
            Assert.Equal("CONTRACT002", result[1]);
            Assert.Equal("CONTRACT003", result[2]);
            await _mockRepository.Received(1).GetAllContractNumbersAsync();
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAllContractNumbersAsync().Returns(new List<string>());

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockRepository.Received(1).GetAllContractNumbersAsync();
        }

        [Fact]
        public async Task GetAllContractNumbersAsync_SingleResult_ReturnsSingleItem()
        {
            // Arrange
            var contractNumbers = new List<string> { "CONTRACT001" };
            _mockRepository.GetAllContractNumbersAsync().Returns(contractNumbers);

            // Act
            var result = await _contractService.GetAllContractNumbersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("CONTRACT001", result[0]);
            await _mockRepository.Received(1).GetAllContractNumbersAsync();
        }
    }
}
