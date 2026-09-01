using Apha.FPSApps.Web.Models.Components.DataGrid;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Models.Components.DataGrid
{
    public class GridHelpersTests
    {
        private sealed class SimpleRow
        {
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
        }

        private sealed class DynamicRow
        {
            public string Key { get; set; } = string.Empty;
            public IDictionary<string, object?> Values { get; } = new Dictionary<string, object?>();
        }

        private sealed class NonDictionaryValuesRow
        {
            public string Values { get; set; } = "not-a-dictionary";
        }

        [Fact]
        public void GetPropertyValue_ReturnsValue_WhenPropertyExists()
        {
            var row = new SimpleRow { Name = "Acme", Amount = 42m };

            Assert.Equal("Acme", GridHelpers.GetPropertyValue(row, nameof(SimpleRow.Name)));
            Assert.Equal(42m, GridHelpers.GetPropertyValue(row, nameof(SimpleRow.Amount)));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_ForMissingProperty_EvenWhenValuesDictionaryPresent()
        {
            // Dynamic/pivot rows are now dictionary-backed and resolved via the
            // IDictionary overload; the reflection-based "Values" fallback is not used.
            var row = new DynamicRow { Key = "K1" };
            row.Values["WG1"] = 100m;

            Assert.Null(GridHelpers.GetPropertyValue(row, "WG1"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsDynamicValue_FromDictionaryRow()
        {
            var row = new Dictionary<string, string?>
            {
                ["Key"] = "K1",
                ["WG1"] = "100",
                ["WG2"] = null
            };

            Assert.Equal("100", GridHelpers.GetPropertyValue(row, "WG1"));
            Assert.Null(GridHelpers.GetPropertyValue(row, "WG2"));
            Assert.Null(GridHelpers.GetPropertyValue(row, "DoesNotExist"));
        }

        [Fact]
        public void GetPropertyValue_PrefersRealProperty_OverValuesDictionary()
        {
            var row = new DynamicRow { Key = "K1" };
            row.Values["Key"] = "from-dictionary";

            // "Key" is a real property, so the property value wins over the dictionary entry.
            Assert.Equal("K1", GridHelpers.GetPropertyValue(row, "Key"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenPropertyMissingAndNotInValues()
        {
            var row = new DynamicRow { Key = "K1" };

            Assert.Null(GridHelpers.GetPropertyValue(row, "DoesNotExist"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenPropertyMissingAndNoValuesDictionary()
        {
            var row = new SimpleRow { Name = "Acme" };

            Assert.Null(GridHelpers.GetPropertyValue(row, "Missing"));
        }

        [Fact]
        public void GetPropertyValue_ReturnsNull_WhenValuesPropertyIsNotDictionary()
        {
            var row = new NonDictionaryValuesRow();

            Assert.Null(GridHelpers.GetPropertyValue(row, "Missing"));
        }

        private static DataGridColumn AccountingColumn() =>
            new() { PropertyName = "OffTarget", ColumnType = GridColumnType.GbpValueRoundedAccounting };

        [Theory]
        [InlineData(-2569, "- (£2,569)")]
        [InlineData(-0.6, "- (£1)")]
        [InlineData(0, "£0")]
        [InlineData(214, "£214")]
        [InlineData(1234567, "£1,234,567")]
        public void FormatValue_Accounting_WrapsNegativesInBrackets(decimal value, string expected)
        {
            Assert.Equal(expected, GridHelpers.FormatValue(value, AccountingColumn()));
        }

        [Fact]
        public void FormatValue_Accounting_HandlesDoubleValues()
        {
            Assert.Equal("- (£2,569)", GridHelpers.FormatValue(-2568.7d, AccountingColumn()));
        }

        [Fact]
        public void FormatValue_Accounting_ReturnsEmpty_ForNull()
        {
            Assert.Equal(string.Empty, GridHelpers.FormatValue(null, AccountingColumn()));
        }

        [Fact]
        public void FormatValue_GbpValueRounded_StillUsesMinusSign()
        {
            var column = new DataGridColumn { PropertyName = "Profit", ColumnType = GridColumnType.GbpValueRounded };

            Assert.Equal("-£2,569", GridHelpers.FormatValue(-2569m, column));
        }

        [Theory]
        [InlineData(-2569)]
        [InlineData(-1)]
        public void GetValueCssClass_ReturnsNegativeClass_ForNegativeAccountingValues(decimal value)
        {
            Assert.Equal("grid-negative-value", GridHelpers.GetValueCssClass(value, AccountingColumn()));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(214)]
        public void GetValueCssClass_ReturnsEmpty_ForNonNegativeAccountingValues(decimal value)
        {
            Assert.Equal(string.Empty, GridHelpers.GetValueCssClass(value, AccountingColumn()));
        }

        [Fact]
        public void GetValueCssClass_HandlesNonDecimalNegativeValues()
        {
            Assert.Equal("grid-negative-value", GridHelpers.GetValueCssClass(-5d, AccountingColumn()));
            Assert.Equal("grid-negative-value", GridHelpers.GetValueCssClass(-5, AccountingColumn()));
            Assert.Equal("grid-negative-value", GridHelpers.GetValueCssClass("-5", AccountingColumn()));
            Assert.Equal(string.Empty, GridHelpers.GetValueCssClass("not-a-number", AccountingColumn()));
        }

        [Fact]
        public void GetValueCssClass_ReturnsEmpty_ForNullValue()
        {
            Assert.Equal(string.Empty, GridHelpers.GetValueCssClass(null, AccountingColumn()));
        }

        [Fact]
        public void GetValueCssClass_ReturnsEmpty_ForOtherColumnTypes()
        {
            var column = new DataGridColumn { PropertyName = "Profit", ColumnType = GridColumnType.GbpValueRounded };

            Assert.Equal(string.Empty, GridHelpers.GetValueCssClass(-2569m, column));
        }

        [Fact]
        public void IsNumericColumn_TreatsAccountingColumnAsNumeric()
        {
            Assert.True(GridHelpers.IsNumericColumn(GridColumnType.GbpValueRoundedAccounting));
            Assert.Equal("govuk-table__cell--numeric", GridHelpers.GetAlignmentCssClass(AccountingColumn()));
        }
    }
}
