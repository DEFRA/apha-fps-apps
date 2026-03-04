using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPS.Application.Dtos
{
    public class StaffJobDto
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public double PlannedHours { get; set; }

        public DateTime? SysTimestamp { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
