using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.CustomerServiceTest
{
    public class CustomerServiceTest
    {
        private readonly ICustomerRepository _mockRepository;
        private readonly CustomerService _sut;

        public CustomerServiceTest()
        {
            _mockRepository = Substitute.For<ICustomerRepository>();
            _sut = new CustomerService(_mockRepository);
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithValidData_ReturnsCustomerNameList()
        {
            // Arrange
            var customerEntities = new List<Customer>
            {
                new Customer { CustomerName = "Alice" },
                new Customer { CustomerName = "Bob" }
            };

            _mockRepository.GetAllCustomersAsync()
                .Returns(Task.FromResult<IEnumerable<Customer>>(customerEntities));

            // Act
            var result = await _sut.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainInOrder("Alice", "Bob");

            await _mockRepository.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_WithEmptyList_ReturnsEmptyStringList()
        {
            // Arrange
            _mockRepository.GetAllCustomersAsync()
                .Returns(Task.FromResult<IEnumerable<Customer>>(new List<Customer>()));

            // Act
            var result = await _sut.GetAllCustomersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_ProjectsOnlyCustomerName_ExcludesOtherFields()
        {
            // Arrange
            var customerEntities = new List<Customer>
            {
                new Customer { CustomerName = "DEFRA" },
                new Customer { CustomerName = "APHA" }
            };

            _mockRepository.GetAllCustomersAsync()
                .Returns(Task.FromResult<IEnumerable<Customer>>(customerEntities));

            // Act
            var result = await _sut.GetAllCustomersAsync();

            // Assert
            result.Should().BeEquivalentTo("DEFRA", "APHA");

            await _mockRepository.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllCustomersAsync()
                .Returns(Task.FromException<IEnumerable<Customer>>(expectedException));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllCustomersAsync()
            );

            exception.Message.Should().Be("Database connection failed");

            await _mockRepository.Received(1).GetAllCustomersAsync();
        }
    }
}
