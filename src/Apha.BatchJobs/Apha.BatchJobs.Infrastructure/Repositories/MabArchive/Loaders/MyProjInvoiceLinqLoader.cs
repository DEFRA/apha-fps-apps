using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyProjInvoiceLinqLoader : MabArchiveLinqLoaderBase
{
    public override int Sequence => 7;

    public override string Name => "my_proj_invoice";

    protected override async Task<int> LoadWithLinqAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await context.MaSrcProjInvoice
            .AsNoTracking()
            .Where(i => i.FpsYear == year)
            .Select(i => new MaDstMyProjInvoice
            {
                Year = year,
                ProjectParent = i.ProjectParent,
                Month = i.Month,
                Amount = i.Amount,
                CostOfWork = i.CostOfWork,
                Wip = i.Wip,
                ProfitLoss = i.ProfitLoss,
                Detail = i.Detail,
                InvoiceCounter = i.InvoiceCounter,
                Type = i.Type
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyProjInvoice.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}
