using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Common.Contracts.PIMS
{
    public class CommentReq
    {
        public string? Project { get; set; }
        public int? Year { get; set; }
        public string? Topic { get; set; }
        public string? Comment { get; set; }
        public string? Madeby { get; set; }
        public int Commentno { get; set; }
        public string? Commenttext { get; set; }
        public DateTime? Dateentered { get; set; }
    }
}
