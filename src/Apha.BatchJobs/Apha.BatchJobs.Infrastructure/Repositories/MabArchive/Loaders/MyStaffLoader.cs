using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyStaffLoader : MabArchiveExecutionLoaderBase
{
    public override int Sequence => 21;

    public override string Name => "my_staff";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var rows = await (
            from wge in context.MaSrcTblWgEmployee.AsNoTracking()
            join e in context.MaSrcTblEmployee.AsNoTracking()
                on wge.SpNumber equals e.SpNumber
            where wge.FpsYear == year
            select new MaDstMyStaff
            {
                Year = year,
                StaffId = wge.PactId,
                Name = (e.LastName ?? string.Empty) + ", " + (e.FirstName ?? string.Empty),
                WorkGroupGrade = wge.WorkGroupGrade,
                Title = e.Title,
                PersonStatus = wge.PersonStatus,
                PersonClass = wge.PersonClass,
                HrsPaid = wge.HrsPaid,
                LeaveHours = wge.LeaveHours,
                SickSpecial = wge.SickSpecial,
                HrsAvail = wge.HrsAvail
            })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return 0;
        }

        var distinctRows = rows
            .GroupBy(r => new { r.Year, r.StaffId })
            .Select(g => g.First())
            .ToList();

        await context.MaDstMyStaff.AddRangeAsync(distinctRows, cancellationToken);
        var inserted = await context.SaveChangesAsync(cancellationToken);

        if (inserted != distinctRows.Count)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: Row count mismatch. Expected to insert {distinctRows.Count} rows, " +
                $"but SaveChangesAsync returned {inserted}.");
        }

        var sourceCount = await context.MaSrcTblWgEmployee
            .AsNoTracking()
            .Where(w => w.FpsYear == year)
            .CountAsync(cancellationToken);

        if (distinctRows.Count > sourceCount)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: Deduped rows {distinctRows.Count} exceed source WgEmployee rows {sourceCount}.");
        }

        var invalidNames = distinctRows.Count(r => string.IsNullOrWhiteSpace(r.Name) || r.Name == ", ");
        if (invalidNames > 0)
        {
            throw new InvalidOperationException(
                $"Seq 21 MyStaff: {invalidNames} rows have invalid Name field (both LastName and FirstName NULL).");
        }

        return inserted;
    }
}



