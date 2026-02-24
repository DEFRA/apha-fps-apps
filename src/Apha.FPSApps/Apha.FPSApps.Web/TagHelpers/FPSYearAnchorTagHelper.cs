using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-action")]
    public class FPSYearAnchorTagHelper : AnchorTagHelper
    {
        private readonly IFPSYearContext _fy;

        public FPSYearAnchorTagHelper(
            IHtmlGenerator generator,
            IFPSYearContext fy)
            : base(generator)
        {
            _fy = fy;
        }
        
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var href = output.Attributes["href"]?.Value?.ToString();

            if (string.IsNullOrEmpty(href) || href.Contains("year="))
                return;

            var separator = href.Contains("?") ? "&" : "?";
            output.Attributes.SetAttribute("href", $"{href}{separator}year={_fy.Year}");

        }
    }
}
