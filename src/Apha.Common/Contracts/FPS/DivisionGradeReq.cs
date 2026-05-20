namespace Apha.Common.Contracts.FPS
{
    using System.ComponentModel.DataAnnotations;

    public class DivisionGradeReq
    {
        [Required(ErrorMessage = "Division Grade is required.")]
        [Display(Name = "Division Grade")]
        public string DivisionGradeCode { get; set; } = null!;

        [Required(ErrorMessage = "Grade code is required.")]
        public string GradeCode { get; set; } = null!;

        [Required(ErrorMessage = "Division is required.")]
        public string Division { get; set; } = null!;

        public decimal? ChargeRate { get; set; }
        public decimal? DirectRate { get; set; }
        public decimal? PayRate { get; set; }
        public decimal? Npr { get; set; }
        public decimal? Ohr { get; set; }
    }
}
