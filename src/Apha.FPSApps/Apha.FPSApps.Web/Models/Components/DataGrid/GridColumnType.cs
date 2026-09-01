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
        RoundTwoDecimal, // decimal/double/string-backed decimal, formatted as £#,##0.00
        GbpValueRoundedAccounting // £0 rounded; negatives shown as - (£2,569) in red
    }
}
