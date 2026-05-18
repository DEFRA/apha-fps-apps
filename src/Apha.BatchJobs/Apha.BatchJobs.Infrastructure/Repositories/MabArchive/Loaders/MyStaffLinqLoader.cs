using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyStaffDotNetLoader : MabArchiveDotNetLoaderBase
{
    public override int Sequence => 21;

    public override string Name => "my_staff";

    protected override async Task<int> LoadWithDotNetAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
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
        return await context.SaveChangesAsync(cancellationToken);
    }
}


