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

        await context.MaDstMyStaff.AddRangeAsync(rows, cancellationToken);
           var inserted = await context.SaveChangesAsync(cancellationToken);

           // ? VALIDATION: Verify all rows were inserted
           if (inserted != rows.Count)
           {
               throw new InvalidOperationException(
                   $"Seq 21 MyStaff: Row count mismatch. Expected to insert {rows.Count} rows, " +
                   $"but SaveChangesAsync returned {inserted}.");
           }

           // ? VALIDATION: Verify JOIN result count against source WgEmployee table
           var sourceCount = await context.MaSrcTblWgEmployee
               .AsNoTracking()
               .Where(w => w.FpsYear == year)
               .CountAsync(cancellationToken);

           if (rows.Count != sourceCount)
           {
               throw new InvalidOperationException(
                   $"Seq 21 MyStaff: Row count mismatch. Loaded {rows.Count} rows from JOIN, " +
                   $"but source WgEmployee has {sourceCount}. Missing Employee JOIN records?");
           }

           // ? VALIDATION: Verify critical fields populated (Name should not be just ", ")
           var invalidNames = rows.Count(r => string.IsNullOrWhiteSpace(r.Name) || r.Name == ", ");
           if (invalidNames > 0)
           {
               throw new InvalidOperationException(
                   $"Seq 21 MyStaff: {invalidNames} rows have invalid Name field (both LastName and FirstName NULL).");
           }

           return inserted;
    }
}



