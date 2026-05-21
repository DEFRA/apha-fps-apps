using Apha.PACT.Core.Entities;
using Apha.PACT.Core.ReadModels;

namespace Apha.PACT.Core.Interfaces
{
    public interface IWorkGroupReportRepository
    {
        Task<PactProfitCentreView?> GetProfitCentreAsync(string profitCentre);
        Task<IEnumerable<WorkGroup>> GetWorkGroupsForEmailAsync(string profitCentre);
        Task<IEnumerable<TimeSheetTemplateRow>>   GetTimeSheetTemplateAsync(string workGroup, short month, short layout);
        Task<IEnumerable<OutputSheetTemplateRow>> GetOutputSheetTemplateAsync(string workGroup, short month);
    }
}
