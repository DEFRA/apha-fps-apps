using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class ProjectRes
    {
        public string Parentproject { get; set; } = null!;
        public string? Projecttitle { get; set; }
        public string? Disease { get; set; }
        public string? Contract { get; set; }
        public string? Projectstatus { get; set; }
        public string? Shorttitle { get; set; }
        public string? Costbookno { get; set; }
    }
}
