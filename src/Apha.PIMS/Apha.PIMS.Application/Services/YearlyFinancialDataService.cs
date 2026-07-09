/*
 * TRANSFORMENGINE MIGRATION — YearlyFinancialDataService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-07-09
 *
 * CHANGED:
 *   - New file: no prior C# service existed
 *   - Service orchestrates IYearlyFinancialDataRepository calls for CRUD + PACT actuals query
 *   - Validation logic extracted from VBA form events (frmProjectRadTrackData_Update):
 *       * CreateAsync: validates Year > 0 and Project non-empty; blocks duplicate composite key
 *       * UpdateAsync: validates Year > 0 and Project non-empty; throws KeyNotFoundException if
 *         record not found before update
 *   - "Fix Costing" logic (btnFixCosting VBA): when Locked flag transitions to 1, DateCosted
 *     and CostedBy are preserved as provided by caller (populated by controller from current
 *     user context) — no silent overwrite in service layer; service preserves what caller supplies
 *   - AutoMapper used for all Entity <-> DTO conversions (no manual property mapping)
 *   - No direct DbContext usage — all data access via IYearlyFinancialDataRepository
 *
 * PRESERVED:
 *   - All composite key (year, project) semantics from Access form RecordSource filter
 *   - Duplicate-key guard on create (VBA BeforeInsert event behavior)
 *   - Existence check on update (VBA BeforeUpdate event behavior)
 *   - GetPactCostsAsync delegates directly to repository (no service-layer aggregation needed;
 *     repository returns vpactprojectyearcosts rows already grouped by project+year)
 *   - Async method signatures consistent with IRadTrackInvoiceService pattern
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: VBA btnUpdateCosting_Click copied ActualExpenditure, ManHours,
 *     ManDays, ManYears, PayCosts, NonPayOhCosts, TestCosts, AnimalCosts from PACT actuals
 *     back into the main record — this "apply PACT actuals" operation should be implemented
 *     as a dedicated ApplyPactCostsAsync method once repository supports it
 *   - TRANSFORMENGINE TODO: Verify locked-record edit guard: if Locked == 1 the VBA form
 *     prevented editing; service currently does not enforce this — add guard once confirmed
 */

using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    /// <summary>
    /// Application service for yearly financial data operations.
    /// Orchestrates <see cref="IYearlyFinancialDataRepository"/> calls and
    /// applies business validation derived from frmProjectRadTrackData_Update VBA logic.
    /// </summary>
    public class YearlyFinancialDataService : IYearlyFinancialDataService
    {
        private readonly IYearlyFinancialDataRepository _repository;
        private readonly IMapper _mapper;

        public YearlyFinancialDataService(IYearlyFinancialDataRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GetAllAsync — paginated list filtered by project (QueryParameters.Filter = project code)
        //   Matches form RecordSource: SELECT * FROM MY_tlkpProjectRadTrackData WHERE project = [current project]
        public async Task<PaginatedResult<YearlyFinancialDataDto>> GetAllAsync(QueryParameters<string> parameters)
        {
            if (parameters is null)
                throw new ArgumentException("Query parameters must not be null.", nameof(parameters));

            PaginationParameters<string> paginationParams =
                _mapper.Map<PaginationParameters<string>>(parameters);

            PagedData<YearlyFinancialData> pagedData =
                await _repository.GetAllAsync(parameters.Filter ?? string.Empty, paginationParams);

            return new PaginatedResult<YearlyFinancialDataDto>
            {
                Data = _mapper.Map<List<YearlyFinancialDataDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }

        // TRANSFORMENGINE: GetByKeyAsync — single record by composite key (year + project)
        public async Task<YearlyFinancialDataDto?> GetByKeyAsync(short year, string project)
        {
            YearlyFinancialData? entity = await _repository.GetByKeyAsync(year, project);
            return entity is null ? null : _mapper.Map<YearlyFinancialDataDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — validates required fields then blocks duplicate composite key
        //   Derived from VBA frmProjectRadTrackData_Update BeforeInsert and Form_BeforeInsert events:
        //     * Year and Project are required (form bound fields, not nullable in RecordSource)
        //     * Duplicate year+project combination is rejected (primary key constraint guard)
        public async Task<YearlyFinancialDataDto> CreateAsync(YearlyFinancialDataDto dto)
        {
            if (dto is null)
                throw new ArgumentException("YearlyFinancialData DTO must not be null.", nameof(dto));

            // TRANSFORMENGINE: Required field validation — Year and Project form the composite PK
            var errors = new List<BusinessValidationError>();

            if (dto.Year <= 0)
                errors.Add(new BusinessValidationError(
                    "Year is required and must be a valid financial year.",
                    "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError(
                    "Project is required.",
                    "PROJECT_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // TRANSFORMENGINE: Duplicate composite key guard — preserves PK constraint semantics from Access form
            bool duplicate = await _repository.ExistsAsync(dto.Year, dto.Project!);
            if (duplicate)
            {
                errors.Add(new BusinessValidationError(
                    $"A yearly financial data record for year {dto.Year} and project '{dto.Project}' already exists.",
                    "DUPLICATE_YEARLY_FINANCIAL_DATA"));
                throw new BusinessValidationErrorException(errors);
            }

            YearlyFinancialData newEntity = _mapper.Map<YearlyFinancialData>(dto);
            YearlyFinancialData created = await _repository.CreateAsync(newEntity);
            return _mapper.Map<YearlyFinancialDataDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — validates required fields, confirms record exists, then updates
        //   Derived from VBA frmProjectRadTrackData_Update BeforeUpdate event:
        //     * Year and Project are required (composite key identifies the row to update)
        //     * Record must exist — KeyNotFoundException propagates to API middleware as 404
        public async Task<YearlyFinancialDataDto> UpdateAsync(YearlyFinancialDataDto dto)
        {
            if (dto is null)
                throw new ArgumentException("YearlyFinancialData DTO must not be null.", nameof(dto));

            // TRANSFORMENGINE: Required field validation — composite key fields needed to identify row
            var errors = new List<BusinessValidationError>();

            if (dto.Year <= 0)
                errors.Add(new BusinessValidationError(
                    "Year is required and must be a valid financial year.",
                    "YEAR_REQUIRED"));

            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError(
                    "Project is required.",
                    "PROJECT_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // TRANSFORMENGINE: Existence check — guard against updating a non-existent record
            YearlyFinancialData existing =
                await _repository.GetByKeyAsync(dto.Year, dto.Project!)
                ?? throw new KeyNotFoundException(
                    $"Yearly financial data record for year {dto.Year} and project '{dto.Project}' was not found.");

            // TRANSFORMENGINE: Map updated values onto the retrieved entity (preserves EF change tracking)
            _mapper.Map(dto, existing);

            YearlyFinancialData updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<YearlyFinancialDataDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — delegates directly to repository composite key delete
        public async Task<bool> DeleteAsync(short year, string project)
            => await _repository.DeleteAsync(year, project);

        // TRANSFORMENGINE: GetPactCostsAsync — reads vpactprojectyearcosts view rows for btnUpdateCosting
        //   Used by the "Update Costing" button to display PACT actuals alongside budgeted values
        public async Task<IReadOnlyList<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year)
        {
            if (string.IsNullOrWhiteSpace(project))
                throw new ArgumentException("Project is required.", nameof(project));

            if (year <= 0)
                throw new ArgumentException("Year must be a valid financial year.", nameof(year));

            IReadOnlyList<PactProjectYearCosts> rows =
                await _repository.GetPactCostsAsync(project, year);

            return _mapper.Map<IReadOnlyList<PactProjectYearCostsDto>>(rows);
        }
    }
}
