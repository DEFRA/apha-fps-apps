// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RadTrackInvoiceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services (Steps 4-6)
 * Migrated : 2026-06-12
 *
 * CHANGED:
 *   - New file: service implementation orchestrating RadTrackInvoice CRUD, filtered pagination,
 *     and aggregate totals via IRadTrackInvoiceRepository.
 *   - Business validation guards extracted from the frmpimsinvoices VBA form event handlers and
 *     qryInvoices.msaccsql context:
 *       * Project is required on Create (form validation in Access was enforced by required
 *         combo-box selection before record save was allowed).
 *       * DueAmount and DueDate are required on Create (mandatory in the Add modal).
 *       * Duplicate InvoiceRef check within the same Project+Contract scope before Create/Update.
 *       * UpdateAsync requires a non-zero InvoiceCounter (PK guard).
 *   - AutoMapper used for Entity <-> DTO mapping; no manual property assignment.
 *   - BusinessValidationErrorException pattern used consistently with MilestoneService.
 *   - GetAllAsync maps QueryParameters<RadTrackInvoiceFilter> (Application layer) to
 *     PaginationParameters<RadTrackInvoiceFilter> (Core layer) via AutoMapper.
 *   - GetTotalsAsync maps RadTrackInvoiceTotals Core value object to RadTrackInvoiceTotalsDto.
 *
 * PRESERVED:
 *   - All repository method calls are async end-to-end.
 *   - No direct DbContext usage — repository interface only.
 *   - Filter dimensions (Project, Contract, Year, Program) passed through unchanged from
 *     QueryParameters to PaginationParameters so repository LINQ predicates apply correctly.
 *   - ExistsAsync exposed transparently to callers (API controller duplicate-check path).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify that AutoMapper can resolve the
 *     QueryParameters<RadTrackInvoiceFilter> -> PaginationParameters<RadTrackInvoiceFilter>
 *     generic map registered in EntityMapper.cs (CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap()).
 *   - TRANSFORMENGINE TODO: If InvoicePaid is later changed to bool on the DTO surface, update
 *     the InvoicePaid assignment guard in CreateAsync/UpdateAsync accordingly.
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
    // TRANSFORMENGINE: Implementation of IRadTrackInvoiceService.
    // Orchestrates IRadTrackInvoiceRepository calls and enforces business rules extracted from
    // frmpimsinvoices VBA form event logic and the qryInvoices named query constraints.
    public class RadTrackInvoiceService : IRadTrackInvoiceService
    {
        private readonly IRadTrackInvoiceRepository _repository;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: Constructor injection — IRadTrackInvoiceRepository + AutoMapper.
        // No DbContext injected; all data access routed through the repository interface.
        public RadTrackInvoiceService(IRadTrackInvoiceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // TRANSFORMENGINE: GetAllAsync — maps Application QueryParameters to Core PaginationParameters,
        // delegates to repository, maps result list back to DTO list.
        public async Task<PaginatedResult<RadTrackInvoiceDto>> GetAllAsync(QueryParameters<RadTrackInvoiceFilter> parameters)
        {
            if (parameters is null)
                throw new ArgumentException("Query parameters must not be null.", nameof(parameters));

            // TRANSFORMENGINE: Generic map QueryParameters<RadTrackInvoiceFilter> -> PaginationParameters<RadTrackInvoiceFilter>
            // registered in EntityMapper.cs via CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap().
            PaginationParameters<RadTrackInvoiceFilter> paginationParams =
                _mapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(parameters);

            PagedData<RadTrackInvoice> pagedData = await _repository.GetAllAsync(paginationParams);

            return new PaginatedResult<RadTrackInvoiceDto>
            {
                Data = _mapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data),
                PaginationData = _mapper.Map<PaginationDto>(pagedData.PaginationData)
            };
        }

        // TRANSFORMENGINE: GetByIdAsync — single record fetch by PK; returns null if not found.
        public async Task<RadTrackInvoiceDto?> GetByIdAsync(int invoiceCounter)
        {
            RadTrackInvoice? entity = await _repository.GetByIdAsync(invoiceCounter);
            return entity is null ? null : _mapper.Map<RadTrackInvoiceDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — validates required fields, checks duplicate InvoiceRef,
        // then persists a new RadTrackInvoice record. Business guards extracted from the
        // frmpimsinvoices VBA BeforeUpdate event and Access required-field combo-box enforcements.
        public async Task<RadTrackInvoiceDto> CreateAsync(RadTrackInvoiceDto dto)
        {
            if (dto is null)
                throw new ArgumentException("Invoice DTO must not be null.", nameof(dto));

            var errors = new List<BusinessValidationError>();

            // TRANSFORMENGINE: Project is required — enforced by a required combo-box in the Access form.
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            // TRANSFORMENGINE: DueAmount is required — "Amount Due" was a mandatory field in the Add modal.
            if (!dto.DueAmount.HasValue)
                errors.Add(new BusinessValidationError("Amount Due is required.", "DUE_AMOUNT_REQUIRED"));

            // TRANSFORMENGINE: DueDate is required — "Date Due" was a mandatory field in the Add modal.
            if (!dto.DueDate.HasValue)
                errors.Add(new BusinessValidationError("Date Due is required.", "DUE_DATE_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // TRANSFORMENGINE: Duplicate InvoiceRef guard — prevents the same invoice reference being
            // entered twice for the same project and contract combination.
            if (!string.IsNullOrWhiteSpace(dto.InvoiceRef))
            {
                bool duplicate = await _repository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef);
                if (duplicate)
                {
                    errors.Add(new BusinessValidationError(
                        "An invoice with this reference already exists for the selected project and contract.",
                        "INVOICE_REF_DUPLICATE"));
                    throw new BusinessValidationErrorException(errors);
                }
            }

            RadTrackInvoice newEntity = _mapper.Map<RadTrackInvoice>(dto);
            RadTrackInvoice created = await _repository.CreateAsync(newEntity);
            return _mapper.Map<RadTrackInvoiceDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — validates PK presence, required fields, then checks duplicate
        // InvoiceRef (excluding the current record), maps, and persists the update.
        public async Task<RadTrackInvoiceDto> UpdateAsync(RadTrackInvoiceDto dto)
        {
            if (dto is null)
                throw new ArgumentException("Invoice DTO must not be null.", nameof(dto));

            var errors = new List<BusinessValidationError>();

            // TRANSFORMENGINE: InvoiceCounter must be set for an update — PK guard.
            if (dto.InvoiceCounter <= 0)
                errors.Add(new BusinessValidationError("Invoice counter is required for update.", "INVOICE_COUNTER_REQUIRED"));

            // TRANSFORMENGINE: Project is required on update — consistent with Create validation.
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required.", "PROJECT_REQUIRED"));

            // TRANSFORMENGINE: DueAmount is required on update.
            if (!dto.DueAmount.HasValue)
                errors.Add(new BusinessValidationError("Amount Due is required.", "DUE_AMOUNT_REQUIRED"));

            // TRANSFORMENGINE: DueDate is required on update.
            if (!dto.DueDate.HasValue)
                errors.Add(new BusinessValidationError("Date Due is required.", "DUE_DATE_REQUIRED"));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            // TRANSFORMENGINE: Verify the record exists before attempting update.
            RadTrackInvoice existing = await _repository.GetByIdAsync(dto.InvoiceCounter)
                ?? throw new KeyNotFoundException($"Invoice with counter {dto.InvoiceCounter} was not found.");

            // TRANSFORMENGINE: Duplicate InvoiceRef guard for update — excludes the current record from
            // the existence check so the same record can be saved without false-positive duplicate error.
            if (!string.IsNullOrWhiteSpace(dto.InvoiceRef))
            {
                bool duplicate = await _repository.ExistsAsync(
                    dto.Project,
                    dto.Contract,
                    dto.InvoiceRef,
                    excludeInvoiceCounter: dto.InvoiceCounter);

                if (duplicate)
                {
                    errors.Add(new BusinessValidationError(
                        "An invoice with this reference already exists for the selected project and contract.",
                        "INVOICE_REF_DUPLICATE"));
                    throw new BusinessValidationErrorException(errors);
                }
            }

            // TRANSFORMENGINE: Map updated DTO values onto the existing tracked entity.
            _mapper.Map(dto, existing);
            RadTrackInvoice updated = await _repository.UpdateAsync(existing);
            return _mapper.Map<RadTrackInvoiceDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — delegates directly to repository; returns true if deleted.
        public async Task<bool> DeleteAsync(int invoiceCounter)
            => await _repository.DeleteAsync(invoiceCounter);

        // TRANSFORMENGINE: GetTotalsAsync — maps Core RadTrackInvoiceTotals value object to
        // RadTrackInvoiceTotalsDto for the totals footer row. Filter is passed through unchanged
        // so the totals match the current filtered grid result set.
        public async Task<RadTrackInvoiceTotalsDto> GetTotalsAsync(RadTrackInvoiceFilter? filter)
        {
            RadTrackInvoiceTotals totals = await _repository.GetTotalsAsync(filter);
            return _mapper.Map<RadTrackInvoiceTotalsDto>(totals);
        }

        // TRANSFORMENGINE: ExistsAsync — transparent pass-through to repository existence check.
        public async Task<bool> ExistsAsync(string? project, string? contract, string? invoiceRef, int? excludeInvoiceCounter = null)
            => await _repository.ExistsAsync(project, contract, invoiceRef, excludeInvoiceCounter);
    }
}
