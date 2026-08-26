using Apha.FPSApps.Web.Areas.FPS.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.FPS.UserPermission
{
    public class UserPermissionViewModelTests
    {
        [Theory]
        [InlineData(nameof(UserPermissionViewModel.UserId), "User_ID")]
        [InlineData(nameof(UserPermissionViewModel.Username), "SQL UserName")]
        [InlineData(nameof(UserPermissionViewModel.Dt2Username), "DT2 UserName")]
        public void Property_DisplayAttribute_UsesAgreedColumnHeading(string propertyName, string expectedName)
        {
            // Arrange
            var property = typeof(UserPermissionViewModel).GetProperty(propertyName);

            // Act
            var attribute = property?.GetCustomAttribute<DisplayAttribute>();

            // Assert — regression guard for defect 238: headings must read
            // "User_ID", "SQL UserName" and "DT2 UserName".
            Assert.NotNull(property);
            Assert.NotNull(attribute);
            Assert.Equal(expectedName, attribute!.Name);
        }

        [Fact]
        public void Username_RequiredAttribute_UsesUserNameCasingInErrorMessage()
        {
            // Arrange
            var property = typeof(UserPermissionViewModel).GetProperty(nameof(UserPermissionViewModel.Username));

            // Act
            var attribute = property?.GetCustomAttribute<RequiredAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal("SQL UserName is required", attribute!.ErrorMessage);
        }

        [Theory]
        [InlineData(nameof(UserPermissionViewModel.Username), 50, "SQL UserName cannot exceed 50 characters")]
        [InlineData(nameof(UserPermissionViewModel.Dt2Username), 100, "DT2 UserName cannot exceed 100 characters")]
        public void Property_StringLengthAttribute_UsesUserNameCasingInErrorMessage(
            string propertyName, int expectedLength, string expectedMessage)
        {
            // Arrange
            var property = typeof(UserPermissionViewModel).GetProperty(propertyName);

            // Act
            var attribute = property?.GetCustomAttribute<StringLengthAttribute>();

            // Assert
            Assert.NotNull(attribute);
            Assert.Equal(expectedLength, attribute!.MaximumLength);
            Assert.Equal(expectedMessage, attribute.ErrorMessage);
        }

        [Fact]
        public void Username_WhenExceedingMaxLength_ReportsUserNameCasedValidationError()
        {
            // Arrange
            var model = new UserPermissionViewModel
            {
                Username = new string('a', 51),
                UserEmail = "user@example.com"
            };
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(
                model, new ValidationContext(model), results, validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage == "SQL UserName cannot exceed 50 characters");
        }

        [Fact]
        public void Username_WhenMissing_ReportsUserNameCasedRequiredError()
        {
            // Arrange
            var model = new UserPermissionViewModel { UserEmail = "user@example.com" };
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(
                model, new ValidationContext(model), results, validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(results, r => r.ErrorMessage == "SQL UserName is required");
        }
    }
}
