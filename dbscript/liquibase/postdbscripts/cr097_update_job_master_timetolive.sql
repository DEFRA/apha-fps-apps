--liquibase formatted sql

--changeset arihant:CR097 labels:dml context:all splitStatements:false

/* CR097: Set job_master.timetolive to 90 minutes for the six jobs that wait for the batch worker after dispatch.
   A request not picked up within this time is marked Failed, so it stops blocking new requests.
   Other jobs are unchanged. Safe to re-run. */

DO $$
DECLARE
    v_found int;
    v_rows  int;
BEGIN
    SELECT count(*) INTO v_found
    FROM fps.job_master
    WHERE jobname IN ('BulkTestRatesUpdate', 'BulkStaffRatesUpdate', 'BulkAnimalRatesUpdate',
                      'YearEnd-DataSetup', 'YearEnd-CutOver', 'RecreateSummary');
    IF v_found <> 6 THEN
        RAISE EXCEPTION 'Expected 6 job_master rows, found % - check job names in %', v_found, current_database();
    END IF;

    UPDATE fps.job_master
       SET timetolive = 90,
           updated_at = now()
     WHERE jobname IN ('BulkTestRatesUpdate', 'BulkStaffRatesUpdate', 'BulkAnimalRatesUpdate',
                       'YearEnd-DataSetup', 'YearEnd-CutOver', 'RecreateSummary')
       AND timetolive IS DISTINCT FROM 90;
    GET DIAGNOSTICS v_rows = ROW_COUNT;
    RAISE NOTICE 'timetolive set to 90 on % row(s) in %', v_rows, current_database();
END $$;

/* Verify */
SELECT jobid, jobname, timetolive, updated_at
FROM fps.job_master
WHERE jobname IN ('BulkTestRatesUpdate', 'BulkStaffRatesUpdate', 'BulkAnimalRatesUpdate',
                  'YearEnd-DataSetup', 'YearEnd-CutOver', 'RecreateSummary')
ORDER BY jobid;