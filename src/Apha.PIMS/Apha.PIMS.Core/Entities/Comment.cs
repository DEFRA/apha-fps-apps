using System;
using System.Collections.Generic;

namespace Apha.PIMS.Core.Entities
{

    public partial class Comment
    {
        public int CommentNo { get; set; }

        public string Project { get; set; } = null!;

        public short Year { get; set; }

        public DateTime? DateEntered { get; set; }

        public string Topic { get; set; } = null!;

        public string? CommentText { get; set; }
        public string? MadeBy { get; set; }
    }
}
