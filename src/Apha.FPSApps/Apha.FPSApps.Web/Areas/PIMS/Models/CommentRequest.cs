namespace Apha.FPSApps.Web.Areas.PIMS.Models
{
    public record CommentRequest(
         string? Project,
         int? Year,
         string? Topic,
         string? Comment);
}
