using Apha.Costbook.DataAccess;

namespace Apha.Costbook.Core.Interfaces;

public interface IProjectYearRepository
{
    Task<IEnumerable<ProjectYear>> GetByProjectAsync(string project);
    Task<int?> GetMaxProjectYearAsync(string project);
    Task<ProjectYear> AddProjectYearAsync(string project, int year);
    Task<ProjectYear> UpdateProjectYearAsync(ProjectYear projectYear);
    Task<IEnumerable<PayRateLookup>> GetPayRatesAsync(bool isDefra);
}

public record PayRateLookup(string WgGrade, double? ChargeRate, double? PayRate, double? Npr, double? Ohr);
