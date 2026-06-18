-- CR015 Cleanup: Drop temporary validation view after proof run
-- Date: 2026-06-18
-- Request Type: Drop
-- Schema: fps
-- Object Type: View

BEGIN;

DROP VIEW IF EXISTS fps.qrytotaltestcosts_refined_validation;

COMMIT;
