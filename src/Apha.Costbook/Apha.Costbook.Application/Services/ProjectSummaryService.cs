using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;
using ClosedXML.Excel;

namespace Apha.Costbook.Application.Services;

public class ProjectSummaryService : IProjectSummaryService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IMapper _mapper;

    public ProjectSummaryService(IProjectRepository projectRepo, IMapper mapper)
    {
        _projectRepo = projectRepo;
        _mapper = mapper;
    }

    public Task<double> GetProfitIncludedTotalAsync(string projectId, int year)
        => _projectRepo.GetProfitIncludedTotalAsync(projectId, year);

    public async Task<StaffYearsPivotDto> GetStaffYearsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetStaffYearsPivotAsync(projectId, parameters);
        return new StaffYearsPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new StaffYearsRowDto
            {
                Project = r.Project,
                Grade = r.Grade,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }
    public async Task<StaffEffortPivotDto> GetStaffEffortAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetStaffEffortAsync(projectId, parameters);
        return new StaffEffortPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new StaffEffortRowDto
            {
                Project = r.Project,
                WorkGroup = r.WorkGroup,
                GradeCode = r.GradeCode,
                Name = r.Name,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }

    public async Task<ProjectCostsPivotDto> GetProjectCostsPivotAsync(string projectId, QueryParameters<string>? query = null)
    {
        var parameters = query != null
            ? _mapper.Map<PaginationParameters<string>>(query)
            : null;

        var data = await _projectRepo.GetProjectCostsPivotAsync(projectId, parameters);
        return new ProjectCostsPivotDto
        {
            Years = data.Years,
            TotalCount = data.TotalCount,
            Rows = data.Rows.Select(r => new ProjectCostsRowDto
            {
                Project = r.Project,
                Category = r.Category,
                Total = r.Total,
                YearlyAmounts = r.YearlyAmounts
            }).ToList()
        };
    }

    public async Task<byte[]> ExportProjectSummaryToExcelAsync(string projectId)
    {
        var data = await _projectRepo.GetProjectSummaryExportDataAsync(projectId);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Project Summary");

        // Header block —  VBA Cells(1,1)..Cells(4,2)
        ws.Cell(1, 1).Value = "Costbook Project Summary";
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(2, 1).Value = "Project"; ws.Cell(2, 1).Style.Font.Bold = true;
        ws.Cell(2, 2).Value = data.Project?.ProjectId ?? projectId;
        ws.Cell(4, 1).Value = "Inflation"; ws.Cell(4, 1).Style.Font.Bold = true;
        ws.Cell(4, 2).Value = data.Project?.Inflation ?? 0;

        int rowNum = 7;

        foreach (var year in data.Years)
        {
            ws.Cell(rowNum, 1).Value = year.YearValue;
            ws.Cell(rowNum, 1).Style.Font.Bold = true;
            rowNum++;

            // Staff
            var staffForYear = data.StaffRequirements
                .Where(s => s.Year == year.YearValue).ToList();
            if (staffForYear.Count > 0)
            {
                ws.Cell(rowNum, 2).Value = "WG Grade"; ws.Cell(rowNum, 2).Style.Font.Bold = true;
                ws.Cell(rowNum, 3).Value = "Charge Rate"; ws.Cell(rowNum, 3).Style.Font.Bold = true;
                ws.Cell(rowNum, 4).Value = "No Hours"; ws.Cell(rowNum, 4).Style.Font.Bold = true;
                ws.Cell(rowNum, 6).Value = "Cost"; ws.Cell(rowNum, 6).Style.Font.Bold = true;
                rowNum++;
                foreach (var s in staffForYear)
                {
                    ws.Cell(rowNum, 2).Value = s.WgGrade;
                    ws.Cell(rowNum, 3).Value = s.Chargerate ?? 0;
                    ws.Cell(rowNum, 4).Value = s.Nohours ?? 0;
                    ws.Cell(rowNum, 6).Value = (s.Chargerate ?? 0) * (s.Nohours ?? 0);
                    rowNum++;
                }
            }

            // Tests
            var testsForYear = data.TestRequirements
                .Where(t => t.Year == year.YearValue).ToList();
            if (testsForYear.Count > 0)
            {
                ws.Cell(rowNum, 2).Value = "Test Code"; ws.Cell(rowNum, 2).Style.Font.Bold = true;
                ws.Cell(rowNum, 3).Value = "Unit Price"; ws.Cell(rowNum, 3).Style.Font.Bold = true;
                ws.Cell(rowNum, 4).Value = "No Tests"; ws.Cell(rowNum, 4).Style.Font.Bold = true;
                ws.Cell(rowNum, 6).Value = "Cost"; ws.Cell(rowNum, 6).Style.Font.Bold = true;
                rowNum++;
                foreach (var t in testsForYear)
                {
                    ws.Cell(rowNum, 2).Value = t.TestCode;
                    ws.Cell(rowNum, 3).Value = t.UnitPrice ?? 0;
                    ws.Cell(rowNum, 4).Value = t.NumberOfTests ?? 0;
                    ws.Cell(rowNum, 6).Value = (t.UnitPrice ?? 0) * (t.NumberOfTests ?? 0);
                    rowNum++;
                }
            }

            // Animals
            var animalsForYear = data.AnimalRequirements
                .Where(a => a.Year == year.YearValue).ToList();
            if (animalsForYear.Count > 0)
            {
                ws.Cell(rowNum, 2).Value = "Animal Type"; ws.Cell(rowNum, 2).Style.Font.Bold = true;
                ws.Cell(rowNum, 3).Value = "Daily Rate"; ws.Cell(rowNum, 3).Style.Font.Bold = true;
                ws.Cell(rowNum, 4).Value = "Number of Days"; ws.Cell(rowNum, 4).Style.Font.Bold = true;
                ws.Cell(rowNum, 5).Value = "Number of Animals"; ws.Cell(rowNum, 5).Style.Font.Bold = true;
                ws.Cell(rowNum, 6).Value = "Cost"; ws.Cell(rowNum, 6).Style.Font.Bold = true;
                rowNum++;
                foreach (var a in animalsForYear)
                {
                    ws.Cell(rowNum, 2).Value = a.AnimalType;
                    ws.Cell(rowNum, 3).Value = a.DailyRate ?? 0;
                    ws.Cell(rowNum, 4).Value = a.NumberOfDays ?? 0;
                    ws.Cell(rowNum, 5).Value = a.NumberOfAnimals ?? 0;
                    ws.Cell(rowNum, 6).Value = (a.DailyRate ?? 0) * (a.NumberOfDays ?? 0) * (a.NumberOfAnimals ?? 0);
                    rowNum++;
                }
            }

            // Additional Costs
            var additionalForYear = data.AdditionalCosts
                .Where(ac => ac.Year == year.YearValue).ToList();
            if (additionalForYear.Count > 0)
            {
                ws.Cell(rowNum, 2).Value = "Description"; ws.Cell(rowNum, 2).Style.Font.Bold = true;
                ws.Cell(rowNum, 3).Value = "Account Cat"; ws.Cell(rowNum, 3).Style.Font.Bold = true;
                ws.Cell(rowNum, 6).Value = "Cost"; ws.Cell(rowNum, 6).Style.Font.Bold = true;
                rowNum++;
                foreach (var ac in additionalForYear)
                {
                    ws.Cell(rowNum, 2).Value = ac.Description;
                    ws.Cell(rowNum, 3).Value = ac.AccountCat;
                    ws.Cell(rowNum, 6).Value = ac.ItemCost ?? 0;
                    rowNum++;
                }
            }

            rowNum++;
        }

        ws.Column(3).Style.NumberFormat.Format = "$#,##0.00";
        ws.Column(6).Style.NumberFormat.Format = "$#,##0.00";
        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<ProjectYearCostSummaryDto> GetProjectYearCostSummaryAsync(string projectId, int year)
    {
        var entity = await _projectRepo.GetProjectYearCostSummaryAsync(projectId, year);
        return new ProjectYearCostSummaryDto
        {
            Project             = entity.Project,
            Year                = entity.Year,
            StaffCostTotal      = entity.StaffCostTotal,
            TestCostTotal       = entity.TestCostTotal,
            AnimalCostTotal     = entity.AnimalCostTotal,
            AdditionalCostTotal = entity.AdditionalCostTotal
        };
    }
}