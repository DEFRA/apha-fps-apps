using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.FPS.ProjectMisc
{
    public class ProjectMiscItemTests
    {
        [Theory]
        [InlineData(nameof(ProjectMiscItem.CostCentre), "CostCentre")]
        [InlineData(nameof(ProjectMiscItem.OracleProjectCode), "OracleProjectCode")]
        [InlineData(nameof(ProjectMiscItem.SubAccountCode), "SubAccountCode")]
        public void Property_DisplayAttribute_HasNoSpacesInFieldName(string propertyName, string expectedName)
        {
            // Arrange
            var property = typeof(ProjectMiscItem).GetProperty(propertyName);

            // Act
            var attribute = property?.GetCustomAttribute<DisplayAttribute>();

            // Assert — regression guard for defect 246: the Misc Project Data grid
            // headings must not contain spaces.
            Assert.NotNull(property);
            Assert.NotNull(attribute);
            Assert.Equal(expectedName, attribute!.Name);
            Assert.DoesNotContain(" ", attribute.Name!, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("CostCentre")]
        [InlineData("OracleProjectCode")]
        [InlineData("SubAccountCode")]
        public void GetColumnsDefination_RendersSpaceFreeColumnHeading(string expectedHeading)
        {
            // Act
            var columns = GridDataProvider.GetColumnsDefination<ProjectMiscItem>();

            // Assert — the grid builds its headings from the Display attribute,
            // so the rendered column header must also be space-free.
            var column = columns.FirstOrDefault(c => c.PropertyName == expectedHeading);
            Assert.NotNull(column);
            Assert.Equal(expectedHeading, column!.DisplayName);
        }
    }
}
