using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.FPS
{
    public class ProgramRes
    {
        public string ProgramNo { get; set; } = null!;

        public string? ProgramName { get; set; }

        public string? Directorate { get; set; }

        public string? Minim { get; set; }

        public string? SectorName { get; set; }

        public string? Customer { get; set; }

        public decimal? Target { get; set; }

        public string? Manager { get; set; }

        public int? FpsCalYear { get; set; }
    }
}
