using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class ProjectStaffPlan
    {
        public short? Year { get; set; }

        public string? Parentproject { get; set; }

        public string? Pcgrade { get; set; }

        public string? Workgroupgrade { get; set; }

        public string? Name { get; set; }

        public double? Plannedhours { get; set; }

        public decimal? Rate { get; set; }

        public decimal? Cost { get; set; }
    }
}
