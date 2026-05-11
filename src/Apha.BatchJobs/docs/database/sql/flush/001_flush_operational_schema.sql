-- Flush script for local/dev reset cycles.
-- WARNING: This removes data from the seeded ScheduledLoadFromFps footprint
-- across fps and mabarchive schemas.

BEGIN;

TRUNCATE TABLE
	fps.scheduled_load_validation_result,
	fps.scheduled_load_step_run,
	fps.scheduled_load_run,
	fps.job_queue_log,
	fps.job_queue,
	fps.job_status,
	fps.job_master,
	fps.job_lock,
	fps.fpsyeartotals,
	fps.tlkpproject,
	mabarchive.my_fpsyeartotals,
	mabarchive.my_tlkpproject_all,
	mabarchive.my_monthlyoutput,
	mabarchive.my_monthlytime,
	mabarchive.my_projectmonthfinal,
	mabarchive.my_proj_invoice,
	mabarchive.my_proj_subcontract,
	mabarchive.my_profitcentregrade,
	mabarchive.my_staff,
	mabarchive.my_tbladditionalcosts,
	mabarchive.my_tblanimalreq,
	mabarchive.my_tblanimals,
	mabarchive.my_tblcontract,
	mabarchive.my_tblprofitcentre,
	mabarchive.my_tblstaffjob,
	mabarchive.my_testorproduct,
	mabarchive.my_timecostcalcs,
	mabarchive.my_tlkpprogram,
	mabarchive.my_tlkpproject,
	mabarchive.my_tlkptestreqmt,
	mabarchive.my_workgroup,
	mabarchive.my_workgroupgrade,
	mabarchive.g_tlkpproject,
	mabarchive.tlkpyear
RESTART IDENTITY CASCADE;

COMMIT;

SELECT
	(SELECT COUNT(*) FROM fps.job_master) AS job_master_count,
	(SELECT COUNT(*) FROM fps.job_status) AS job_status_count,
	(SELECT COUNT(*) FROM fps.scheduled_load_run) AS scheduled_load_run_count,
	(SELECT COUNT(*) FROM fps.scheduled_load_step_run) AS scheduled_load_step_run_count,
	(SELECT COUNT(*) FROM fps.scheduled_load_validation_result) AS scheduled_load_validation_result_count,
	(SELECT COUNT(*) FROM fps.tlkpproject) AS tlkpproject_count,
	(SELECT COUNT(*) FROM fps.fpsyeartotals) AS fpsyeartotals_count,
	(SELECT COUNT(*) FROM mabarchive.my_fpsyeartotals) AS my_fpsyeartotals_count,
	(SELECT COUNT(*) FROM mabarchive.my_tlkpproject_all) AS my_tlkpproject_all_count;
