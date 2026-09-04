using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Areas.FPS.Models;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.DivisionViewModelTest
{
    public class DivisionViewModelTests
    {
        private static IList<ValidationResult> ValidateModel(DivisionViewModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static DivisionViewModel CreateValidModel()
        {
            return new DivisionViewModel
            {
                DivisionId = 1,
                AgencyId = 1,
                DivName = "VSD",
                CentOverhead = 10m
            };
        }

        [Theory]
        [InlineData("V")]
        [InlineData("VSD")]
        [InlineData("ABCDEFGHIJ")]
        public void DivName_WithinDatabaseLength_PassesValidation(string divName)
        {
            var model = CreateValidModel();
            model.DivName = divName;

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(DivisionViewModel.DivName)));
        }

        [Theory]
        [InlineData("ABCDEFGHIJK")]
        [InlineData("dfshdgvsgdg")]
        public void DivName_ExceedingDatabaseLength_FailsValidation(string divName)
        {
            var model = CreateValidModel();
            model.DivName = divName;

            var results = ValidateModel(model);

            var error = Assert.Single(results, r => r.MemberNames.Contains(nameof(DivisionViewModel.DivName)));
            Assert.Equal("Division Name cannot exceed 10 characters", error.ErrorMessage);
        }

        [Fact]
        public void DivName_WhenEmpty_FailsRequiredValidation()
        {
            var model = CreateValidModel();
            model.DivName = string.Empty;

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "Division Name is required");
        }

        [Fact]
        public void DivName_StringLengthAttribute_MatchesDatabaseColumnLength()
        {
            var attribute = typeof(DivisionViewModel)
                .GetProperty(nameof(DivisionViewModel.DivName))!
                .GetCustomAttributes(typeof(StringLengthAttribute), inherit: false)
                .Cast<StringLengthAttribute>()
                .Single();

            Assert.Equal(10, attribute.MaximumLength);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(1010101010)]
        [InlineData(int.MaxValue)]
        public void DivisionId_WithinIntegerRange_PassesValidation(int divisionId)
        {
            var model = CreateValidModel();
            model.DivisionId = divisionId;

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(DivisionViewModel.DivisionId)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void DivisionId_OutsidePositiveRange_FailsValidation(int divisionId)
        {
            var model = CreateValidModel();
            model.DivisionId = divisionId;

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "Division ID must be a positive number");
        }

        [Fact]
        public void DivisionId_WhenNull_FailsRequiredValidation()
        {
            var model = CreateValidModel();
            model.DivisionId = null;

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "Division ID is required");
        }

        [Fact]
        public void ValidModel_ProducesNoValidationErrors()
        {
            var model = CreateValidModel();

            var results = ValidateModel(model);

            Assert.Empty(results);
        }
    }
}
