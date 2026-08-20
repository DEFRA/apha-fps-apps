using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.QueryReportServiceTest
{
    public class QueryReportServiceTests
    {
        private readonly IQueriesRepository _repository;
        private readonly IMapper _mapper;
        private readonly QueryReportService _service;

        public QueryReportServiceTests()
        {
            _repository = Substitute.For<IQueriesRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new QueryReportService(_repository, _mapper);
        }

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new QueryReportService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new QueryReportService(_repository, null!));
        }

        #endregion

        #region GetQueryReportsAsync

        [Fact]
        public async Task GetQueryReportsAsync_RepositoryReturnsData_ReturnsSameData()
        {
            // Arrange
            var reports = new List<QueryReportItem>
            {
                new() { ReportName = "R1", ReportDescription = "Report 1" },
                new() { ReportName = "R2", ReportDescription = "Report 2" }
            };

            _repository.GetQueryReportsAsync().Returns(reports);

            // Act
            var result = await _service.GetQueryReportsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("R1", result[0].ReportName);
            await _repository.Received(1).GetQueryReportsAsync();
        }

        [Fact]
        public async Task GetQueryReportsAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _repository.GetQueryReportsAsync().ThrowsAsync(new InvalidOperationException("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetQueryReportsAsync());
        }

        #endregion

        #region GetMonitoringReportDataAsync

        [Fact]
        public async Task GetMonitoringReportDataAsync_PassesParametersToRepository_AndReturnsPagedData()
        {
            // Arrange
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10) { Filter = "{}", SortBy = "Program" };
            const short reportYear = 2025;
            const double fiscalMonth = 6;
            const string contractFilter = "LabTGen";
            var programFilter = new List<string> { "TB" };

            var expected = new PagedData<MonitoringReportData>(
                new List<MonitoringReportData> { new() { Program = "TB", ParentProject = "PP001" } },
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });

            _repository.GetMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, contractFilter, programFilter)
                .Returns(expected);

            // Act
            var result = await _service.GetMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, contractFilter, programFilter);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            await _repository.Received(1).GetMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, contractFilter, programFilter);
        }

        [Fact]
        public async Task GetMonitoringReportDataAsync_WhenOptionalFiltersOmitted_UsesDefaults()
        {
            // Arrange
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            var expected = new PagedData<MonitoringReportData>(
                new List<MonitoringReportData>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _repository.GetMonitoringReportDataAsync(parameters, 2024, 10, "*", null).Returns(expected);

            // Act
            var result = await _service.GetMonitoringReportDataAsync(parameters, 2024, 10);

            // Assert
            Assert.Empty(result.Data);
            await _repository.Received(1).GetMonitoringReportDataAsync(parameters, 2024, 10, "*", null);
        }

        #endregion

        #region GetProgramCustomerMonitoringReportDataAsync

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_PassesParametersToRepository_AndReturnsPagedData()
        {
            // Arrange
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10) { Filter = "{}" };
            const short reportYear = 2025;
            const double fiscalMonth = 8;
            var programFilter = new List<string> { "AMR", "TB" };

            var expected = new PagedData<ProgramCustomerMonitoringReportData>(
                new List<ProgramCustomerMonitoringReportData> { new() { Program = "AMR", ParentProject = "PP100" } },
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 });

            _repository.GetProgramCustomerMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, programFilter)
                .Returns(expected);

            // Act
            var result = await _service.GetProgramCustomerMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, programFilter);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            await _repository.Received(1).GetProgramCustomerMonitoringReportDataAsync(parameters, reportYear, fiscalMonth, programFilter);
        }

        [Fact]
        public async Task GetProgramCustomerMonitoringReportDataAsync_WithNullProgramFilter_PassesNullToRepository()
        {
            // Arrange
            var parameters = new PaginationParameters<string>(page: 1, pageSize: 10);

            var expected = new PagedData<ProgramCustomerMonitoringReportData>(
                new List<ProgramCustomerMonitoringReportData>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _repository.GetProgramCustomerMonitoringReportDataAsync(parameters, 2024, 1, null).Returns(expected);

            // Act
            var result = await _service.GetProgramCustomerMonitoringReportDataAsync(parameters, 2024, 1, null);

            // Assert
            Assert.Empty(result.Data);
            await _repository.Received(1).GetProgramCustomerMonitoringReportDataAsync(parameters, 2024, 1, null);
        }

        #endregion
    }
}
