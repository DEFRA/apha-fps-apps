using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectDetail
    {
        public string Parentproject { get; set; } = null!;
        public string? Version { get; set; }
        public string? FileRef { get; set; }
        public string? CustomerRef { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? CostbookNumber { get; set; }
        public int? Riskid { get; set; }
        public bool UseProjectYears { get; set; }
        public DateOnly? RevisedEndDate { get; set; }
        public DateOnly? ClosedDate { get; set; }
    }
}
