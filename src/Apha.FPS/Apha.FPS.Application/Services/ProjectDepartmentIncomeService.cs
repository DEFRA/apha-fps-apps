using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProjectDepartmentIncomeService : IProjectDepartmentIncomeService
    {
        private readonly IProjectDepartmentIncomeRepository _repository;
        private readonly IMapper _mapper;

        public ProjectDepartmentIncomeService(IProjectDepartmentIncomeRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper     = mapper     ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTimeIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTimeDto>>(results);
        }

        public async Task<PaginatedResult<DepartmentIncomeTimeDto>> GetPagedTimeIncomeAsync(
            QueryParameters<string> query, string? project, int? monthFrom, int? monthTo)
        {
            var from   = ResolveMonthFrom(monthFrom);
            var to     = ResolveMonthTo(monthTo, from);
            var filter = _mapper.Map<Core.Pagination.PaginationParameters<string>>(query);
            var result = await _repository.GetPagedTimeIncomeAsync(filter, project, from, to);
            return _mapper.Map<PaginatedResult<DepartmentIncomeTimeDto>>(result);
        }

        public async Task<List<DepartmentIncomeTestDto>> GetTestIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTestIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTestDto>>(results);
        }

        public async Task<PaginatedResult<DepartmentIncomeTestDto>> GetPagedTestIncomeAsync(
            QueryParameters<string> query, string? project, int? monthFrom, int? monthTo)
        {
            var from   = ResolveMonthFrom(monthFrom);
            var to     = ResolveMonthTo(monthTo, from);
            var filter = _mapper.Map<Core.Pagination.PaginationParameters<string>>(query);
            var result = await _repository.GetPagedTestIncomeAsync(filter, project, from, to);
            return _mapper.Map<PaginatedResult<DepartmentIncomeTestDto>>(result);
        }

        // AcctCode IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAnimalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAnimalDto>>(results);
        }

        public async Task<PaginatedResult<DepartmentIncomeAnimalDto>> GetPagedAnimalIncomeAsync(
            QueryParameters<string> query, string? project, int? monthFrom, int? monthTo)
        {
            var from   = ResolveMonthFrom(monthFrom);
            var to     = ResolveMonthTo(monthTo, from);
            var filter = _mapper.Map<Core.Pagination.PaginationParameters<string>>(query);
            var result = await _repository.GetPagedAnimalIncomeAsync(filter, project, from, to);
            return _mapper.Map<PaginatedResult<DepartmentIncomeAnimalDto>>(result);
        }

        // AcctCode NOT IN ("LargeAnimals","SmallAnimals","Mice") filter enforced in repository implementation
        public async Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAdditionalIncomeAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAdditionalDto>>(results);
        }

        public async Task<PaginatedResult<DepartmentIncomeAdditionalDto>> GetPagedAdditionalIncomeAsync(
            QueryParameters<string> query, string? project, int? monthFrom, int? monthTo)
        {
            var from   = ResolveMonthFrom(monthFrom);
            var to     = ResolveMonthTo(monthTo, from);
            var filter = _mapper.Map<Core.Pagination.PaginationParameters<string>>(query);
            var result = await _repository.GetPagedAdditionalIncomeAsync(filter, project, from, to);
            return _mapper.Map<PaginatedResult<DepartmentIncomeAdditionalDto>>(result);
        }

        public async Task<List<DepartmentIncomeTotalsDto>> GetTotalsAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTotalsAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTotalsDto>>(results);
        }

        // ── Current (old style) variants ─────────────────────────────────────────────────────────

        public async Task<List<DepartmentIncomeTimeDto>> GetTimeIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTimeIncomeCurrentAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTimeDto>>(results);
        }

        public async Task<List<DepartmentIncomeTestDto>> GetTestIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTestIncomeCurrentAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTestDto>>(results);
        }

        public async Task<List<DepartmentIncomeAnimalDto>> GetAnimalIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAnimalIncomeCurrentAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAnimalDto>>(results);
        }

        public async Task<List<DepartmentIncomeAdditionalDto>> GetAdditionalIncomeCurrentAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetAdditionalIncomeCurrentAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeAdditionalDto>>(results);
        }

        public async Task<List<DepartmentIncomeTotalsDto>> GetTotalsCurrentAsync(string? project, int? monthFrom, int? monthTo)
        {
            var from    = ResolveMonthFrom(monthFrom);
            var to      = ResolveMonthTo(monthTo, from);
            var results = await _repository.GetTotalsCurrentAsync(project, from, to);
            return _mapper.Map<List<DepartmentIncomeTotalsDto>>(results);
        }

        public async Task<List<PeriodLookupDto>> GetPeriodsAsync(double? accntsPeriod = null)
        {
            var results = await _repository.GetPeriodsAsync(accntsPeriod);
            return _mapper.Map<List<PeriodLookupDto>>(results);
        }

        public async Task<List<PeriodSnapshotDto>> GetSnapshotPeriodsAsync()
        {
            var results = await _repository.GetSnapshotPeriodsAsync();
            return _mapper.Map<List<PeriodSnapshotDto>>(results);
        }

        public Task<int> UpdatePeriodLockedAsync(string periodName, bool periodLocked)
            => _repository.UpdatePeriodLockedAsync(periodName, periodLocked);

        // ── VBA default-value helpers ─────────────────────────────────────────────────────────────
        //   Access VBA: Function fnDeptIncomeMonthFrom() → If IsNull(MonthFrom) Then 1 Else MonthFrom
        private static int ResolveMonthFrom(int? monthFrom) => monthFrom ?? 1;

        //   Access VBA: Function fnDeptIncomeMonthTo() → If IsNull(MonthTo) Then
        //                   If resolvedMonthFrom = 1 Then 12 Else resolvedMonthFrom
        //               Else MonthTo
        private static int ResolveMonthTo(int? monthTo, int resolvedMonthFrom)
        {
            if (monthTo.HasValue)
                return monthTo.Value;

            return resolvedMonthFrom == 1 ? 12 : resolvedMonthFrom;
        }
    }
}
