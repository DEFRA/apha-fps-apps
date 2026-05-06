using System.ComponentModel.DataAnnotations;

namespace Apha.Costbook.Application.Dtos;

public class StaffRequirementDto
{
    public int SrIdentity { get; set; }
    [Required(ErrorMessage = "Project is required.")]
    public string? Project { get; set; }

    [Required(ErrorMessage = "Year is required.")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "WG Grade is required.")]
    public string WgGrade { get; set; } = null!;
    [Required(ErrorMessage = "Name is required.")]
    public string? Name { get; set; }
    [Required(ErrorMessage = "Hours is required.")]
    public double? Nohours { get; set; }
    [Required(ErrorMessage = "Days is required.")]
    public double? Nodays { get; set; }
    [Required(ErrorMessage = "Rate is required.")]
    public double? Chargerate { get; set; }
    [Required(ErrorMessage = "Cost is required.")]
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
