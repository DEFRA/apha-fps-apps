namespace Apha.Common.Contracts.Costbook;

public class StaffRequirementRes
{
    public int SrIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string WgGrade { get; set; } = null!;
    public string? Name { get; set; }
    public double? Nohours { get; set; }
    public double? Nodays { get; set; }
    public double? Chargerate { get; set; }
    public double? StaffCost { get; set; }
    public double? Payrate { get; set; }
    public double? Npr { get; set; }
    public double? Ohr { get; set; }

    // Enriched from qryStaffReqGrade joins
    public string? WorkGroup { get; set; }
    public string? GradeCode { get; set; }
    public string? Programme { get; set; }
    public double? EuroConvRate { get; set; }
    public string? EuGrade { get; set; }
}
