using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPSApps.Web.Controllers
{
    /// <summary>
    /// Browser-accessible diagnostics page that lists and downloads the centralised
    /// outbound API performance log(s) produced by <c>ApiPerformanceLoggingHandler</c>.
    /// </summary>
    [Authorize]
    [Route("api-performance-logs")]
    public class ApiPerformanceLogController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ApiPerformanceLogController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var directory = GetLogDirectory();

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>API Performance Logs</title>");
            sb.Append("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;}table{border-collapse:collapse;}th,td{border:1px solid #ccc;padding:.4rem .8rem;text-align:left;}a{color:#0067b8;}</style>");
            sb.Append("</head><body>");
            sb.Append("<h1>Outbound API Performance Logs</h1>");

            if (!Directory.Exists(directory))
            {
                sb.Append("<p>No log directory found yet. Logs are created once API calls are made.</p>");
            }
            else
            {
                var files = Directory.GetFiles(directory, "*.csv")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTimeUtc)
                    .ToList();

                if (files.Count == 0)
                {
                    sb.Append("<p>No log files found yet.</p>");
                }
                else
                {
                    sb.Append("<table><tr><th>File</th><th>Size (KB)</th><th>Last Modified (UTC)</th><th>Download</th></tr>");
                    foreach (var file in files)
                    {
                        sb.Append("<tr>");
                        sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(file.Name)}</td>");
                        sb.Append($"<td>{(file.Length / 1024.0):N1}</td>");
                        sb.Append($"<td>{file.LastWriteTimeUtc:yyyy-MM-dd HH:mm:ss}</td>");
                        sb.Append($"<td><a href=\"/api-performance-logs/download/{System.Net.WebUtility.UrlEncode(file.Name)}\">Download</a></td>");
                        sb.Append("</tr>");
                    }
                    sb.Append("</table>");
                }
            }

            sb.Append("</body></html>");
            return Content(sb.ToString(), "text/html");
        }

        [HttpGet("download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)
                || fileName.Contains("..", StringComparison.Ordinal)
                || Path.GetFileName(fileName) != fileName)
            {
                return BadRequest("Invalid file name.");
            }

            var directory = GetLogDirectory();
            var fullPath = Path.Combine(directory, fileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var stream = new FileStream(
                fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return File(stream, "text/csv", fileName);
        }

        private string GetLogDirectory()
        {
            var csvPath = _configuration["ApiPerformanceLogging:CsvFilePath"];
            if (string.IsNullOrWhiteSpace(csvPath))
            {
                csvPath = Path.Combine("Logs", "api-performance.csv");
            }

            if (!Path.IsPathRooted(csvPath))
            {
                csvPath = Path.Combine(_environment.ContentRootPath, csvPath);
            }

            return Path.GetDirectoryName(csvPath) ?? _environment.ContentRootPath;
        }
    }
}
