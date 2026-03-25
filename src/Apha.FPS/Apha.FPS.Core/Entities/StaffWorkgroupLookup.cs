using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPS.Core.Entities
{
    public class StaffWorkgroupLookup
    {
        public string StaffID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string WorkGroupGrade { get; set; } = string.Empty;
        public double HrsAvail { get; set; }
    }
}
