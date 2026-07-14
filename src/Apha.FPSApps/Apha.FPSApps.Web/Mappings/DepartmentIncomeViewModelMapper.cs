/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New AutoMapper Profile created for DepartmentIncome ViewModel/Item ↔ DTO mappings (Web layer)
 *   - Maps all 5 grid row Item types to their corresponding frontend DTOs using convention-based mapping
 *   - PeriodLookupDto is a lookup-only type — no ViewModel mapping required (used directly in dropdowns)
 *   - DepartmentIncomeTimeItem, TestItem, AnimalItem, AdditionalItem, TotalsItem stub classes
 *     created in Phase 10 alongside this mapper; [GridColumn] / [Display] attributes added in Phase 11
 *
 * PRESERVED:
 *   - All property names align between Item classes and their corresponding DTOs (18/14/13/8/7 properties)
 *   - ReverseMap() applied to all Item ↔ Dto mappings — MVC controller may map in either direction
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If Phase 11 adds properties to Item classes that differ from DTO property names,
 *     replace .ReverseMap() with explicit .ForMember() calls
 *   - TRANSFORMENGINE TODO: DepartmentIncomeViewModel (Phase 11) may require a Dto mapping if form filters
 *     are bound through a ViewModel — add CreateMap<DepartmentIncomeViewModel, ...> in Phase 11
 */

using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using Apha.FPSApps.Web.Areas.FPS.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class DepartmentIncomeViewModelMapper : Profile
    {
        public DepartmentIncomeViewModelMapper()
        {
            // TRANSFORMENGINE: Time query grid row — maps DepartmentIncomeTimeItem ↔ DepartmentIncomeTimeDto
            // All 18 properties align by convention
            CreateMap<DepartmentIncomeTimeItem, DepartmentIncomeTimeDto>().ReverseMap();

            // TRANSFORMENGINE: Tests query grid row — maps DepartmentIncomeTestItem ↔ DepartmentIncomeTestDto
            // All 14 properties align by convention
            CreateMap<DepartmentIncomeTestItem, DepartmentIncomeTestDto>().ReverseMap();

            // TRANSFORMENGINE: Animals query grid row — maps DepartmentIncomeAnimalItem ↔ DepartmentIncomeAnimalDto
            // All 13 properties align by convention
            CreateMap<DepartmentIncomeAnimalItem, DepartmentIncomeAnimalDto>().ReverseMap();

            // TRANSFORMENGINE: Additional/Exceptional query grid row — maps DepartmentIncomeAdditionalItem ↔ DepartmentIncomeAdditionalDto
            // All 8 properties align by convention
            CreateMap<DepartmentIncomeAdditionalItem, DepartmentIncomeAdditionalDto>().ReverseMap();

            // TRANSFORMENGINE: Totals (PIVOT) query grid row — maps DepartmentIncomeTotalsItem ↔ DepartmentIncomeTotalsDto
            // All 7 properties align by convention (nullable decimal? pivot columns preserved)
            CreateMap<DepartmentIncomeTotalsItem, DepartmentIncomeTotalsDto>().ReverseMap();
        }
    }
}
