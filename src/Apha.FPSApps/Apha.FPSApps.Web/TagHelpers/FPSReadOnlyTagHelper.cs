using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("button")]
    [HtmlTargetElement("input", Attributes = "type=submit")]
    [HtmlTargetElement("input", Attributes = "type=button")]
    public class FPSReadOnlyTagHelper : TagHelper
    {
        private readonly IFpsYearContext _fy;

        public FPSReadOnlyTagHelper(IFpsYearContext fy)
        {
            _fy = fy;
        }

        [HtmlAttributeName("allow-edit")]
        public bool AllowEdit { get; set; } = false;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Disable button only when year is read-only AND editing is not explicitly allowed
            // This allows specific buttons to remain enabled even in read-only mode
            if (_fy.IsReadOnly && !AllowEdit)
            {
                output.Attributes.SetAttribute("disabled", "disabled");
            }

            // Remove the allow-edit attribute from final HTML
            output.Attributes.RemoveAll("allow-edit");
        }
    }
}
