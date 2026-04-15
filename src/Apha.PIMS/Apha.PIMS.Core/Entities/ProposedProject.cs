using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities
{

    public partial class ProposedProject
    {
        public int Id { get; set; }

        public string Parentproject { get; set; } = null!;

        public string? Projecttitle { get; set; }

        public string? Program { get; set; }

        public string? Customer { get; set; }

        public string? Manager { get; set; }

        public string? Projectstatus { get; set; }

        public string? Costbookno { get; set; }

        public string? Disease { get; set; }

        public string? Reason { get; set; }
    }
}