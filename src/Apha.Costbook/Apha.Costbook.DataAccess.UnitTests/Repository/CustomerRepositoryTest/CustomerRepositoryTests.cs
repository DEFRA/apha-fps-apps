using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.CustomerRepositoryTest
{
    public class CustomerRepositoryTests
    {
        private static CustomerRepository CreateRepository(IEnumerable<Customer> customers)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var customersMockSet = RepositoryTestHelper.CreateMockDbSet(customers);
            mockContext.Setup(x => x.Set<Customer>()).Returns(customersMockSet.Object);
            mockContext.Setup(x => x.Customers).Returns(customersMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new CustomerRepository(mockContext.Object);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsAllCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { CustomerName = "Customer A" },
                new() { CustomerName = "Customer B" },
                new() { CustomerName = "Customer C" }
            };
            var repo = CreateRepository(customers);

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsEmptyList_WhenNoCustomers()
        {
            // Arrange
            var repo = CreateRepository(new List<Customer>());

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsCorrectCustomerNames()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { CustomerName = "APHA" },
                new() { CustomerName = "DEFRA" }
            };
            var repo = CreateRepository(customers);

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            Assert.Contains(result, c => c.CustomerName == "APHA");
            Assert.Contains(result, c => c.CustomerName == "DEFRA");
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsSingleCustomer_WhenOnlyOneExists()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { CustomerName = "Only Customer" }
            };
            var repo = CreateRepository(customers);

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Only Customer", result[0].CustomerName);
        }
    }
}
