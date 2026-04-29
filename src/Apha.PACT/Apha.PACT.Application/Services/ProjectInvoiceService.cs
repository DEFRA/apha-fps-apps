using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class ProjectInvoiceService : IProjectInvoiceService
    {
        private readonly IProjectInvoiceRepository _repository;
        private readonly IMapper _mapper;

        public ProjectInvoiceService(IProjectInvoiceRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectInvoiceDto>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectInvoice> pagedData = await _repository.GetPagedProjectInvoicesAsync(parameters, parentProject);
            return _mapper.Map<PaginatedResult<ProjectInvoiceDto>>(pagedData);
        }

        public async Task<decimal> GetTotalAmountAsync(string? parentProject)
            => await _repository.GetTotalAmountAsync(parentProject);

        public async Task<ProjectInvoiceDto?> GetByIdAsync(int invoiceCounter)
        {
            ProjectInvoice? entity = await _repository.GetByIdAsync(invoiceCounter);
            return entity == null ? null : _mapper.Map<ProjectInvoiceDto>(entity);
        }

        public async Task<ProjectInvoiceDto> CreateAsync(ProjectInvoiceDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.ProjectParent))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectInvoice entity = _mapper.Map<ProjectInvoice>(dto);
            ProjectInvoice created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProjectInvoiceDto>(created);
        }

        public async Task<ProjectInvoiceDto> UpdateAsync(ProjectInvoiceDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.ProjectParent))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectInvoice entity = _mapper.Map<ProjectInvoice>(dto);
            ProjectInvoice updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectInvoiceDto>(updated);
        }

        public async Task<bool> DeleteAsync(int invoiceCounter)
        {
            return await _repository.DeleteAsync(invoiceCounter);
        }

        public async Task<MonthlyInvoicesPivotDto> GetMonthlyInvoicesSummaryAsync()
        {
            List<Core.Entities.MonthlyInvoicesSummary> data = await _repository.GetMonthlyInvoicesSummaryAsync();

            List<int> months = data
                .Select(x => x.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            List<MonthlyInvoicesSummaryDto> rows = data
                .GroupBy(x => new { x.Program, x.Parentproject })
                .Select(g => new MonthlyInvoicesSummaryDto
                {
                    Program = g.Key.Program,
                    ParentProject = g.Key.Parentproject,
                    MonthlyAmounts = g.ToDictionary(x => x.Month, x => x.Monthlyamount ?? 0m)
                })
                .OrderBy(r => r.Program)
                .ThenBy(r => r.ParentProject)
                .ToList();

            return new MonthlyInvoicesPivotDto
            {
                Months = months,
                Rows = rows
            };
        }
    }
}
