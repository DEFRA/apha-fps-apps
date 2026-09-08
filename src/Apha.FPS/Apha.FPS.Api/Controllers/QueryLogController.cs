using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// Lightweight diagnostics page for viewing and downloading the query-profiling CSV log(s).
    /// Browse to /query-logs to see the list of available files.
    /// </summary>
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("query-logs")]
    public class QueryLogController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public QueryLogController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        /// <summary>
        /// Renders a small HTML page listing the query-profiling log files with download links.
        /// </summary>
        [HttpGet("")]
        public ContentResult Index()
        {
            var directory = GetLogDirectory();

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">");
            sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.Append("<title>Query Profiling Logs</title>");
            sb.Append("<style>");
            sb.Append("body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;color:#1d2939;}");
            sb.Append("h1{font-size:1.4rem;}");
            sb.Append("table{border-collapse:collapse;width:100%;max-width:900px;margin-top:1rem;}");
            sb.Append("th,td{border:1px solid #d0d5dd;padding:.5rem .75rem;text-align:left;font-size:.9rem;}");
            sb.Append("th{background:#f2f4f7;}");
            sb.Append("a.btn{display:inline-block;padding:.35rem .75rem;background:#1570ef;color:#fff;");
            sb.Append("text-decoration:none;border-radius:4px;font-size:.85rem;}");
            sb.Append("a.btn:hover{background:#175cd3;}");
            sb.Append(".empty{color:#667085;margin-top:1rem;}");
            sb.Append("</style></head><body>");
            sb.Append("<h1>Query Profiling Logs</h1>");

            if (directory is null || !Directory.Exists(directory))
            {
                sb.Append("<p class=\"empty\">No log directory found.</p>");
            }
            else
            {
                var files = Directory.GetFiles(directory, "*.csv")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToList();

                if (files.Count == 0)
                {
                    sb.Append("<p class=\"empty\">No CSV log files found yet.</p>");
                }
                else
                {
                    sb.Append("<table><thead><tr><th>File</th><th>Size</th>");
                    sb.Append("<th>Last modified (UTC)</th><th>Download</th></tr></thead><tbody>");
                    foreach (var file in files)
                    {
                        var encodedName = Uri.EscapeDataString(file.Name);
                        sb.Append("<tr>");
                        sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(file.Name)}</td>");
                        sb.Append($"<td>{FormatSize(file.Length)}</td>");
                        sb.Append($"<td>{file.LastWriteTimeUtc:yyyy-MM-dd HH:mm:ss}</td>");
                        sb.Append($"<td><a class=\"btn\" href=\"/query-logs/download/{encodedName}\">Download</a></td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</tbody></table>");
                }
            }

            sb.Append("</body></html>");

            return new ContentResult
            {
                Content = sb.ToString(),
                ContentType = "text/html",
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Downloads a specific CSV log file from the log directory.
        /// </summary>
        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            // Guard against path traversal — only allow a bare file name.
            if (string.IsNullOrWhiteSpace(fileName)
                || fileName.Contains("..", StringComparison.Ordinal)
                || Path.GetFileName(fileName) != fileName
                || !fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file name.");
            }

            var directory = GetLogDirectory();
            if (directory is null)
            {
                return NotFound("Log directory not configured.");
            }

            var fullPath = Path.Combine(directory, fileName);
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("Log file not found.");
            }

            // Open with shared read so the file can be downloaded while the app is writing to it.
            var stream = new FileStream(
                fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            return File(stream, "text/csv", fileName);
        }

        private string? GetLogDirectory()
        {
            var csvPath = _configuration["QueryProfiling:CsvFilePath"];
            if (string.IsNullOrWhiteSpace(csvPath))
            {
                csvPath = Path.Combine("Logs", "query-profiling.csv");
            }

            if (!Path.IsPathRooted(csvPath))
            {
                csvPath = Path.Combine(_environment.ContentRootPath, csvPath);
            }

            return Path.GetDirectoryName(csvPath);
        }

        private static string FormatSize(long bytes)
        {
            string[] units = { "B", "KB", "MB", "GB" };
            double size = bytes;
            var unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.##} {units[unit]}";
        }
    }
}
