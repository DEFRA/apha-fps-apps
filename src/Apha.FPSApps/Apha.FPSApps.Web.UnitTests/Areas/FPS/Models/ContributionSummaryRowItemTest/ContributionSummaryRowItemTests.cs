using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Areas.FPS.Models.ContributionSummaryRowItemTest
{
    public class ContributionSummaryRowItemTests
    {
        private static PropertyInfo GetProperty(string name)
            => typeof(ContributionSummaryRowItem).GetProperty(name)!;

        private static GridColumnAttribute GetGridColumn(string name)
            => GetProperty(name).GetCustomAttribute<GridColumnAttribute>()!;

        private static string GetDisplayName(string name)
            => GetProperty(name).GetCustomAttribute<DisplayAttribute>()!.Name!;

        #region PctPlannedDisplay

        [Theory]
        [InlineData(100d, 0.12d, "12.00%")]
        [InlineData(100d, 2.14d, "214.00%")]
        [InlineData(100d, 1d, "100.00%")]
        [InlineData(100d, 0.005d, "0.50%")]
        [InlineData(100d, -0.25d, "-25.00%")]
        public void PctPlannedDisplay_WhenCalculable_FormatsRatioAsPercentage(
            double availableHours, double ratio, string expected)
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = availableHours, PctPlanned = ratio };

            // Act & Assert
            Assert.Equal(expected, item.PctPlannedDisplay);
        }

        [Fact]
        public void PctPlannedDisplay_WhenRatioIsZero_ReturnsZeroPercent()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = 100d, PctPlanned = 0d };

            // Act & Assert
            Assert.Equal("0.00%", item.PctPlannedDisplay);
        }

        [Fact]
        public void PctPlannedDisplay_WhenAvailableHoursIsZero_ReturnsExclamation()
        {
            // Arrange — Access shows "!" on division by zero
            var item = new ContributionSummaryRowItem { AvHrs = 0d, PctPlanned = 0.5d };

            // Act & Assert
            Assert.Equal("!", item.PctPlannedDisplay);
        }

        [Fact]
        public void PctPlannedDisplay_WhenAvailableHoursIsNull_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = null, PctPlanned = 0.5d };

            // Act & Assert
            Assert.Equal("!", item.PctPlannedDisplay);
        }

        [Fact]
        public void PctPlannedDisplay_WhenRatioIsNull_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = 100d, PctPlanned = null };

            // Act & Assert
            Assert.Equal("!", item.PctPlannedDisplay);
        }

        [Fact]
        public void PctPlannedDisplay_WhenAvailableHoursAndRatioAreNull_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem();

            // Act & Assert
            Assert.Equal("!", item.PctPlannedDisplay);
        }

        #endregion

        #region PctAssuredPlannedDisplay

        [Theory]
        [InlineData(100d, 0.12d, "12.00%")]
        [InlineData(100d, 2.14d, "214.00%")]
        [InlineData(100d, 0.5d, "50.00%")]
        [InlineData(100d, -1.5d, "-150.00%")]
        public void PctAssuredPlannedDisplay_WhenCalculable_FormatsRatioAsPercentage(
            double availableHours, double ratio, string expected)
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = availableHours, PctAssuredPlanned = ratio };

            // Act & Assert
            Assert.Equal(expected, item.PctAssuredPlannedDisplay);
        }

        [Fact]
        public void PctAssuredPlannedDisplay_WhenAvailableHoursIsZero_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = 0d, PctAssuredPlanned = 0.5d };

            // Act & Assert
            Assert.Equal("!", item.PctAssuredPlannedDisplay);
        }

        [Fact]
        public void PctAssuredPlannedDisplay_WhenAvailableHoursIsNull_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = null, PctAssuredPlanned = 0.5d };

            // Act & Assert
            Assert.Equal("!", item.PctAssuredPlannedDisplay);
        }

        [Fact]
        public void PctAssuredPlannedDisplay_WhenRatioIsNull_ReturnsExclamation()
        {
            // Arrange
            var item = new ContributionSummaryRowItem { AvHrs = 100d, PctAssuredPlanned = null };

            // Act & Assert
            Assert.Equal("!", item.PctAssuredPlannedDisplay);
        }

        [Fact]
        public void PercentageDisplays_AreIndependentOfEachOther()
        {
            // Arrange — only the planned ratio is calculable
            var item = new ContributionSummaryRowItem
            {
                AvHrs             = 200d,
                PctPlanned        = 0.25d,
                PctAssuredPlanned = null
            };

            // Act & Assert
            Assert.Equal("25.00%", item.PctPlannedDisplay);
            Assert.Equal("!",      item.PctAssuredPlannedDisplay);
        }

        #endregion

        #region Grid column metadata

        [Fact]
        public void RawPercentageProperties_AreHiddenFromTheGrid()
        {
            // The raw ratio columns are replaced by their display counterparts
            Assert.False(GetGridColumn(nameof(ContributionSummaryRowItem.PctPlanned)).IsVisible);
            Assert.False(GetGridColumn(nameof(ContributionSummaryRowItem.PctAssuredPlanned)).IsVisible);
        }

        [Theory]
        [InlineData(nameof(ContributionSummaryRowItem.PctPlannedDisplay), 8)]
        [InlineData(nameof(ContributionSummaryRowItem.PctAssuredPlannedDisplay), 11)]
        public void PercentageDisplayColumns_AreRenderedAsRightAlignedText(string propertyName, int expectedOrder)
        {
            // Arrange
            var column = GetGridColumn(propertyName);

            // Assert — text type so "!" renders, numeric CSS so it stays right aligned
            Assert.Equal(expectedOrder, column.Order);
            Assert.Equal(GridColumnType.Text, column.Type);
            Assert.Equal("govuk-table__cell--numeric", column.CssClass);
        }

        [Theory]
        [InlineData(nameof(ContributionSummaryRowItem.AppHours), "PlanHrs")]
        [InlineData(nameof(ContributionSummaryRowItem.AppFec), "FEC")]
        [InlineData(nameof(ContributionSummaryRowItem.PctPlannedDisplay), "% Planned")]
        [InlineData(nameof(ContributionSummaryRowItem.PctAssuredPlannedDisplay), "% Planned")]
        public void AssuredPlannedTimeColumns_UseAccessAlignedHeadings(string propertyName, string expectedHeading)
        {
            // Assert
            Assert.Equal(expectedHeading, GetDisplayName(propertyName));
        }

        #endregion
    }
}
