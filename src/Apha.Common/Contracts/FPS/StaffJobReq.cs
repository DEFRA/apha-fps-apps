using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.FPS
{
    public class StaffJobReq
    {
        public string StaffId { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        /// <summary>
        /// The original JobCode before an edit operation (used for composite-key lookup on update).
        /// </summary>
        public string? OriginalJobCode { get; set; }

        public double PlannedHours { get; set; }        

        public int? FpsCalYear { get; set; }
    }
}
