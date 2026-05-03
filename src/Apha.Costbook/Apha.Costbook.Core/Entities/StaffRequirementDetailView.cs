namespace Apha.Costbook.Core.Entities;

/// <summary>
/// Read model for the enriched staff requirement query
/// (equivalent of MS Access qryStaffReqGrade).
/// </summary>
public class StaffRequirementDetailView
{
    public int SrIdentity { get; set; }
    public string? Project { get; set; }
    public int? Year { get; set; }
    public string WgGrade { get; set; } = null!;
    public string? Name { get; set; }
    public double? Nohours { get; set; }
    public double? Nodays { get; set; }
    public double? Chargerate { get; set; }
    public double? Payrate { get; set; }
    public double? Npr { get; set; }
    public double? Ohr { get; set; }

    // From WorkGroupGrade join
    public string? WorkGroup { get; set; }
    public string? GradeCode { get; set; }

    // From Project join
    public string? Programme { get; set; }
    public double? EuroConvRate { get; set; }

    // From EuGradeConversion DLookup join
    public string? EuGrade { get; set; }
}