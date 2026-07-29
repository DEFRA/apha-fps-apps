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

        public DepartmentIncomeService(IDepartmentIncomeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper     = mapper;
        }

        public async Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTimeIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTimeDto>>(results);
        }

        public async Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTestIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTestDto>>(results);
        }

        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAnimalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAnimalDto>>(results);
        }

        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAdditionalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAdditionalDto>>(results);
        }

        public async Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTotalsAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTotalsDto>>(results);
        }

        public async Task<List<PeriodLookupDto>> GetPeriodsAsync()
        {
            var results = await _repository.GetPeriodsAsync();
            return _mapper.Map<List<PeriodLookupDto>>(results);
        }

        // ── VBA default-value helpers ─────────────────────────────────────────────────────────────
        //   Access VBA: Function fnDeptIncomeMonthFrom() → If IsNull(MonthFrom) Then 1 Else MonthFrom
        private static int ResolveMonthFrom(int? monthFrom) => monthFrom ?? 1;

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
