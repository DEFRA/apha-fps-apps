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
        Percentage // e.g. 0.75 → "75.00%"
    }
}
