using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests
{
    public class ProjectInvoiceServiceTests
    {
        private readonly IProjectInvoiceRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectInvoiceService _service;

        public ProjectInvoiceServiceTests()
        {
            _repository = Substitute.For<IProjectInvoiceRepository>();
            _mapper = Substitute.For<IMapper>();
            _service = new ProjectInvoiceService(_repository, _mapper);
        }

        private static MonthlyInvoicesSummary MakeSummaryRow(string program, string project, int month, decimal amount)
            => new() { FpsYear = 2025, Program = program, ParentProject = project, Month = month, MonthlyAmount = amount };

        #region GetMonthlyInvoicesSummaryAsync

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithRows_GroupsByProgramAndProject()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var params_ = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(params_);
            _repository.GetMonthlyInvoicesSummaryAsync(params_).Returns(
            [
                MakeSummaryRow("PROG1", "PRJ001", 1, 100m),
                MakeSummaryRow("PROG1", "PRJ001", 2, 200m),
                MakeSummaryRow("PROG2", "PRJ002", 1, 300m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal(2, result.Rows.Count);
            var row1 = result.Rows.First(r => r.Program == "PROG1");
            Assert.Equal(100m, row1.MonthlyAmounts[1]);
            Assert.Equal(200m, row1.MonthlyAmounts[2]);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithNoRows_ReturnsEmptyPivot()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns([]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Empty(result.Rows);
            Assert.Empty(result.Months);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_PaginationMetadata_CalculatedCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 2 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            // 3 distinct (Program, Project) groups → TotalRecords = 3, TotalPages = 2
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                MakeSummaryRow("A", "PRJ1", 1, 10m),
                MakeSummaryRow("B", "PRJ2", 1, 20m),
                MakeSummaryRow("C", "PRJ3", 1, 30m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal(3, result.Pagination.TotalRecords);
            Assert.Equal(2, result.Pagination.TotalPages);
            Assert.Equal(1, result.Pagination.PageNumber);
            Assert.Equal(2, result.Pagination.PageSize);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByProgram_SortsAscendingByDefault()
        {
            // Arrange
            var query = new QueryParameters<string> { SortBy = "Program", Descending = false, Page = 1, PageSize = 10 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                MakeSummaryRow("Z", "PRJ2", 1, 10m),
                MakeSummaryRow("A", "PRJ1", 1, 20m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal("A", result.Rows[0].Program);
            Assert.Equal("Z", result.Rows[1].Program);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByMonthColumn_SortsByMonthlyAmount()
        {
            // Arrange
            var query = new QueryParameters<string> { SortBy = "M1", Descending = false, Page = 1, PageSize = 10 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                MakeSummaryRow("B", "PRJ2", 1, 500m),
                MakeSummaryRow("A", "PRJ1", 1, 100m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal(100m, result.Rows[0].MonthlyAmounts[1]);
            Assert.Equal(500m, result.Rows[1].MonthlyAmounts[1]);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_SortByProgramDescending_SortsDescending()
        {
            // Arrange
            var query = new QueryParameters<string> { SortBy = "Program", Descending = true, Page = 1, PageSize = 10 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                MakeSummaryRow("A", "PRJ1", 1, 10m),
                MakeSummaryRow("Z", "PRJ2", 1, 20m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal("Z", result.Rows[0].Program);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_PageTwo_ReturnsSecondPageRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 1 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                MakeSummaryRow("A", "PRJ1", 1, 10m),
                MakeSummaryRow("B", "PRJ2", 1, 20m)
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Single(result.Rows);
            Assert.Equal("B", result.Rows[0].Program);
            Assert.Equal(2, result.Pagination.PageNumber);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_RowWithNullAmount_DefaultsToZero()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<PaginationParameters<string>>(query).Returns(new PaginationParameters<string>());
            _repository.GetMonthlyInvoicesSummaryAsync(Arg.Any<PaginationParameters<string>>()).Returns(
            [
                new MonthlyInvoicesSummary { FpsYear = 2025, Program = "A", ParentProject = "PRJ1", Month = 1, MonthlyAmount = null }
            ]);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.Equal(0m, result.Rows[0].MonthlyAmounts[1]);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "", Month = 1, Amount = 100m };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_MissingMonth_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ001", Month = null, Amount = 100m };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_MissingAmount_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ001", Month = 1, Amount = null };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ValidDto_MapsAndDelegatesToRepository()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ001", Month = 1, Amount = 500m };
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001" };
            var created = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001" };
            var resultDto = new ProjectInvoiceDto { InvoiceCounter = 1 };
            _mapper.Map<ProjectInvoice>(dto).Returns(entity);
            _repository.CreateAsync(entity).Returns(created);
            _mapper.Map<ProjectInvoiceDto>(created).Returns(resultDto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.Equal(1, result.InvoiceCounter);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_MissingProject_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "", Month = 1, Amount = 100m };

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _service.UpdateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_ValidDto_MapsAndDelegatesToRepository()
        {
            // Arrange
            var dto = new ProjectInvoiceDto { ProjectParent = "PRJ001", Month = 2, Amount = 750m };
            var entity = new ProjectInvoice { ProjectParent = "PRJ001" };
            var updated = new ProjectInvoice { ProjectParent = "PRJ001" };
            var resultDto = new ProjectInvoiceDto { ProjectParent = "PRJ001" };
            _mapper.Map<ProjectInvoice>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<ProjectInvoiceDto>(updated).Returns(resultDto);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.Equal("PRJ001", result.ProjectParent);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_ReturnsTrue()
        {
            // Arrange
            _repository.DeleteAsync(5).Returns(true);

            // Act
            var result = await _service.DeleteAsync(5);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ReturnsFalse()
        {
            // Arrange
            _repository.DeleteAsync(99).Returns(false);

            // Act
            var result = await _service.DeleteAsync(99);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsMappedDto()
        {
            // Arrange
            var entity = new ProjectInvoice { InvoiceCounter = 1, ProjectParent = "PRJ001" };
            var dto = new ProjectInvoiceDto { InvoiceCounter = 1 };
            _repository.GetByIdAsync(1).Returns(entity);
            _mapper.Map<ProjectInvoiceDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.InvoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(99).Returns((ProjectInvoice?)null);

            // Act
            var result = await _service.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_ValidParentProject_DelegatesToRepository()
        {
            // Arrange
            _repository.GetTotalAmountAsync("PRJ001").Returns(2000m);

            // Act
            var result = await _service.GetTotalAmountAsync("PRJ001");

            // Assert
            Assert.Equal(2000m, result);
        }

        #endregion
    }
}
