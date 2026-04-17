using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.PIMS.Models;

public class AddEditCommentViewModel
{
    public int Commentno { get; set; }
    [Required(ErrorMessage = "Project is required")]
    [Display(Name = "Project")]
    public string? Project { get; set; }
    [Required(ErrorMessage = "Year is required")]
    [Display(Name = "Year")]
    public int? Year { get; set; }
    [Required(ErrorMessage = "Topic is required")]
    [Display(Name = "Topic")]
    public string? Topic { get; set; } 
    [Required]
    public string? Commenttext { get; set; }
    public bool IsAddingNew { get; set; }

    public List<SelectListItem> YearOptions { get; set; } = [];
    public List<SelectListItem> TopicOptions { get; set; } = [];
}
