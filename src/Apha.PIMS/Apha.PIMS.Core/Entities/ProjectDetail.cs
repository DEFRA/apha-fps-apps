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
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CostbookNumber { get; set; }
        public int? Riskid { get; set; }
        public bool UseProjectYears { get; set; }
        public DateTime? RevisedEndDate { get; set; }
        public DateTime? ClosedDate { get; set; }
    }
}
