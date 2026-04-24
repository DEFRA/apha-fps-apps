using System;
using System.Collections.Generic;
namespace Apha.Costbook.Core.Entities;

public partial class WorkGroupGrade
{
    public string WgGrade { get; set; } = null!;

    public string ProfitCentreGrade { get; set; } = null!;
    public string GradeCode { get; set; } = null!;

    public string WorkGroup { get; set; } = null!;

    public decimal? ChargeRateWg { get; set; }

    public decimal? DirectRateWg { get; set; }
    public decimal? PayRateWg { get; set; }

    public decimal? NprWg { get; set; }

    public decimal? OhrWg { get; set; }
    public decimal? AvSalary { get; set; }

    public string? HrsChangedBy { get; set; }

    public int FpsYear { get; set; }   

    public virtual ProfitCentreGrade ProfitCentreGradeNavigation { get; set; } = null!;

    
}
