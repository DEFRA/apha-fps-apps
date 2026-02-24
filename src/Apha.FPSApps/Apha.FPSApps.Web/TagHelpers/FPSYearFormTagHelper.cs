using Apha.FPSApps.Web.Handler;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Apha.FPSApps.Web.TagHelpers
{
    [HtmlTargetElement("form")]
    public class FPSYearFormTagHelper : ITagHelper
    {
        private readonly IFPSYearContext _fy;

        public FPSYearFormTagHelper(IFPSYearContext fy)
        {
            _fy = fy;
        }

        public int Order => 0;

        public void Init(TagHelperContext context)
        {

        }

        public Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context.Items.ContainsKey("FY_ADDED"))
                return Task.CompletedTask;

            // Append hidden field to form
            output.PostContent.AppendHtml(
                $"<input type='hidden' id='FPSYear' name='FPSYear' value='{_fy.Year}' />");

            context.Items["FY_ADDED"] = true;

            return Task.CompletedTask;
        }
    }
}
