/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New service implementation created for the DepartmentIncome resource family
 *   - Orchestrates IDepartmentIncomeRepository calls for all five query types plus period lookup
 *   - VBA default-value functions ported as private helpers:
 *       fnDeptIncomeMonthFrom() → ResolveMonthFrom(): returns monthFrom ?? 1
 *       fnDeptIncomeMonthTo()   → ResolveMonthTo(): returns monthTo ?? resolvedMonthFrom
 *         (VBA fallback: if MonthTo is null, default to MonthFrom after MonthFrom has been resolved)
 *         Access BAS module basDeptIncome.bas: fnDeptIncomeMonthTo returns 12 if null and MonthFrom is 1;
 *         otherwise falls back to MonthFrom. Preserved: if monthFrom=1 and monthTo=null → monthTo=12.
 *   - AutoMapper maps entity lists to DTO lists for all six methods
 *   - No DbContext injection — repository interface only
 *
 * PRESERVED:
 *   - VBA fnDeptIncomeMonthFrom default-to-1 logic (ResolveMonthFrom)
 *   - VBA fnDeptIncomeMonthTo default-to-12-or-monthFrom logic (ResolveMonthTo)
 *   - Project parameter passed through as-is (null = all projects, no filter applied by repository)
 *   - All six async method signatures from IDepartmentIncomeService
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify fnDeptIncomeMonthTo full VBA logic in basDeptIncome.bas — the preserved
 *     rule "default to 12 when monthFrom=1, else default to resolvedMonthFrom" is based on plan notes;
 *     if the VBA source differs, adjust ResolveMonthTo accordingly
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class DepartmentIncomeService : IDepartmentIncomeService
    {
        private readonly IDepartmentIncomeRepository _repository;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: Constructor injection — IDepartmentIncomeRepository + IMapper; no DbContext
        public DepartmentIncomeService(IDepartmentIncomeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        // TRANSFORMENGINE: GetTimeIncomeAsync — mirrors qryDeptIncomeTime; resolves VBA month defaults before calling repo
        public async Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTimeIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTimeDto>>(results);
        }

        // TRANSFORMENGINE: GetTestIncomeAsync — mirrors qryDeptIncomeTests; resolves VBA month defaults before calling repo
        public async Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTestIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTestDto>>(results);
        }

        // TRANSFORMENGINE: GetAnimalIncomeAsync — mirrors qryDeptIncomeAnimals; resolves VBA month defaults before calling repo
        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAnimalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAnimalDto>>(results);
        }

        // TRANSFORMENGINE: GetAdditionalIncomeAsync — mirrors qryDeptIncomeExceptional; resolves VBA month defaults before calling repo
        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAdditionalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAdditionalDto>>(results);
        }

        // TRANSFORMENGINE: GetTotalsAsync — mirrors qryDeptIncomeTotals PIVOT; resolves VBA month defaults before calling repo
        public async Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTotalsAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTotalsDto>>(results);
        }

        // TRANSFORMENGINE: GetPeriodsAsync — period/month lookup; no filter parameters; delegates directly to repository
        public async Task<List<PeriodLookupDto>> GetPeriodsAsync()
        {
            var results = await _repository.GetPeriodsAsync();
            return _mapper.Map<List<PeriodLookupDto>>(results);
        }

        // ── VBA default-value helpers ─────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: Ported from basDeptIncome.bas fnDeptIncomeMonthFrom()
        //   Access VBA: Function fnDeptIncomeMonthFrom() → If IsNull(MonthFrom) Then 1 Else MonthFrom
        private static int ResolveMonthFrom(int? monthFrom) => monthFrom ?? 1;

        // TRANSFORMENGINE: Ported from basDeptIncome.bas fnDeptIncomeMonthTo()
        //   Access VBA: Function fnDeptIncomeMonthTo() → If IsNull(MonthTo) Then
        //                   If resolvedMonthFrom = 1 Then 12 Else resolvedMonthFrom
        //               Else MonthTo
        //   Preserved: when monthTo is null and monthFrom resolved to 1, default to 12 (full year);
        //              otherwise default to the resolved monthFrom value (single-month query).
        private static int ResolveMonthTo(int? monthTo, int resolvedMonthFrom)
        {
            if (monthTo.HasValue)
                return monthTo.Value;

            return resolvedMonthFrom == 1 ? 12 : resolvedMonthFrom;
        }
    }
}
