-- ============================================================================
-- Database Flush & Reset Script
-- ============================================================================
-- Purpose: Remove all test/seed data from both fps and mabarchive schemas
--          Respects foreign key constraints by deleting dependent tables first
--
-- WARNING: This script DELETES data. Use only for development/testing.
--
-- ============================================================================

BEGIN;

-- ============================================================================
-- FPS SCHEMA: Delete dependent tables first (respect FKs)
-- ============================================================================

-- Delete invoice and subcontract detail data
DELETE FROM fps.proj_invoice WHERE projectparent IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');
DELETE FROM fps.proj_subcontract WHERE project IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete monthly outputs
DELETE FROM fps.monthlyoutput 
  WHERE buyer IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete test capabilities (must be before testorproduct/workgroup due FKs)
DELETE FROM fps.tlkptestcapability 
  WHERE testcode IN ('AQUA-001', 'TERR-001', 'INSP-001', 'DIAG-001');

-- Delete product/test data
DELETE FROM fps.testorproduct 
  WHERE itemcode IN ('AQUA-001', 'TERR-001', 'INSP-001', 'DIAG-001');

-- Delete test requirements
DELETE FROM fps.tlkptestreqmt 
  WHERE buyer IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete milestones
DELETE FROM fps.milestone 
  WHERE project IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete project months
DELETE FROM fps.projectmonth 
  WHERE project IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete parent projects (base table)
DELETE FROM fps.tlkpproject 
  WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete workgroup and program lookups seeded for test runs
DELETE FROM fps.workgroup
  WHERE workgroup IN ('WG001', 'WG002', 'WG003', 'WG004')
    AND fpsyear IN (2024, 2025, 2026);

DELETE FROM fps.tlkpprogram
  WHERE programno IN ('AH', 'TH', 'BS', 'RS')
    AND fpsyear IN (2024, 2025, 2026);

-- ============================================================================
-- MABARCHIVE SCHEMA: Delete archive snapshots
-- ============================================================================

-- Delete archive project snapshots
DELETE FROM mabarchive.my_tlkpproject_all 
  WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

-- Delete archive year totals
DELETE FROM mabarchive.my_fpsyeartotals 
  WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');

COMMIT;

-- ============================================================================
-- VERIFICATION
-- ============================================================================

-- Confirm records deleted
SELECT 'fps.tlkpproject' AS table_name, COUNT(*) as remaining_rows 
  FROM fps.tlkpproject WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004')
UNION ALL
SELECT 'fps.milestone', COUNT(*) 
  FROM fps.milestone WHERE project IN ('AH0001', 'TH0002', 'BS0003', 'RS0004')
UNION ALL
SELECT 'fps.proj_invoice', COUNT(*) 
  FROM fps.proj_invoice WHERE projectparent IN ('AH0001', 'TH0002', 'BS0003', 'RS0004')
UNION ALL
SELECT 'mabarchive.my_fpsyeartotals', COUNT(*) 
  FROM mabarchive.my_fpsyeartotals WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004')
UNION ALL
SELECT 'mabarchive.my_tlkpproject_all', COUNT(*) 
  FROM mabarchive.my_tlkpproject_all WHERE parentproject IN ('AH0001', 'TH0002', 'BS0003', 'RS0004');
