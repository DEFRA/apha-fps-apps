using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Models.Components.YearSelector
{
    public class YearSelectorViewModel
    {
        public IReadOnlyList<SelectListItem> Years { get; init; } = [];
        public int SelectedYear { get; init; }
    }
}