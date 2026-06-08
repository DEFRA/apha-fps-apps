# MABArchive DbContext Mapping Gap Matrix

Status date: 2026-05-15
Scope: Phase 2 audit for LINQ migration readiness across all 24 MABArchive loaders.

## Verification Basis

- Loader source/target footprint extracted from:
  - Apha.BatchJobs.Infrastructure/Repositories/MabArchive/Loaders/MabArchiveLoaders.cs
- DbContext mapping inventory extracted from:
  - Apha.BatchJobs.Infrastructure/Data/BatchJobsDbContext.cs
- Result:
  - RecreateSummaries model mappings exist (fps schema).
  - No MABArchive-specific model block exists (no ConfigureMabArchiveModels).
  - No DbSet entries exist for mabarchive.my_* or mabarchive.g_tlkpproject/tlkpyear targets.

## Coverage Matrix

| Seq | Loader | Source Table(s) | Target Table | Source Model Mapping | Target Model Mapping | Gap |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | my_tlkpprogram | fps.tlkpprogram | mabarchive.my_tlkpprogram | Added (MaSrcTlkpProgram) | Added (MaDstMyTlkpProgram) | No |
| 2 | g_tlkpproject | fps.tlkpproject | mabarchive.g_tlkpproject | Added (MaSrcTlkpProject) | Added (MaDstGTlkpProject) | No |
| 3 | my_tlkpproject | fps.tlkpproject | mabarchive.my_tlkpproject | Added (MaSrcTlkpProject) | Added (MaDstMyTlkpProject) | No |
| 4 | my_fpsyeartotals | fps.fpsyeartotals | mabarchive.my_fpsyeartotals | Added (MaSrcFpsYearTotals) | Added (MaDstMyFpsYearTotals) | No |
| 5 | my_monthlyoutput | fps.monthlyoutput | mabarchive.my_monthlyoutput | Added (MaSrcMonthlyOutput) | Added (MaDstMyMonthlyOutput) | No |
| 6 | my_monthlytime | fps.monthlytime | mabarchive.my_monthlytime | Added (MaSrcMonthlyTime) | Added (MaDstMyMonthlyTime) | No |
| 7 | my_proj_invoice | fps.proj_invoice | mabarchive.my_proj_invoice | Added (MaSrcProjInvoice) | Added (MaDstMyProjInvoice) | No |
| 8 | my_proj_subcontract | fps.proj_subcontract | mabarchive.my_proj_subcontract | Added (MaSrcProjSubContract) | Added (MaDstMyProjSubContract) | No |
| 9 | my_projectmonthfinal | fps.projectmonthfinal | mabarchive.my_projectmonthfinal | Added (MaSrcProjectMonthFinal) | Added (MaDstMyProjectMonthFinal) | No |
| 10 | my_tbladditionalcosts | fps.tbladditionalcosts | mabarchive.my_tbladditionalcosts | Added (MaSrcTblAdditionalCosts) | Added (MaDstMyTblAdditionalCosts) | No |
| 11 | my_tblanimalreq | fps.tblanimalreq | mabarchive.my_tblanimalreq | Added (MaSrcTblAnimalReq) | Added (MaDstMyTblAnimalReq) | No |
| 12 | my_tblcontract | fps.tblcontract | mabarchive.my_tblcontract | Added (MaSrcTblContract) | Added (MaDstMyTblContract) | No |
| 13 | my_tblstaffjob | fps.tblstaffjob | mabarchive.my_tblstaffjob | Added (MaSrcTblStaffJob) | Added (MaDstMyTblStaffJob) | No |
| 14 | my_timecostcalcs | fps.timecostcalcs | mabarchive.my_timecostcalcs | Added (MaSrcTimeCostCalcs) | Added (MaDstMyTimeCostCalcs) | No |
| 15 | my_tlkptestreqmt | fps.tlkptestreqmt | mabarchive.my_tlkptestreqmt | Added (MaSrcTlkpTestReqmt) | Added (MaDstMyTlkpTestReqmt) | No |
| 16 | tlkpyear | fps.tbldb_variables | mabarchive.tlkpyear | Added (MaSrcTblDbVariable) | Added (MaDstTlkpYear) | No |
| 17 | my_workgroupgrade | fps.workgroupgrade | mabarchive.my_workgroupgrade | Added (MaSrcWorkGroupGrade) | Added (MaDstMyWorkGroupGrade) | No |
| 18 | my_profitcentregrade | fps.profitcentregrade | mabarchive.my_profitcentregrade | Added (MaSrcProfitCentreGrade) | Added (MaDstMyProfitCentreGrade) | No |
| 19 | my_tblprofitcentre | fps.tblkpprofitcentre | mabarchive.my_tblprofitcentre | Added (MaSrcTblkpProfitCentre) | Added (MaDstMyTblProfitCentre) | No |
| 20 | my_testorproduct | fps.testorproduct | mabarchive.my_testorproduct | Added (MaSrcTestOrProduct) | Added (MaDstMyTestOrProduct) | No |
| 21 | my_staff | fps.tblwgemployee + fps.tblemployee | mabarchive.my_staff | Added (MaSrcTblWgEmployee + MaSrcTblEmployee) | Added (MaDstMyStaff) | No |
| 22 | my_workgroup | fps.workgroup | mabarchive.my_workgroup | Added (MaSrcWorkGroup) | Added (MaDstMyWorkGroup) | No |
| 23 | my_tblanimals | fps.tblanimals | mabarchive.my_tblanimals | Added (MaSrcTblAnimals) | Added (MaDstMyTblAnimals) | No |
| 24 | my_tlkpproject_all | fps.tlkpproject | mabarchive.my_tlkpproject_all | Added (MaSrcTlkpProject) | Added (MaDstMyTlkpProjectAll) | No |

## Phase 2 Delta

- Confirmed gap reduced: 0/24 loaders remain unmapped after loaders 1-24 source/target model registration.
- Next implementation slice:
  - Move to Phase 3 loader framework refactor, preserving IMabArchiveLoader contract.
  - Introduce LINQ execution base path behind orchestration parity safeguards.
  - Keep SQL fallback available while individual LINQ loaders are introduced.
