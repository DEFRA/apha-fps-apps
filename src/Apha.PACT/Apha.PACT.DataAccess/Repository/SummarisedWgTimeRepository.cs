using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Context;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository;

public class SummarisedWgTimeRepository : BaseRepository, ISummarisedWgTimeRepository
{
    private readonly IFpsRequestContext _fpsRequestContext;

    public SummarisedWgTimeRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) 
        : base(context)
    {
        _fpsRequestContext = fpsRequestContext;
    }
    public async Task<IEnumerable<SummarisedWgTimeView>> GetSummarisedWorkgroupTimeAsync(
        string? workGroup,
        CancellationToken cancellationToken = default)
    {
        int fpsYear = _fpsRequestContext.FpsYear;

        // Query the SummarisedWgTimeView DbSet directly (assumes the view is already created in the database)
        var query = _context.SummarisedWgTimeViews
            .AsNoTracking()
            .Where(v => v.FpsYear == fpsYear);

        if (!string.IsNullOrEmpty(workGroup))
        {
            query = query.Where(v => v.WorkGroup == workGroup);
        }

        var result = await query.ToListAsync(cancellationToken);
        return result;
    }
}
