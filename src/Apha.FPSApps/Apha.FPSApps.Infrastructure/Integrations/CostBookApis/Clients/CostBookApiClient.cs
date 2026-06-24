/*
 * TRANSFORMENGINE MIGRATION — CostBookApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 9 — Infrastructure API Client Implementation (Step 14)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Replaced three null! stub properties with concrete client instantiations:
 *       CostbookMaintenance  → new CostBookMaintenanceApiClient(http, mapper)
 *       CostbookCapsStaff    → new CostBookCapsStaffApiClient(http, mapper)
 *       CostbookAccountGroup → new CostBookAccountGroupApiClient(http, mapper)
 *   - Constructor now wires all 12 sub-clients (9 existing + 3 new)
 *   - Removed null! field initializers and TRANSFORMENGINE TODO STUB comments for the three resolved clients
 *
 * PRESERVED:
 *   - All pre-existing sub-client properties and constructor assignments unchanged
 *   - ICostBookApiClient interface satisfied in full
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */

using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookApiClient : ICostBookApiClient
    {
        public ICostBookProjectApiClient Projects { get; }
        public ICostBookCustomerApiClient Customers { get; }
        public ICostBookDiseaseApiClient Diseases { get; }
        public ICostBookProgramApiClient Programs { get; }
        public ICostBookStaffApiClient Staff { get; }
        public ICostBookContractApiClient Contracts { get; }
        public ICostBookYearlyDetailsApiClient YearlyDetails { get; }
        public ICostBookProjectSummaryApiClient ProjectSummary { get; }
        public ICostBookSettingsApiClient CostbookSettings { get; }

        // TRANSFORMENGINE: Concrete client wired in Phase 9 — targets backend MaintenanceController at api/v1/maintenance
        public ICostBookMaintenanceApiClient CostbookMaintenance { get; }

        // TRANSFORMENGINE: Concrete client wired in Phase 9 — targets backend CapsStaffController at api/v1/capsstaff
        public ICostBookCapsStaffApiClient CostbookCapsStaff { get; }

        // TRANSFORMENGINE: Concrete client wired in Phase 9 — targets backend AccountGroupController at api/v1/accountgroup
        public ICostBookAccountGroupApiClient CostbookAccountGroup { get; }

        public CostBookApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            Projects = new CostBookProjectApiClient(http, mapper);
            Customers = new CostBookCustomerApiClient(http, mapper);
            Diseases = new CostBookDiseaseApiClient(http, mapper);
            Programs = new CostBookProgramApiClient(http, mapper);
            Staff = new CostBookStaffApiClient(http, mapper);
            Contracts = new CostBookContractApiClient(http, mapper);
            YearlyDetails = new CostBookYearlyDetailsApiClient(http, mapper);
            ProjectSummary = new CostBookProjectSummaryApiClient(http, mapper);
            CostbookSettings = new CostBookSettingsApiClient(http, mapper);
            // TRANSFORMENGINE: Phase 9 — wire three new maintenance-area API clients
            CostbookMaintenance = new CostBookMaintenanceApiClient(http, mapper);
            CostbookCapsStaff = new CostBookCapsStaffApiClient(http, mapper);
            CostbookAccountGroup = new CostBookAccountGroupApiClient(http, mapper);
        }
    }
}
