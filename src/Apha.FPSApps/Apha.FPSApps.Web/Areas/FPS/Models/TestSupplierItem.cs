using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestSupplierItem
    {
        [GridColumn(IsVisible = false)]
        public string TestCode { get; set; }

        [Required(ErrorMessage = "Project (Buyer) is required.")]
        [Display(Name = "Project")]
        [GridColumn(Order = 1)]
        public string Buyer { get; set; }

        [Display(Name = "Project Manager")]
        [GridColumn(Order = 2)]
        public string? ProjectManager { get; set; }

        [Display(Name = "No Tests")]
        [Range(0, double.MaxValue, ErrorMessage = "No Tests must be 0 or greater.")]
        [GridColumn(Order = 3)]
        public double? NoTests { get; set; }

        [Display(Name = "Test Price")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        [GridColumn(Order = 4)]
        public decimal? TestPrice { get; set; }

        [Display(Name = "Test Cost")]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = false)]
        [GridColumn(Order = 5)]
        public decimal TestCost { get; set; }

        [Display(Name = "Project Status")]
        [GridColumn(Order = 6)]
        public string? ProjectStatus { get; set; }

        [GridColumn(IsVisible = false)]
            public List<SelectListItem>? ProjectStatusOptions { get; set; }

            [GridColumn(IsVisible = false)]
            public string? ProjectBuyerCode { get; set; }

            [GridColumn(IsVisible = false)]
            public string? TestBuyerCode { get; set; }
        }
}
