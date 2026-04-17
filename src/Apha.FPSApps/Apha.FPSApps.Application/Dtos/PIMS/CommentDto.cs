using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class CommentDto
    {
        public int Commentno { get; set; }
        [Required(ErrorMessage = "Project is required")]
        public string? Project { get; set; }
        [Required(ErrorMessage = "Year is required")]
        public int? Year { get; set; }
        [Required(ErrorMessage = "Topic is required")]
        public string? Topic { get; set; }
        public string? Comment { get; set; }
        public string? Commenttext { get; set; }
        public string? Madeby { get; set; }
        public DateTime? Dateentered { get; set; }

    }
}
