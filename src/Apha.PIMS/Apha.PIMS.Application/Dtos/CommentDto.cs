using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.Application.Dtos
{
    public class CommentDto
    {
        public int Commentno { get; set; }
        public string? Project { get; set; }
        public int? Year { get; set; }
        public string? Topic { get; set; }
        public string? Commenttext { get; set; }
        public string? Madeby { get; set; }
        public DateTime? Dateentered { get; set; }
    }
}
