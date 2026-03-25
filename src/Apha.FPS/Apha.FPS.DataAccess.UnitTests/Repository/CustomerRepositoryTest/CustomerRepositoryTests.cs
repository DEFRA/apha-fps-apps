using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.CustomerRepositoryTest
{
    public class CustomerRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a CustomerRepository with in-memory Customers data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// Customer has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static CustomerRepository CreateRepository(IEnumerable<Customer> customers)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var customersMockSet = RepositoryTestHelper.CreateMockDbSet(customers);
            mockContext.Setup(x => x.Customers).Returns(customersMockSet.Object);

            return new CustomerRepository(mockContext.Object);
        }

        #region GetAllCustomersAsync

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsAllCustomers_WhenDataExists()
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
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsEmptyCollection_WhenNoCustomersExist()
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
        public async Task GetAllCustomersAsync_ReturnsCorrectData_WhenSingleCustomerExists()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new() { CustomerName = "Customer A" }
            };
            var repo = CreateRepository(customers);

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("Customer A", single.CustomerName);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ReturnsIEnumerable_NotNull()
        {
            // Arrange — verifies the return type contract is always IEnumerable, never null
            var repo = CreateRepository(new List<Customer>());

            // Act
            var result = await repo.GetAllCustomersAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<Customer>>(result);
        }

        #endregion
    }
}