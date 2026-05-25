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
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync()
        {
            var items = await _repository.GetAllWorkGroupsAsync();
            return _mapper.Map<IEnumerable<WorkGroupDto>>(items);
        }

        public async Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupTimeCodeAsync(parameters, workGroup, monthNumber);
            return _mapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupValidTimeCodeAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData);
        }

        public async Task<WorkGroupTimeByJobCodeDto> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);

            var entries = await _repository.GetWgSummarisedStaffTimeUsageAsync(workGroup);
            var entryList = entries.ToList();

            // Derive HrsPaid: sum across all distinct people in the work group (mirrors Access FormHeader HrsPaid)
            var hrsPaid = entryList
                .GroupBy(e => e.Name)
                .Select(g => g.First())
                .Sum(e => e.HrsPaid ?? 0);

            var standardHoursPerMonth = hrsPaid > 0 ? hrsPaid / 12.0 : 0;

            // Build ALL rows first — summary must reflect the full dataset, not just the current page
            var allRows = BuildRows(entryList);
            var summary = BuildSummary(allRows, standardHoursPerMonth);

            // Paginate rows after summary is computed
            var page     = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var totalRecords = allRows.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedRows    = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new WorkGroupTimeByJobCodeDto
            {
                Rows    = pagedRows,
                Summary = summary,
                HrsPaid = hrsPaid,
                Pagination = new PaginationDto
                {
                    PageNumber   = page,
                    PageSize     = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages   = totalPages
                }
            };
        }

        /// <summary>
        /// Groups view entries by ParentProject / JobCode and pivots hours into monthly columns,
        /// replicating the Detail section of frmCluedo1 (qryfrmCluedo1).
        /// </summary>
        private static List<WorkGroupTimeByJobCodeRowDto> BuildRows(
            IEnumerable<WgSummarisedStaffTimeUsageView> entries)
        {
            return entries
                .GroupBy(e => new { e.ParentProject, e.JobCode })
                .Select(g =>
                {
                    double HoursForMonth(string monthName) =>
                        g.Where(e => e.MonthName == monthName).Sum(e => e.TotalTime ?? 0);

                    return new WorkGroupTimeByJobCodeRowDto
                    {
                        ParentProject = g.Key.ParentProject,
                        JobCode = g.Key.JobCode,
                        JobTitle = g.First().JobTitle,
                        April = HoursForMonth("April"),
                        May = HoursForMonth("May"),
                        June = HoursForMonth("June"),
                        July = HoursForMonth("July"),
                        August = HoursForMonth("August"),
                        September = HoursForMonth("September"),
                        October = HoursForMonth("October"),
                        November = HoursForMonth("November"),
                        December = HoursForMonth("December"),
                        January = HoursForMonth("January"),
                        February = HoursForMonth("February"),
                        March = HoursForMonth("March"),
                        TotalTime = g.Sum(e => e.TotalTime ?? 0),
                        TotalCost = g.Sum(e => e.TotalCost ?? 0)
                    };
                })
                .OrderBy(r => r.ParentProject)
                .ThenBy(r => r.JobCode)
                .ToList();
        }

        /// <summary>
        /// Builds the three-row footer that appeared at the bottom of frmCluedo1.
        ///
        /// Access expressions translated:
        ///   Field46  = Sum([April])
        ///   Field54  = IIf(IsNull([field46]), 0, [hrspaid]/12)   → standard hours for April
        ///   Field75  = [field46] / [hrspaidmonth]                → % of std hrs for April
        ///   (same pattern repeated for every other month)
        ///   Field66  = sum of all 12 standard-hours fields        → total standard hours
        ///   Field80  = [field51] / [field66]                     → overall %
        /// </summary>
        private static WorkGroupTimeByJobCodeSummaryDto BuildSummary(
            IReadOnlyList<WorkGroupTimeByJobCodeRowDto> rows, double standardHoursPerMonth)
        {
            var totalHoursApril = rows.Sum(r => r.April);
            var totalHoursMay = rows.Sum(r => r.May);
            var totalHoursJune = rows.Sum(r => r.June);
            var totalHoursJuly = rows.Sum(r => r.July);
            var totalHoursAugust = rows.Sum(r => r.August);
            var totalHoursSeptember = rows.Sum(r => r.September);
            var totalHoursOctober = rows.Sum(r => r.October);
            var totalHoursNovember = rows.Sum(r => r.November);
            var totalHoursDecember = rows.Sum(r => r.December);
            var totalHoursJanuary = rows.Sum(r => r.January);
            var totalHoursFebruary = rows.Sum(r => r.February);
            var totalHoursMarch = rows.Sum(r => r.March);
            var grandTotalTime = rows.Sum(r => r.TotalTime);

            // Standard hours per month: 0 when the month has no recorded entries
            // mirrors IIf(IsNull([field46]), 0, [hrspaid]/12)
            double StandardHoursFor(double totalHoursInMonth)
            {
                return totalHoursInMonth == 0 ? 0 : standardHoursPerMonth;
            }

            // Percentage of standard hours allocated per month: mirrors =[field46]/[hrspaidmonth]
            double PercentAllocated(double totalHoursInMonth, double standardHours)
            {
                return standardHours == 0 ? 0 : Math.Round(totalHoursInMonth / standardHours * 100, 1);
            }

            return new WorkGroupTimeByJobCodeSummaryDto
            {
                TotalApril = totalHoursApril,
                TotalMay = totalHoursMay,
                TotalJune = totalHoursJune,
                TotalJuly = totalHoursJuly,
                TotalAugust = totalHoursAugust,
                TotalSeptember = totalHoursSeptember,
                TotalOctober = totalHoursOctober,
                TotalNovember = totalHoursNovember,
                TotalDecember = totalHoursDecember,
                TotalJanuary = totalHoursJanuary,
                TotalFebruary = totalHoursFebruary,
                TotalMarch = totalHoursMarch,
                GrandTotalTime = grandTotalTime,
                GrandTotalCost = rows.Sum(r => r.TotalCost),
                StandardHoursPerMonth = standardHoursPerMonth,

                // Field66: sum of per-month standard hours — only months with data contribute
                // mirrors =[hrspaidmonth]+[Field55]+[Field56]+...+[field104]
                TotalStandardHours =
                    StandardHoursFor(totalHoursApril)     + StandardHoursFor(totalHoursMay)      +
                    StandardHoursFor(totalHoursJune)      + StandardHoursFor(totalHoursJuly)     +
                    StandardHoursFor(totalHoursAugust)    + StandardHoursFor(totalHoursSeptember)+
                    StandardHoursFor(totalHoursOctober)   + StandardHoursFor(totalHoursNovember) +
                    StandardHoursFor(totalHoursDecember)  + StandardHoursFor(totalHoursJanuary)  +
                    StandardHoursFor(totalHoursFebruary)  + StandardHoursFor(totalHoursMarch),

                // Grand total % allocated: mirrors Access Field80 = [field51] / [field66]
                // field51 = grandTotalTime, field66 = TotalStandardHours (sum of active months only)
                GrandTotalPercentAllocated = (
                    StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                    StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                    StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                    StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                    StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                    StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) > 0
                        ? Math.Round(grandTotalTime /
                            (StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                             StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                             StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                             StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                             StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                             StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) * 100, 1)
                        : 0,

                PercentAllocatedApril = PercentAllocated(totalHoursApril, StandardHoursFor(totalHoursApril)),
                PercentAllocatedMay = PercentAllocated(totalHoursMay, StandardHoursFor(totalHoursMay)),
                PercentAllocatedJune = PercentAllocated(totalHoursJune, StandardHoursFor(totalHoursJune)),
                PercentAllocatedJuly = PercentAllocated(totalHoursJuly, StandardHoursFor(totalHoursJuly)),
                PercentAllocatedAugust = PercentAllocated(totalHoursAugust, StandardHoursFor(totalHoursAugust)),
                PercentAllocatedSeptember = PercentAllocated(totalHoursSeptember, StandardHoursFor(totalHoursSeptember)),
                PercentAllocatedOctober = PercentAllocated(totalHoursOctober, StandardHoursFor(totalHoursOctober)),
                PercentAllocatedNovember = PercentAllocated(totalHoursNovember, StandardHoursFor(totalHoursNovember)),
                PercentAllocatedDecember = PercentAllocated(totalHoursDecember, StandardHoursFor(totalHoursDecember)),
                PercentAllocatedJanuary = PercentAllocated(totalHoursJanuary, StandardHoursFor(totalHoursJanuary)),
                PercentAllocatedFebruary = PercentAllocated(totalHoursFebruary, StandardHoursFor(totalHoursFebruary)),
                PercentAllocatedMarch = PercentAllocated(totalHoursMarch, StandardHoursFor(totalHoursMarch))
            };
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }
    }
}
