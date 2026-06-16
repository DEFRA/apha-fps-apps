using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.UnitTests.Dtos.FPS
{
    public class GradeDtoTests
    {
        #region Property Tests

        [Fact]
        public void Properties_SetAndGet_AllValues_ReturnsCorrectValues()
        {
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

            Assert.Equal("SGO1",             dto.GradeCode);
            Assert.Equal("Senior Grade One", dto.Description);
            Assert.Equal(65000.00m,          dto.AvSalary);
            Assert.Equal("P01",              dto.PactCode);
            Assert.Equal(208.0,              dto.AvLeaveHrs);
            Assert.Equal(40.0,               dto.AvSickHrs);
            Assert.Equal(2025,               dto.FpsYear);
        }

        [Fact]
        public void NullableProperties_SetToNull_ReturnNull()
        {
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

            Assert.Equal("AA", dto.GradeCode);
            Assert.Null(dto.Description);
            Assert.Null(dto.AvSalary);
            Assert.Null(dto.PactCode);
            Assert.Null(dto.AvLeaveHrs);
            Assert.Null(dto.AvSickHrs);
            Assert.Null(dto.FpsYear);
        }

        [Fact]
        public void Properties_CanBeUpdatedAfterInitialisation()
        {
            var dto = new GradeDto { GradeCode = "X" };

            dto.GradeCode   = "Y";
            dto.Description = "Updated Description";
            dto.AvSalary    = 70000.00m;
            dto.PactCode    = "P02";
            dto.AvLeaveHrs  = 160.0;
            dto.AvSickHrs   = 30.0;
            dto.FpsYear     = 2026;

            Assert.Equal("Y",                    dto.GradeCode);
            Assert.Equal("Updated Description",  dto.Description);
            Assert.Equal(70000.00m,              dto.AvSalary);
            Assert.Equal("P02",                  dto.PactCode);
            Assert.Equal(160.0,                  dto.AvLeaveHrs);
            Assert.Equal(30.0,                   dto.AvSickHrs);
            Assert.Equal(2026,                   dto.FpsYear);
        }

        #endregion

        #region Default Value Tests

        [Fact]
        public void DefaultValues_WhenConstructedWithNoArguments_AreExpected()
        {
            var dto = new GradeDto();

            Assert.Null(dto.GradeCode);
            Assert.Null(dto.Description);
            Assert.Null(dto.AvSalary);
            Assert.Null(dto.PactCode);
            Assert.Null(dto.AvLeaveHrs);
            Assert.Null(dto.AvSickHrs);
            Assert.Null(dto.FpsYear);
        }

        [Fact]
        public void GradeCode_SetToEmptyString_ReturnsEmptyString()
        {
            var dto = new GradeDto { GradeCode = string.Empty };

            Assert.Equal(string.Empty, dto.GradeCode);
        }

        [Theory]
        [InlineData(0.00)]
        [InlineData(-1234.56)]
        [InlineData(9999999.99)]
        public void AvSalary_SetToBoundaryValues_ReturnsCorrectValue(double raw)
        {
            var value = (decimal)raw;
            var dto   = new GradeDto { GradeCode = "X", AvSalary = value };

            Assert.Equal(value, dto.AvSalary);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-8.5)]
        [InlineData(2080.0)]
        public void AvLeaveHrs_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new GradeDto { GradeCode = "X", AvLeaveHrs = value };

            Assert.Equal(value, dto.AvLeaveHrs);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-4.25)]
        [InlineData(1040.0)]
        public void AvSickHrs_SetToBoundaryValues_ReturnsCorrectValue(double value)
        {
            var dto = new GradeDto { GradeCode = "X", AvSickHrs = value };

            Assert.Equal(value, dto.AvSickHrs);
        }

        #endregion
    }
}
