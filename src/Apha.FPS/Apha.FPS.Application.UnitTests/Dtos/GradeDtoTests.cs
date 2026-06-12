using Apha.FPS.Application.Dtos;
using FluentAssertions;

namespace Apha.FPS.Application.UnitTests.Dtos
{
    public class GradeDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
            // Arrange & Act
            var dto = new GradeDto
            {
                GradeCode   = "SGO1",
                Description = "Senior Grade One",
                AvSalary    = 65000.00m,
                PactCode    = "P01",
                AvLeaveHrs  = 208.0,
                AvSickHrs   = 40.0,
                FpsYear     = 2025
            };

            // Assert
            dto.GradeCode.Should().Be("SGO1");
            dto.Description.Should().Be("Senior Grade One");
            dto.AvSalary.Should().Be(65000.00m);
            dto.PactCode.Should().Be("P01");
            dto.AvLeaveHrs.Should().Be(208.0);
            dto.AvSickHrs.Should().Be(40.0);
            dto.FpsYear.Should().Be(2025);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
            // Arrange & Act
            var dto = new GradeDto
            {
                GradeCode   = "AA",
                Description = null,
                AvSalary    = null,
                PactCode    = null,
                AvLeaveHrs  = null,
                AvSickHrs   = null,
                FpsYear     = null
            };

            // Assert
            dto.GradeCode.Should().Be("AA");
            dto.Description.Should().BeNull();
            dto.AvSalary.Should().BeNull();
            dto.PactCode.Should().BeNull();
            dto.AvLeaveHrs.Should().BeNull();
            dto.AvSickHrs.Should().BeNull();
            dto.FpsYear.Should().BeNull();
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            // Arrange
            var dto = new GradeDto { GradeCode = "X" };

            // Act
            dto.GradeCode   = "Y";
            dto.Description = "Updated Description";
            dto.AvSalary    = 70000.00m;
            dto.PactCode    = "P02";
            dto.AvLeaveHrs  = 160.0;
            dto.AvSickHrs   = 30.0;
            dto.FpsYear     = 2026;

            // Assert
            dto.GradeCode.Should().Be("Y");
            dto.Description.Should().Be("Updated Description");
            dto.AvSalary.Should().Be(70000.00m);
            dto.PactCode.Should().Be("P02");
            dto.AvLeaveHrs.Should().Be(160.0);
            dto.AvSickHrs.Should().Be(30.0);
            dto.FpsYear.Should().Be(2026);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new GradeDto();

            dto.GradeCode.Should().BeNull();
            dto.Description.Should().BeNull();
            dto.AvSalary.Should().BeNull();
            dto.PactCode.Should().BeNull();
            dto.AvLeaveHrs.Should().BeNull();
            dto.AvSickHrs.Should().BeNull();
            dto.FpsYear.Should().BeNull();
        }

        [Fact]
        public void GradeCode_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new GradeDto { GradeCode = string.Empty };

            dto.GradeCode.Should().BeEmpty();
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-1234.56)]
        [InlineData(9999999.99)]
        public void AvSalary_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto = new GradeDto { GradeCode = "X", AvSalary = value };

            dto.AvSalary.Should().Be(value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-8.5)]
        [InlineData(2080.0)]
        public void AvLeaveHrs_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new GradeDto { GradeCode = "X", AvLeaveHrs = value };

            dto.AvLeaveHrs.Should().Be(value);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-4.25)]
        [InlineData(1040.0)]
        public void AvSickHrs_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new GradeDto { GradeCode = "X", AvSickHrs = value };

            dto.AvSickHrs.Should().Be(value);
        }

        #endregion
    }
}
