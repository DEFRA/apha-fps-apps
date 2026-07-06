/*
 * TRANSFORMENGINE MIGRATION — IRadTrackProgRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New Core repository interface for RadtrackProg CRUD operations (mabarchive.tblradtrackprog)
 *   - All methods are async (Task<T>) — no synchronous signatures
 *   - Natural string PK (program varchar(10)) reflected in method signatures
 *   - GetAllAsync returns full list for Programme Tab administration
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
    // TRANSFORMENGINE: interface covers CRUD for RadtrackProg (mabarchive.tblradtrackprog); natural string PK (program varchar(10)); Programme Tab
    public interface IRadTrackProgRepository
    {
        Task<List<RadtrackProg>> GetAllAsync();

        Task<RadtrackProg?> GetByIdAsync(string program);

        Task<RadtrackProg> AddAsync(RadtrackProg entity);

        Task<RadtrackProg> UpdateAsync(RadtrackProg entity);

        Task DeleteAsync(string program);

        Task<bool> ExistsAsync(string program);
    }
}
