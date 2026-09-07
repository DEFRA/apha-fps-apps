using System.ComponentModel.DataAnnotations;
using Apha.FPSApps.Web.Areas.FPS.Models;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.WorkgroupMaintenanceItemTest
{
    public class WorkgroupMaintenanceItemTests
    {
        private static IList<ValidationResult> ValidateModel(WorkgroupMaintenanceItem model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        private static WorkgroupMaintenanceItem CreateValidModel()
        {
            return new WorkgroupMaintenanceItem
            {
                WorkGroupName = "WG001",
                ProfitCentre = "2000",
                Description = "Test description",
                Owner = "Test Owner",
                CentralOverhead = 34m
            };
        }

        [Theory]
        [InlineData("W")]
        [InlineData("WG001")]
        [InlineData("12345678901234567890123456789012345678901234567890")]
        public void WorkGroupName_WithinDatabaseLength_PassesValidation(string workGroupName)
        {
            var model = CreateValidModel();
            model.WorkGroupName = workGroupName;

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(WorkgroupMaintenanceItem.WorkGroupName)));
        }

        [Theory]
        [InlineData("123456789012345678901234567890123456789012345678901")]
        [InlineData("TesttheWorkgroupdhcjhdvchvcsdhvcksdhvcksdhvcksdhvcksdh")]
        public void WorkGroupName_ExceedingDatabaseLength_FailsValidation(string workGroupName)
        {
            var model = CreateValidModel();
            model.WorkGroupName = workGroupName;

            var results = ValidateModel(model);

            var error = Assert.Single(results, r => r.MemberNames.Contains(nameof(WorkgroupMaintenanceItem.WorkGroupName)));
            Assert.Equal("WorkGroup cannot exceed 50 characters", error.ErrorMessage);
        }

        [Fact]
        public void WorkGroupName_WhenEmpty_FailsRequiredValidation()
        {
            var model = CreateValidModel();
            model.WorkGroupName = string.Empty;

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "WorkGroup is required");
        }

        [Theory]
        [InlineData("D")]
        [InlineData("Test description")]
        [InlineData("123456789012345678901234567890123456789012345")]
        public void Description_WithinDatabaseLength_PassesValidation(string description)
        {
            var model = CreateValidModel();
            model.Description = description;

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(WorkgroupMaintenanceItem.Description)));
        }

        [Theory]
        [InlineData("1234567890123456789012345678901234567890123456")]
        [InlineData("wfscasdhckjsdchbsjhbcjshbcsjdhbckjsdhbckjsdhbckjsdh")]
        public void Description_ExceedingDatabaseLength_FailsValidation(string description)
        {
            var model = CreateValidModel();
            model.Description = description;

            var results = ValidateModel(model);

            var error = Assert.Single(results, r => r.MemberNames.Contains(nameof(WorkgroupMaintenanceItem.Description)));
            Assert.Equal("Description cannot exceed 45 characters", error.ErrorMessage);
        }

        [Fact]
        public void Description_WhenNull_PassesValidation()
        {
            var model = CreateValidModel();
            model.Description = null;

            var results = ValidateModel(model);

            Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(WorkgroupMaintenanceItem.Description)));
        }

        [Fact]
        public void ProfitCentre_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.ProfitCentre = new string('P', 51);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "ResourceCentre cannot exceed 50 characters");
        }

        [Fact]
        public void ProfitCentre_WhenEmpty_FailsRequiredValidation()
        {
            var model = CreateValidModel();
            model.ProfitCentre = string.Empty;

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "ResourceCentre is required");
        }

        [Fact]
        public void Owner_ExceedingDatabaseLength_FailsValidation()
        {
            var model = CreateValidModel();
            model.Owner = new string('O', 51);

            var results = ValidateModel(model);

            Assert.Contains(results, r => r.ErrorMessage == "Owner cannot exceed 50 characters");
        }

        [Theory]
        [InlineData(nameof(WorkgroupMaintenanceItem.WorkGroupName), 50)]
        [InlineData(nameof(WorkgroupMaintenanceItem.ProfitCentre), 50)]
        [InlineData(nameof(WorkgroupMaintenanceItem.Owner), 50)]
        [InlineData(nameof(WorkgroupMaintenanceItem.Description), 45)]
        public void StringLengthAttributes_MatchDatabaseColumnLengths(string propertyName, int expectedLength)
        {
            var attribute = typeof(WorkgroupMaintenanceItem)
                .GetProperty(propertyName)!
                .GetCustomAttributes(typeof(StringLengthAttribute), inherit: false)
                .Cast<StringLengthAttribute>()
                .Single();

            Assert.Equal(expectedLength, attribute.MaximumLength);
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
