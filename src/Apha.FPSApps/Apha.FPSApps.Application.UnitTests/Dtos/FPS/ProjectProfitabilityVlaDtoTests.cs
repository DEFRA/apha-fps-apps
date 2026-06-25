using Apha.FPSApps.Application.Dtos.FPS;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class ProjectProfitabilityVlaDtoTests
    {
        [Fact]
        public void ProjectProfitabilityVlaDto_AllProperties_GetAndSetCorrectly()
        {
            // Arrange & Act
            var dto = new ProjectProfitabilityVlaDto
            {
                Id              = 42,
                JobCode         = "PP001",
                Program         = "PROG01",
                Customer        = "ACME Ltd",
                Manager         = "John Smith",
                Status          = "Approved",
                StaffCosts      = 1000m,
                TestCost        = 200m,
                AnimalCosts     = 150m,
                AdditionalCosts = 50m,
                TotalCosts      = 1400m,
                Budget          = 5000m,
                Profit          = 3600m,
                TargetProfit    = 3000m,
                OffTarget       = 600m
            };

            // Assert
            Assert.Equal(42,           dto.Id);
            Assert.Equal("PP001",      dto.JobCode);
            Assert.Equal("PROG01",     dto.Program);
            Assert.Equal("ACME Ltd",   dto.Customer);
            Assert.Equal("John Smith", dto.Manager);
            Assert.Equal("Approved",   dto.Status);
            Assert.Equal(1000m,        dto.StaffCosts);
            Assert.Equal(200m,         dto.TestCost);
            Assert.Equal(150m,         dto.AnimalCosts);
            Assert.Equal(50m,          dto.AdditionalCosts);
            Assert.Equal(1400m,        dto.TotalCosts);
            Assert.Equal(5000m,        dto.Budget);
            Assert.Equal(3600m,        dto.Profit);
            Assert.Equal(3000m,        dto.TargetProfit);
            Assert.Equal(600m,         dto.OffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaDto_DefaultValues_NullablePropertiesAreNull_ValueTypesAreZero()
        {
            // Arrange & Act
            var dto = new ProjectProfitabilityVlaDto();

            // Assert — nullable properties default to null; decimal value types default to 0
            Assert.Null(dto.Id);
            Assert.Null(dto.Program);
            Assert.Null(dto.Customer);
            Assert.Null(dto.Manager);
            Assert.Null(dto.Status);
            Assert.Null(dto.Budget);
            Assert.Equal(0m, dto.StaffCosts);
            Assert.Equal(0m, dto.TestCost);
            Assert.Equal(0m, dto.AnimalCosts);
            Assert.Equal(0m, dto.AdditionalCosts);
            Assert.Equal(0m, dto.TotalCosts);
            Assert.Equal(0m, dto.Profit);
            Assert.Equal(0m, dto.TargetProfit);
            Assert.Equal(0m, dto.OffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaDto_OffTarget_AcceptsNegativeValue()
        {
            // Arrange & Act — negative OffTarget triggers the red highlight in the VLA grid
            var dto = new ProjectProfitabilityVlaDto
            {
                JobCode   = "PP001",
                OffTarget = -500m
            };

            // Assert
            Assert.Equal(-500m, dto.OffTarget);
        }

        [Fact]
        public void ProjectProfitabilityVlaDto_Budget_AcceptsNull()
        {
            // Arrange & Act — budget is optional; projects may have no budget set
            var dto = new ProjectProfitabilityVlaDto
            {
                JobCode = "PP001",
                Budget  = null
            };

            // Assert
            Assert.Null(dto.Budget);
        }

        [Theory]
        [InlineData("Approved")]
        [InlineData("Completed")]
        [InlineData("Not Approved")]
        [InlineData(null)]
        public void ProjectProfitabilityVlaDto_Status_AcceptsAllValidValues(string? status)
        {
            // Arrange & Act
            var dto = new ProjectProfitabilityVlaDto { JobCode = "PP001", Status = status };

            // Assert
            Assert.Equal(status, dto.Status);
        }
    }
}
