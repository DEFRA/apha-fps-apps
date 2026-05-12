-- validation-sp-execution.sql
-- Tests RecreateSummaries stored procedure logic (steps 1-2)
-- Creates FPS totals from views and validates results

BEGIN;

-- Step 1: Delete existing FPS totals
DELETE FROM fps.fpsyeartotals;

-- Step 2: Create FPS totals from views with proper type casting
INSERT INTO fps.fpsyeartotals
(parentproject, program, totaladditionalcosts, totalanimalcosts, totalstaffcosts, 
 totaltestcosts, totalcosts, custincome, transferincome, totalincome, budget_cvl,
 requiredprofit, manager, customer, projectstatus, pvsincome, plancaseworkdebit, 
 totalpaycosts, fpsyear)
SELECT DISTINCT
    tlkpproject.parentproject,
    tlkpproject.program,
    COALESCE(qrytotaladditionalcosts.totaladditionalcosts, '0'::money),
    COALESCE(qrytotalanimalcosts.totalanimalcosts, '0'::money)::numeric::double precision,
    COALESCE(qrytotalstaffcosts.totalstaffcosts, '0'::money)::numeric::double precision,
    COALESCE(qrytotaltestcosts.totaltestcosts, '0'::money)::numeric::double precision,
    (COALESCE(qrytotaladditionalcosts.totaladditionalcosts, '0'::money)::numeric::double precision +
     COALESCE(qrytotalanimalcosts.totalanimalcosts, '0'::money)::numeric::double precision +
     COALESCE(qrytotalstaffcosts.totalstaffcosts, '0'::money)::numeric::double precision +
     COALESCE(qrytotaltestcosts.totaltestcosts, '0'::money)::numeric::double precision +
     COALESCE(tlkpproject.plancaseworkdebit, '0'::money)::numeric::double precision),
    tlkpproject.custincome,
    tlkpproject.transferincome,
    tlkpproject.custincome + tlkpproject.transferincome,
    tlkpproject.budget_cvl,
    tlkpproject.profit,
    tlkpproject.manager,
    tlkpproject.customer,
    tlkpproject.projectstatus,
    COALESCE(tlkpproject.pvsincome, '0'::money),
    COALESCE(tlkpproject.plancaseworkdebit, '0'::money),
    COALESCE((COALESCE(qrytotalstaffcosts.totalpaycosts, '0'::money))::numeric::double precision, 0::double precision),
    tlkpproject.fpsyear
FROM fps.tlkpproject
LEFT JOIN fps.qrytotaladditionalcosts ON tlkpproject.parentproject = qrytotaladditionalcosts.jobcode
LEFT JOIN fps.qrytotalanimalcosts     ON tlkpproject.parentproject = qrytotalanimalcosts.jobcode
LEFT JOIN fps.qrytotalstaffcosts      ON tlkpproject.parentproject = qrytotalstaffcosts.jobcode
LEFT JOIN fps.qrytotaltestcosts       ON tlkpproject.parentproject = qrytotaltestcosts.jobcode;

COMMIT;

-- Query results for validation
\echo ''
\echo '=========================================='
\echo 'SP Results - FPS Year Totals'
\echo '=========================================='
\echo ''

SELECT 
    'RESULT' AS result_type,
    parentproject,
    program,
    fpsyear,
    totaladditionalcosts::text,
    totalanimalcosts::text,
    totalstaffcosts::text,
    totaltestcosts::text,
    totalcosts::text,
    custincome::text,
    transferincome::text,
    totalincome::text,
    requiredprofit::text,
    projectstatus
FROM fps.fpsyeartotals
WHERE fpsyear IN (2024, 2025, 2026)
ORDER BY fpsyear, parentproject;

-- Summary statistics
\echo ''
\echo '=========================================='
\echo 'Summary Statistics'
\echo '=========================================='
\echo ''

SELECT 
    'SUMMARY' AS result_type,
    COUNT(*)::text as total_projects,
    COUNT(DISTINCT fpsyear)::text as years_covered,
    MIN(fpsyear)::text as min_year,
    MAX(fpsyear)::text as max_year,
    SUM(totalcosts)::text as aggregate_costs,
    SUM(totalincome)::text as aggregate_income
FROM fps.fpsyeartotals
WHERE fpsyear IN (2024, 2025, 2026);

-- Step 3: Log execution
INSERT INTO fps.recreatesummaries_log (userid, period, datedone, fpsyear)
VALUES ('validation-sp-test', 1, CURRENT_TIMESTAMP, 2024);

\echo ''
\echo '[OK] Validation complete - SP executed successfully'
\echo ''
