/*
 * TRANSFORMENGINE MIGRATION — IFrequencyRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for Frequency CRUD operations
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Single integer PK (frequencyid) — reflected in method signatures
 *   - GetAllAsync returns full list for frequency dropdown / lookup usage
 *   - ExistsAsync follows AnyAsync-style existence semantics per phase rules
 *
 * PRESERVED:
 *   - No infrastructure-specific code (DbContext, EF) in this Core interface
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    // TRANSFORMENGINE: interface covers CRUD for Frequency (mabarchive.tlkpfrequency); single integer PK (frequencyid); lookup/reference table
    public interface IFrequencyRepository
    {
        Task<List<Frequency>> GetAllAsync();

        Task<Frequency?> GetByIdAsync(int frequencyid);

        Task<Frequency> AddAsync(Frequency entity);

        Task<Frequency> UpdateAsync(Frequency entity);

        Task DeleteAsync(int frequencyid);

        Task<bool> ExistsAsync(int frequencyid);
    }
}
