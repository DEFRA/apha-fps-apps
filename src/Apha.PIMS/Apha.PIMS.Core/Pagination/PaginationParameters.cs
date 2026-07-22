/*
 * TRANSFORMENGINE MIGRATION — PaginationParameters.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access DataGrid column-sort and filter UI state → generic server-side pagination request model
 *   - Generic TFilter type parameter allows each repository to define its own strongly-typed filter model
 *   - Search, SortBy, Descending replace Access form OrderBy/FilterOn/Filter properties
 *   - Page + PageSize replace Access form RecordsetClone bookmark-based paging
 *
 * PRESERVED:
 *   - Defaults match original Access form paging behaviour: page 1, 10 rows, no sort
 *   - Optional Search and Filter allow both free-text search and structured filter objects
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.PIMS.Core.Pagination
{
    // TRANSFORMENGINE: Shared query/pagination parameter carrier — used by all paged repository calls
    public class PaginationParameters<TFilter>
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public TFilter? Filter { get; set; }

        public PaginationParameters(string? search = null, string? sortBy = "", bool descending = false, int page = 1, int pageSize = 10)
        {
            Search = search;
            SortBy = sortBy;
            Descending = descending;
            Page = page;
            PageSize = pageSize;
        }
    }
}
