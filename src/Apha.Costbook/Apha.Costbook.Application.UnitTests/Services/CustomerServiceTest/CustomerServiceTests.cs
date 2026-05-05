using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using AutoMapper;
using NSubstitute;

namespace Apha.Costbook.Application.UnitTests.Services.CustomerServiceTest
{
    public class CustomerServiceTests
    {
        private readonly ICustomerRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly CustomerService _customerService;

        public CustomerServiceTests()
        {
            _mockRepository = Substitute.For<ICustomerRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _customerService = new CustomerService(_mockRepository, _mockMapper);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsCustomerDtos()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerName = "Customer A" },
                new Customer { CustomerName = "Customer B" }
            };
            var customerDtos = new List<CustomerDto>
            {
                new CustomerDto { CustomerName = "Customer A" },
                new CustomerDto { CustomerName = "Customer B" }
            };

            _mockRepository.GetAllCustomersAsync().Returns(customers);
            _mockMapper.Map<List<CustomerDto>>(customers).Returns(customerDtos);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Customer A", result[0].CustomerName);
            Assert.Equal("Customer B", result[1].CustomerName);
            await _mockRepository.Received(1).GetAllCustomersAsync();
            _mockMapper.Received(1).Map<List<CustomerDto>>(customers);
        }

        [Fact]
        public async Task GetAllCustomersAsync_EmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var customers = new List<Customer>();
            var customerDtos = new List<CustomerDto>();

            _mockRepository.GetAllCustomersAsync().Returns(customers);
            _mockMapper.Map<List<CustomerDto>>(customers).Returns(customerDtos);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            await _mockRepository.Received(1).GetAllCustomersAsync();
            _mockMapper.Received(1).Map<List<CustomerDto>>(customers);
        }

        [Fact]
        public async Task GetAllCustomersAsync_SingleResult_ReturnsSingleItem()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerName = "Customer A" }
            };
            var customerDtos = new List<CustomerDto>
            {
                new CustomerDto { CustomerName = "Customer A" }
            };

            _mockRepository.GetAllCustomersAsync().Returns(customers);
            _mockMapper.Map<List<CustomerDto>>(customers).Returns(customerDtos);

            // Act
            var result = await _customerService.GetAllCustomersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("Customer A", result[0].CustomerName);
            await _mockRepository.Received(1).GetAllCustomersAsync();
        }
    }
}
