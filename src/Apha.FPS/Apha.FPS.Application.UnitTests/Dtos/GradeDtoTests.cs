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
    }
}
