using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class ProjectDetailRes
    {
        public string? Parentproject { get; set; }
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
