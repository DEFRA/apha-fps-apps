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
        GbpValueParens   // £#,##0.00 with negatives as (£#,##0.00) — used by Department Income grids
    }
}
