/*
 * TRANSFORMENGINE MIGRATION — FpsDepartmentIncomeApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New AutoMapper Profile created for DepartmentIncome Res → frontend DTO mappings
 *   - Maps all 5 backend query response contracts (Time, Tests, Animals, Additional, Totals) to their
 *     corresponding frontend DTOs in Apha.FPSApps.Application.Dtos.DepartmentIncome
 *   - Maps PeriodLookupRes → PeriodLookupDto for the period dropdown lookup endpoint
 *   - All property names align between Res and Dto (convention-based; no ForMember required)
 *   - ReverseMap() applied to all mappings — service layer may map in either direction
 *
 * PRESERVED:
 *   - Property name conventions established in backend Res contracts and frontend Dtos (Phase 1 + Phase 7)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: If backend Res property names ever diverge from Dto property names
 *     (e.g. after a backend schema change), replace .ReverseMap() with explicit .ForMember() calls
 */

using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos.DepartmentIncome;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsDepartmentIncomeApiDtoMapper : Profile
    {
        public FpsDepartmentIncomeApiDtoMapper()
        {
            // TRANSFORMENGINE: DepartmentIncome Time — GET /api/v1/department-income/time
            // All 18 properties align by convention between DepartmentIncomeTimeRes and DepartmentIncomeTimeDto
            CreateMap<DepartmentIncomeTimeRes, DepartmentIncomeTimeDto>().ReverseMap();

            // TRANSFORMENGINE: DepartmentIncome Tests — GET /api/v1/department-income/tests
            // All 14 properties align by convention between DepartmentIncomeTestRes and DepartmentIncomeTestDto
            CreateMap<DepartmentIncomeTestRes, DepartmentIncomeTestDto>().ReverseMap();

            // TRANSFORMENGINE: DepartmentIncome Animals — GET /api/v1/department-income/animals
            // All 13 properties align by convention between DepartmentIncomeAnimalRes and DepartmentIncomeAnimalDto
            CreateMap<DepartmentIncomeAnimalRes, DepartmentIncomeAnimalDto>().ReverseMap();

            // TRANSFORMENGINE: DepartmentIncome Additional/Exceptional — GET /api/v1/department-income/additional
            // All 8 properties align by convention between DepartmentIncomeAdditionalRes and DepartmentIncomeAdditionalDto
            CreateMap<DepartmentIncomeAdditionalRes, DepartmentIncomeAdditionalDto>().ReverseMap();

            // TRANSFORMENGINE: DepartmentIncome Totals (PIVOT) — GET /api/v1/department-income/totals
            // All 7 properties align by convention between DepartmentIncomeTotalsRes and DepartmentIncomeTotalsDto
            CreateMap<DepartmentIncomeTotalsRes, DepartmentIncomeTotalsDto>().ReverseMap();

            // TRANSFORMENGINE: Period lookup — GET /api/v1/department-income/periods
            // Dedicated lookup DTO — must NOT be handled by CRUD entity mappers above
            // AccntsPeriod, MonthName, MonthNumber align by convention
            CreateMap<PeriodLookupRes, PeriodLookupDto>().ReverseMap();
        }
    }
}
