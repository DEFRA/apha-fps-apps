using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class TestSupplierViewDtoTests
    {
        [Fact]
        public void TestSupplierViewDto_DefaultValues_AreExpected()
        {
            var dto = new TestSupplierViewDto();

            Assert.Null(dto.ProjectManager);
            Assert.Null(dto.NoRequired);
            Assert.Null(dto.UnitPrice);
            Assert.Null(dto.TestCost);
            Assert.Null(dto.ProjectStatus);
        }

        [Fact]
        public void TestSupplierViewDto_SetAndGetRequiredProperties_ReturnCorrectValues()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TC001",
                Buyer = "BuyerA"
            };

            Assert.Equal("TC001", dto.TestCode);
            Assert.Equal("BuyerA", dto.Buyer);
        }

        [Fact]
        public void TestSupplierViewDto_SetAndGetAllProperties_ReturnCorrectValues()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TC002",
                Buyer = "BuyerB",
                ProjectManager = "PM1",
                NoRequired = 5,
                UnitPrice = 9.99m,
                TestCost = 49.95m,
                ProjectStatus = "Active"
            };

            Assert.Equal("TC002", dto.TestCode);
            Assert.Equal("BuyerB", dto.Buyer);
            Assert.Equal("PM1", dto.ProjectManager);
            Assert.Equal(5, dto.NoRequired);
            Assert.Equal(9.99m, dto.UnitPrice);
            Assert.Equal(49.95m, dto.TestCost);
            Assert.Equal("Active", dto.ProjectStatus);
        }

        [Fact]
        public void TestSupplierViewDto_SetNullablePropertiesBackToNull_ReturnNull()
        {
            var dto = new TestSupplierViewDto
            {
                TestCode = "TC003",
                Buyer = "BuyerC",
                ProjectManager = "PM",
                NoRequired = 3,
                UnitPrice = 1.00m,
                TestCost = 3.00m,
                ProjectStatus = "Closed"
            };

            dto.ProjectManager = null;
            dto.NoRequired = null;
            dto.UnitPrice = null;
            dto.TestCost = null;
            dto.ProjectStatus = null;

            Assert.Null(dto.ProjectManager);
            Assert.Null(dto.NoRequired);
            Assert.Null(dto.UnitPrice);
            Assert.Null(dto.TestCost);
            Assert.Null(dto.ProjectStatus);
        }
    }
}
