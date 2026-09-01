using Apha.FPSApps.Web.Areas.FPS.Models;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.PlanStaffZTCodeItemViewModelTest
{
    public class PlanStaffZTCodeItemViewModelTests
    {
        private static IList<ValidationResult> ValidateModel(PlanStaffZTCodeItemViewModel model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static PlanStaffZTCodeItemViewModel CreateValidModel(double plannedHours)
        {
            return new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = "ZT001",
                PlannedHours = plannedHours
            };
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0.5)]
        [InlineData(40)]
        [InlineData(1000.75)]
        public void PlannedHours_WithNonNegativeValue_PassesValidation(double plannedHours)
        {
            var model = CreateValidModel(plannedHours);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(PlanStaffZTCodeItemViewModel.PlannedHours)));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-0.5)]
        [InlineData(-40)]
        public void PlannedHours_WithNegativeValue_PassesValidation(double plannedHours)
        {
            // Negative hours are permitted so users can record corrections
            var model = CreateValidModel(plannedHours);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(PlanStaffZTCodeItemViewModel.PlannedHours)));
        }

        [Theory]
        [InlineData(1e10)]
        [InlineData(2147483648.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        public void PlannedHours_WithValueOutsideInt32Range_ValidatesWithoutThrowing(double plannedHours)
        {
            // Guards against RangeAttribute converting via Convert.ToInt32, which threw
            // OverflowException for any double beyond Int32 bounds.
            var model = CreateValidModel(plannedHours);

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(PlanStaffZTCodeItemViewModel.PlannedHours)));
        }

        [Fact]
        public void PlannedHours_WithNaN_FailsValidationWithExpectedMessage()
        {
            var model = CreateValidModel(double.NaN);

            var results = ValidateModel(model);

            var failure = Assert.Single(
                results,
                r => r.MemberNames.Contains(nameof(PlanStaffZTCodeItemViewModel.PlannedHours)));
            Assert.Equal("Hours must be a valid number", failure.ErrorMessage);
        }

        [Fact]
        public void JobCode_WhenMissing_FailsValidation()
        {
            var model = new PlanStaffZTCodeItemViewModel
            {
                StaffID = "S001",
                JobCode = null!,
                PlannedHours = 40
            };

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.MemberNames.Contains(nameof(PlanStaffZTCodeItemViewModel.JobCode)));
        }

        [Fact]
        public void ValidModel_ProducesNoValidationErrors()
        {
            var model = CreateValidModel(40);

            var results = ValidateModel(model);

            Assert.Empty(results);
        }

        [Fact]
        public void OriginalJobCode_DefaultsToNull_AndIsAssignable()
        {
            var model = CreateValidModel(40);
            Assert.Null(model.OriginalJobCode);

            model.OriginalJobCode = "ZT002";

            Assert.Equal("ZT002", model.OriginalJobCode);
            Assert.Empty(ValidateModel(model));
        }
    }
}
