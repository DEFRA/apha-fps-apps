using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities
{

    public partial class Comment
    {
        public int Commentno { get; set; }

        public string Project { get; set; } = null!;

        public short Year { get; set; }

        public DateTime? Dateentered { get; set; }

        public string Topic { get; set; } = null!;

        public string? Commenttext { get; set; }

        public string? Madeby { get; set; }
    }
}
