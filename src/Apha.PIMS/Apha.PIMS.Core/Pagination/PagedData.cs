/*
 * TRANSFORMENGINE MIGRATION — PagedData.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - MS Access DataGrid paging (client-side, in-memory) → server-side pagination primitive
 *   - Generic PagedData<T> wraps any entity/DTO result set with PaginationData metadata
 *   - PaginationData carries PageNumber, PageSize, TotalPages, TotalRecords for frontend consumption
 *
 * PRESERVED:
 *   - Read-only IReadOnlyCollection<T> Data property prevents accidental mutation after construction
 *   - Constructor enforces required initialization of both data and pagination metadata
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
namespace Apha.PIMS.Core.Pagination
{
    // TRANSFORMENGINE: Shared pagination envelope — used by all paged repository results across PIMS
    public class PagedData<T>
    {
        public IReadOnlyCollection<T> Data { get; }       
        public PaginationData PaginationData { get; }

        public PagedData(IReadOnlyCollection<T> items, PaginationData paginationData)
        {
            Data = items;           
            PaginationData = paginationData;
        }
    }

    public class PaginationData
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
