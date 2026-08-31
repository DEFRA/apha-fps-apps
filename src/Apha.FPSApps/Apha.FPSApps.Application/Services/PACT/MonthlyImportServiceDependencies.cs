using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Interfaces.PACT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Apha.FPSApps.Application.Services.PACT
{
    public interface IMonthlyImportServiceDependencies
    {
        IExcelImportService ExcelImportService { get; }
        IWorkGroupService WorkGroupService { get; }
        IPactTimeCodeValidService TimeCodeValidService { get; }
        IMonthService MonthService { get; }
        IS3StorageService S3StorageService { get; }
        IHttpContextAccessor HttpContextAccessor { get; }
        IConfiguration Configuration { get; }
    }

    public class MonthlyImportServiceDependencies : IMonthlyImportServiceDependencies
    {
        public MonthlyImportServiceDependencies(
            IExcelImportService excelImportService,
            IWorkGroupService workGroupService,
            IPactTimeCodeValidService timeCodeValidService,
            IMonthService monthService,
            IS3StorageService s3StorageService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            ExcelImportService = excelImportService;
            WorkGroupService = workGroupService;
            TimeCodeValidService = timeCodeValidService;
            MonthService = monthService;
            S3StorageService = s3StorageService;
            HttpContextAccessor = httpContextAccessor;
            Configuration = configuration;
        }

        public IExcelImportService ExcelImportService { get; }
        public IWorkGroupService WorkGroupService { get; }
        public IPactTimeCodeValidService TimeCodeValidService { get; }
        public IMonthService MonthService { get; }
        public IS3StorageService S3StorageService { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public IConfiguration Configuration { get; }
    }
}
