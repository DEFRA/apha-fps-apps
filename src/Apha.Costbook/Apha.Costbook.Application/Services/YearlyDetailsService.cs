using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Application.Services;

public class YearlyDetailsService : IYearlyDetailsService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IProjectYearRepository _projectYearRepo;
    private readonly IStaffRequirementRepository _staffRepo;
    private readonly ITestRequirementRepository _testRepo;
    private readonly IAnimalRequirementRepository _animalRepo;
    private readonly IAdditionalCostRepository _additionalCostRepo;
    private readonly IMapper _mapper;

    public YearlyDetailsService(
        IProjectRepository projectRepo,
        IProjectYearRepository projectYearRepo,
        IStaffRequirementRepository staffRepo,
        ITestRequirementRepository testRepo,
        IAnimalRequirementRepository animalRepo,
        IAdditionalCostRepository additionalCostRepo,
        IMapper mapper)
    {
        _projectRepo = projectRepo;
        _projectYearRepo = projectYearRepo;
        _staffRepo = staffRepo;
        _testRepo = testRepo;
        _animalRepo = animalRepo;
        _additionalCostRepo = additionalCostRepo;
        _mapper = mapper;
    }

    public async Task<ProjectHeaderDto?> GetProjectHeaderAsync(string projectId)
    {
        var project = await _projectRepo.GetProjectByIdAsync(projectId);
        return project is null ? null : _mapper.Map<ProjectHeaderDto>(project);
    }

    public async Task<IEnumerable<ProjectYearDto>> GetProjectYearsAsync(string projectId)
    {
        var years = await _projectYearRepo.GetByProjectAsync(projectId);
        return _mapper.Map<IEnumerable<ProjectYearDto>>(years);
    }

    public async Task<ProjectYearDto> AddProjectYearAsync(string projectId, int year)
    {
        var added = await _projectYearRepo.AddProjectYearAsync(projectId, year);
        return _mapper.Map<ProjectYearDto>(added);
    }

    public async Task<ProjectYearDto> UpdateProjectYearAsync(ProjectYearDto dto)
    {
        var entity = _mapper.Map<ProjectYear>(dto);
        var updated = await _projectYearRepo.UpdateProjectYearAsync(entity);
        return _mapper.Map<ProjectYearDto>(updated);
    }

    // ── Staff ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<StaffRequirementDto>> GetStaffRequirementsAsync(string projectId, int year)
    {
        var rows = await _staffRepo.GetByProjectYearAsync(projectId, year);
        return rows.Select(r => new StaffRequirementDto
        {
            SrIdentity = r.SrIdentity,
            Project = r.Project,
            Year = r.Year,
            WgGrade = r.WgGrade,
            Name = r.Name,
            Nohours = r.Nohours,
            Nodays = r.Nodays,
            Chargerate = r.Chargerate,
            StaffCost = r.Chargerate.HasValue && r.Nohours.HasValue ? r.Chargerate.Value * r.Nohours.Value : null,
            Payrate = r.Payrate,
            Npr = r.Npr,
            Ohr = r.Ohr
        });
    }

    public async Task<StaffRequirementDto> AddStaffRequirementAsync(StaffRequirementDto dto)
    {
        var entity = _mapper.Map<StaffRequirement>(dto);
        var result = await _staffRepo.AddAsync(entity);
        return MapStaffToDto(result);
    }

    public async Task<StaffRequirementDto> UpdateStaffRequirementAsync(StaffRequirementDto dto)
    {
        var entity = _mapper.Map<StaffRequirement>(dto);
        var result = await _staffRepo.UpdateAsync(entity);
        return MapStaffToDto(result);
    }

    public async Task<bool> DeleteStaffRequirementAsync(int srIdentity)
        => await _staffRepo.DeleteAsync(srIdentity);

    // ── Tests ────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<TestRequirementDto>> GetTestRequirementsAsync(string projectId, int year)
    {
        var rows = await _testRepo.GetByProjectYearAsync(projectId, year);
        return rows.Select(r => new TestRequirementDto
        {
            Project = r.Project,
            Year = r.Year,
            TestCode = r.TestCode,
            NumberOfTests = r.NumberOfTests,
            UnitPrice = r.UnitPrice,
            TestCost = r.UnitPrice.HasValue && r.NumberOfTests.HasValue ? r.UnitPrice.Value * r.NumberOfTests.Value : null
        });
    }

    public async Task<TestRequirementDto> AddTestRequirementAsync(TestRequirementDto dto)
    {
        var entity = _mapper.Map<TestRequirement>(dto);
        var result = await _testRepo.AddAsync(entity);
        return MapTestToDto(result);
    }

    public async Task<TestRequirementDto> UpdateTestRequirementAsync(TestRequirementDto dto)
    {
        var entity = _mapper.Map<TestRequirement>(dto);
        var result = await _testRepo.UpdateAsync(entity);
        return MapTestToDto(result);
    }

    public async Task<bool> DeleteTestRequirementAsync(string projectId, int year, string testCode)
        => await _testRepo.DeleteAsync(projectId, year, testCode);

    // ── Animals ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AnimalRequirementDto>> GetAnimalRequirementsAsync(string projectId, int year)
    {
        var rows = await _animalRepo.GetByProjectYearAsync(projectId, year);
        return rows.Select(r => new AnimalRequirementDto
        {
            ArIdentity = r.ArIdentity,
            Project = r.Project,
            Year = r.Year,
            AnimalType = r.AnimalType,
            NumberOfDays = r.NumberOfDays,
            NumberOfAnimals = r.NumberOfAnimals,
            DailyRate = r.DailyRate,
            AnimalCost = r.NumberOfDays.HasValue && r.NumberOfAnimals.HasValue && r.DailyRate.HasValue
                ? r.NumberOfDays.Value * r.NumberOfAnimals.Value * r.DailyRate.Value
                : null
        });
    }

    public async Task<AnimalRequirementDto> AddAnimalRequirementAsync(AnimalRequirementDto dto)
    {
        var entity = _mapper.Map<AnimalRequirement>(dto);
        var result = await _animalRepo.AddAsync(entity);
        return MapAnimalToDto(result);
    }

    public async Task<AnimalRequirementDto> UpdateAnimalRequirementAsync(AnimalRequirementDto dto)
    {
        var entity = _mapper.Map<AnimalRequirement>(dto);
        var result = await _animalRepo.UpdateAsync(entity);
        return MapAnimalToDto(result);
    }

    public async Task<bool> DeleteAnimalRequirementAsync(int arIdentity)
        => await _animalRepo.DeleteAsync(arIdentity);

    // ── Additional Costs ─────────────────────────────────────────────────────

    public async Task<IEnumerable<AdditionalCostDto>> GetAdditionalCostsAsync(string projectId, int year)
    {
        var rows = await _additionalCostRepo.GetByProjectYearAsync(projectId, year);
        return _mapper.Map<IEnumerable<AdditionalCostDto>>(rows);
    }

    public async Task<AdditionalCostDto> AddAdditionalCostAsync(AdditionalCostDto dto)
    {
        var entity = _mapper.Map<AdditionalCost>(dto);
        var result = await _additionalCostRepo.AddAsync(entity);
        return _mapper.Map<AdditionalCostDto>(result);
    }

    public async Task<AdditionalCostDto> UpdateAdditionalCostAsync(AdditionalCostDto dto)
    {
        var entity = _mapper.Map<AdditionalCost>(dto);
        var result = await _additionalCostRepo.UpdateAsync(entity);
        return _mapper.Map<AdditionalCostDto>(result);
    }

    public async Task<bool> DeleteAdditionalCostAsync(int acIdentity)
        => await _additionalCostRepo.DeleteAsync(acIdentity);

    // ── Lookups ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<PayRateDto>> GetPayRatesAsync(bool isDefra)
    {
        var rates = await _projectYearRepo.GetPayRatesAsync(isDefra);
        return rates.Select(r => new PayRateDto
        {
            WgGrade = r.WgGrade,
            ChargeRate = r.ChargeRate,
            PayRate = r.PayRate,
            Npr = r.Npr,
            Ohr = r.Ohr
        });
    }

    public async Task<IEnumerable<AnimalRateDto>> GetAnimalRatesAsync(bool isDefra)
    {
        var rates = await _animalRepo.GetAnimalRatesAsync(isDefra);
        return rates.Select(r => new AnimalRateDto { AnimalType = r.AnimalType, DailyRate = r.DailyRate });
    }

    public async Task<IEnumerable<AccountCategoryDto>> GetAccountCategoriesAsync()
    {
        var cats = await _additionalCostRepo.GetProjectSpecificAccountCategoriesAsync();
        return cats.Select(c => new AccountCategoryDto { AccShortName = c.AccShortName, UseInflation = c.UseInflation });
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static StaffRequirementDto MapStaffToDto(StaffRequirement r) => new()
    {
        SrIdentity = r.SrIdentity,
        Project = r.Project,
        Year = r.Year,
        WgGrade = r.WgGrade,
        Name = r.Name,
        Nohours = r.Nohours,
        Nodays = r.Nodays,
        Chargerate = r.Chargerate,
        StaffCost = r.Chargerate.HasValue && r.Nohours.HasValue ? r.Chargerate.Value * r.Nohours.Value : null,
        Payrate = r.Payrate,
        Npr = r.Npr,
        Ohr = r.Ohr
    };

    private static TestRequirementDto MapTestToDto(TestRequirement r) => new()
    {
        Project = r.Project,
        Year = r.Year,
        TestCode = r.TestCode,
        NumberOfTests = r.NumberOfTests,
        UnitPrice = r.UnitPrice,
        TestCost = r.UnitPrice.HasValue && r.NumberOfTests.HasValue ? r.UnitPrice.Value * r.NumberOfTests.Value : null
    };

    private static AnimalRequirementDto MapAnimalToDto(AnimalRequirement r) => new()
    {
        ArIdentity = r.ArIdentity,
        Project = r.Project,
        Year = r.Year,
        AnimalType = r.AnimalType,
        NumberOfDays = r.NumberOfDays,
        NumberOfAnimals = r.NumberOfAnimals,
        DailyRate = r.DailyRate,
        AnimalCost = r.NumberOfDays.HasValue && r.NumberOfAnimals.HasValue && r.DailyRate.HasValue
            ? r.NumberOfDays.Value * r.NumberOfAnimals.Value * r.DailyRate.Value
            : null
    };
}
