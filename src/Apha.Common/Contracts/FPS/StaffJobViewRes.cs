using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.FPS
{
    public class StaffJobViewRes
    {
        public string? StaffID { get; set; }
        public string? JobCode { get; set; }
        public double PlannedHours { get; set; }
        public string? Name { get; set; }
        public string? WorkGroupGrade { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? StaffCost { get; set; }
        public string? GradeCode { get; set; }
        public string? WorkGroup { get; set; }
        public string? SectorName { get; set; }
        public double Days { get; set; }
        public string? ZtDescription { get; set; }
    }
}
