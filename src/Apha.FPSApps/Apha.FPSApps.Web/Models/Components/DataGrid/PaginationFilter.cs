using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Models.Components.DataGrid
{
    public class PaginationFilter<TFilter>
    {
        [MaxLength(200)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
        public int Page { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "PageSize must be between 1 and 1000.")]
        public int PageSize { get; set; } = 10;

        [MaxLength(100)]
        public string? SortBy { get; set; }

        public bool Descending { get; set; } = false;

        public TFilter? Filter { get; set; }
    }
}

