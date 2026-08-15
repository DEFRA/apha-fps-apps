namespace Apha.FPS.Api.TestTools
{
    /// <summary>
    /// Embeds the exact, already-verified content of docs/sql/yearend-testing/yearend_reset_2026.sql
    /// (BatchJobs repo, git-ignored - see the parent conversation's note on why an external file
    /// path can't be relied on here). Embedded rather than read from disk so this compiles into the
    /// deployed API and works identically to how it was proven against `batchjobs` directly. Keep
    /// this in sync with the source file by hand if either changes - do not fork the logic.
    /// </summary>
    public static class YearEndResetSql
    {
        public const string Reset2026 = """
            -- yearend_reset_2026.sql (embedded copy - see class doc comment)
            BEGIN;

            DO $$
            DECLARE
                v_db text;
                v_active_lock_count int;
                v_running_count int;
                v_before_business_total bigint;
                v_after_business_total bigint;
                v_before_staging_total bigint;
                v_year2026_status text;
                v_open_year_count int;
                v_open_year int;
                v_matrix_tables text[] := ARRAY[
                    'projectmonth2','projectmonth3','tbladminusers','tblperiod','tbltestreqwg',
                    'tbltotalbusinessoverheads','tbluser_profitcentre','tbluser_program',
                    'tbluser_projectgroup','tbluser_testowner','tlkpmanager','workgroupmonth',
                    'additionalcosts_log','animalreq_log','fpsyeartotals','mo_log','mt_log',
                    'project_log','projectmonth','projectmonthfinal','recreatesummaries_log',
                    'staffjob_log','tblsurvff_fees','tblsurvff_submissions','tbltestreqbaseline',
                    'testreq_log','timecostcalcs',
                    'profitcentregrade_nondefra','tblanimalreq','tblpurchase','tbladditionalcosts',
                    'tbltestrequirementrccost','monthlytime','milestone','proj_invoice',
                    'proj_subcontract','tlkpjobcode','monthlyoutput','plancatwggrade','tblstaffjob',
                    'tblanimals','tblbid','tblwgemployee','tbltestrccost','tlkptestcapability',
                    'tlkptestreqmt','timecodevalid',
                    'tlkpproject','tblemployee','tblkpaccountcategory','testorproduct','workgroupgrade',
                    'profitcentregrade','tblcontract','tlkpprogram','workgroup',
                    'costcentre','divisiongrade',
                    'grade'
                ];
                v_tbl text;
                v_cnt bigint;
            BEGIN
                SELECT current_database() INTO v_db;
                IF v_db <> 'batchjobs' THEN
                    RAISE EXCEPTION 'yearend_reset_2026 refuses to run against database "%" -- this operation only ever targets batchjobs.', v_db;
                END IF;

                SELECT count(*) INTO v_active_lock_count FROM fps.job_lock WHERE job_name = 'YearEnd' AND is_active = true;
                IF v_active_lock_count > 0 THEN
                    RAISE EXCEPTION 'Refusing to reset -- the shared YearEnd lock is currently held.';
                END IF;

                SELECT count(*) INTO v_running_count
                FROM fps.job_queue q
                JOIN fps.job_master m ON m.jobid = q.jobid
                JOIN fps.job_status s ON s.statusid = q.statusid
                WHERE m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver') AND s.status = 'Running';
                IF v_running_count > 0 THEN
                    RAISE EXCEPTION 'Refusing to reset -- a YearEnd-DataSetup or YearEnd-CutOver execution is currently Running.';
                END IF;

                IF EXISTS (SELECT 1 FROM fps.tblpaymentschedule WHERE fpsyear = 2026) THEN
                    RAISE EXCEPTION 'Refusing to reset: fps.tblpaymentschedule contains 2026 rows and is outside the approved Year End reset scope.';
                END IF;

                v_before_business_total := 0;
                FOREACH v_tbl IN ARRAY v_matrix_tables LOOP
                    EXECUTE format('SELECT count(*) FROM fps.%I WHERE fpsyear = 2026', v_tbl) INTO v_cnt;
                    v_before_business_total := v_before_business_total + v_cnt;
                END LOOP;

                SELECT
                    (SELECT count(*) FROM fps.proj_subcontract_staging) +
                    (SELECT count(*) FROM fps.tblstagingmonthlyoutput) +
                    (SELECT count(*) FROM fps.tblstagingmonthlytime)
                INTO v_before_staging_total;

                SELECT yearstatus INTO v_year2026_status FROM fps.tblyearmaster WHERE fpsyear = 2026;

                RAISE NOTICE 'BEFORE: 2026 business rows (59-table matrix) = %, staging rows (3 tables) = %, tblyearmaster.2026.status = %',
                    v_before_business_total, v_before_staging_total, coalesce(v_year2026_status, '<absent>');

                FOREACH v_tbl IN ARRAY v_matrix_tables LOOP
                    EXECUTE format('DELETE FROM fps.%I WHERE fpsyear = 2026', v_tbl);
                END LOOP;

                TRUNCATE TABLE fps.proj_subcontract_staging, fps.tblstagingmonthlyoutput, fps.tblstagingmonthlytime RESTART IDENTITY;

                DELETE FROM fps.job_lock WHERE jobqueueid IN (
                    SELECT q.jobqueueid
                    FROM fps.job_queue q
                    JOIN fps.job_master m ON m.jobid = q.jobid
                    WHERE m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver') AND q.fpsyear = 2026
                );
                DELETE FROM fps.job_queue_log WHERE jobqueueid IN (
                    SELECT q.jobqueueid
                    FROM fps.job_queue q
                    JOIN fps.job_master m ON m.jobid = q.jobid
                    WHERE m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver') AND q.fpsyear = 2026
                );
                DELETE FROM fps.job_queue q
                USING fps.job_master m
                WHERE m.jobid = q.jobid
                  AND m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver')
                  AND q.fpsyear = 2026;

                UPDATE fps.tblyearmaster SET yearstatus = 'Open' WHERE fpsyear = 2025;

                DELETE FROM fps.tbluser_category WHERE fpsyear = 2026;
                DELETE FROM fps.tblsettings WHERE fpsyear = 2026;
                DELETE FROM fps.tlkpmonthhours WHERE fpsyear = 2026;
                DELETE FROM fps.tlkpprojectgroup WHERE fpsyear = 2026;

                DELETE FROM fps.tblyearmaster WHERE fpsyear = 2026;

                SELECT count(*), max(fpsyear) INTO v_open_year_count, v_open_year FROM fps.tblyearmaster WHERE yearstatus = 'Open';
                IF v_open_year_count <> 1 OR v_open_year <> 2025 THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: expected exactly one Open year (2025), found % Open year(s), max=%.', v_open_year_count, v_open_year;
                END IF;

                IF EXISTS (SELECT 1 FROM fps.tblyearmaster WHERE fpsyear = 2026) THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: fps.tblyearmaster still has a 2026 row.';
                END IF;

                v_after_business_total := 0;
                FOREACH v_tbl IN ARRAY v_matrix_tables LOOP
                    EXECUTE format('SELECT count(*) FROM fps.%I WHERE fpsyear = 2026', v_tbl) INTO v_cnt;
                    v_after_business_total := v_after_business_total + v_cnt;
                END LOOP;
                IF v_after_business_total <> 0 THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: % residual 2026 business rows remain across the matrix.', v_after_business_total;
                END IF;

                IF (SELECT count(*) FROM fps.tbluser_category WHERE fpsyear = 2026) <> 0
                   OR (SELECT count(*) FROM fps.tblsettings WHERE fpsyear = 2026) <> 0
                   OR (SELECT count(*) FROM fps.tlkpmonthhours WHERE fpsyear = 2026) <> 0
                   OR (SELECT count(*) FROM fps.tlkpprojectgroup WHERE fpsyear = 2026) <> 0 THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: 2026 tbluser_category/config rows still exist.';
                END IF;

                IF (SELECT count(*) FROM fps.proj_subcontract_staging) <> 0
                   OR (SELECT count(*) FROM fps.tblstagingmonthlyoutput) <> 0
                   OR (SELECT count(*) FROM fps.tblstagingmonthlytime) <> 0 THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: at least one staging table is not empty.';
                END IF;

                IF EXISTS (
                    SELECT 1 FROM fps.job_queue q
                    JOIN fps.job_master m ON m.jobid = q.jobid
                    WHERE m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver') AND q.fpsyear = 2026
                ) THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: YearEnd-DataSetup/YearEnd-CutOver job_queue rows for 2026 still exist.';
                END IF;

                IF EXISTS (SELECT 1 FROM fps.job_lock WHERE job_name = 'YearEnd' AND is_active = true) THEN
                    RAISE EXCEPTION 'Post-reset assertion failed: an active YearEnd lock exists after reset.';
                END IF;

                RAISE NOTICE 'AFTER: pre-initiation baseline reached -- 2026 business rows = 0 (was %), staging rows = 0 (was %), tblyearmaster.2025.status = Open, 2026 row + config absent.',
                    v_before_business_total, v_before_staging_total;
            END $$;

            SELECT
                (SELECT count(*) FROM fps.tblyearmaster WHERE fpsyear = 2025 AND yearstatus = 'Open') AS year_2025_open,
                (SELECT count(*) FROM fps.tblyearmaster WHERE fpsyear = 2026) AS year_2026_row_count,
                (SELECT count(*) FROM fps.proj_subcontract_staging) +
                (SELECT count(*) FROM fps.tblstagingmonthlyoutput) +
                (SELECT count(*) FROM fps.tblstagingmonthlytime) AS staging_rows_remaining,
                (SELECT count(*) FROM fps.tblsettings WHERE fpsyear = 2026) +
                (SELECT count(*) FROM fps.tlkpmonthhours WHERE fpsyear = 2026) +
                (SELECT count(*) FROM fps.tlkpprojectgroup WHERE fpsyear = 2026) +
                (SELECT count(*) FROM fps.tbluser_category WHERE fpsyear = 2026) AS config_2026_rows_remaining,
                (SELECT count(*) FROM fps.job_queue q JOIN fps.job_master m ON m.jobid = q.jobid
                 WHERE m.jobname IN ('YearEnd-DataSetup', 'YearEnd-CutOver') AND q.fpsyear = 2026) AS yearend_2026_job_rows_remaining,
                (SELECT count(*) FROM fps.job_lock WHERE job_name = 'YearEnd' AND is_active = true) AS active_yearend_locks;

            COMMIT;
            """;
    }
}
