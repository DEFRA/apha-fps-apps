namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public enum GridColumnType
    {
        Text,
        Number,
        DecimalNumber,
        Dropdown,
        Checkbox,
        Date,
        DateTime,
        ReadOnly,
        UsdValue, // $0.00
        GbpValue, // £0.00
        GbpValueRounded, // £0 (rounded, no decimals)
        DoubleNumber,
        Percentage, // 0.00%
        PercentageRatio, // ratio value always multiplied by 100, e.g. 0.12 -> 12.00%
        RoundTwoDecimal // decimal/double/string-backed decimal, formatted as £#,##0.00
    }
}
