using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class QueryReportService : IQueryReportService
    {
        private readonly IQueriesRepository _repository;
        private readonly IMapper _mapper;

        public QueryReportService(IQueriesRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<QueryReportItem>> GetQueryReportsAsync()
        {
            return await _repository.GetQueryReportsAsync();
        }

        public async Task<PagedData<MonitoringReportData>> GetMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            string contractFilter = "*",
            IEnumerable<string>? programFilter = null)
        {
            return await _repository.GetMonitoringReportDataAsync(
                parameters,
                reportYear,
                fiscalMonth,
                contractFilter,
                programFilter);
        }

        public async Task<PagedData<ProgramCustomerMonitoringReportData>> GetProgramCustomerMonitoringReportDataAsync(
            PaginationParameters<string> parameters,
            short reportYear,
            double fiscalMonth,
            IEnumerable<string>? programFilter = null)
        {
            return await _repository.GetProgramCustomerMonitoringReportDataAsync(
                parameters,
                reportYear,
                fiscalMonth,
                programFilter);
        }
    }
}
