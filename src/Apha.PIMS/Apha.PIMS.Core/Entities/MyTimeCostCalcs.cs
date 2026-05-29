using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Core.Entities
{
    public partial class MyTimeCostCalcs
    {
        public short Year { get; set; }

        public string Workgroup { get; set; } = null!;

        public string Jobcode { get; set; } = null!;

        public string Project { get; set; } = null!;

        public double Month { get; set; }

        public string Staffid { get; set; } = null!;

        public string? Gradecode { get; set; }

        public string? Name { get; set; }

        public decimal? Chargerate { get; set; }

        public string? Class { get; set; }

        public double? Time { get; set; }

        public double? Cost { get; set; }

        public string? Division { get; set; }

        public string? Jobcodeold { get; set; }

        public decimal? Pay { get; set; }

        public decimal? Nonpay { get; set; }

        public decimal? Overhead { get; set; }
    }
}
