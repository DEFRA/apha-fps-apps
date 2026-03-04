using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPS.Core.Entities
{
    public class StaffJobView
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
    }
}
