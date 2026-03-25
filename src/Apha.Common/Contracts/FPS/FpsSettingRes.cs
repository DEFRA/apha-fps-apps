using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts.FPS
{
    public class FpsSettingRes
    {
        public string Id { get; set; } = string.Empty;
        public string? Setting { get; set; }
        public string? Notes { get; set; }
        public string? TestSetting { get; set; }
        public int? FpsCalYear { get; set; }
    }
}
