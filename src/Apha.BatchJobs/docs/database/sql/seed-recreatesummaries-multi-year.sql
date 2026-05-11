-- ============================================================================
-- RecreateSummaries Test Data Seed Script
-- ============================================================================
-- Purpose: Populate base tables with comprehensive multi-year sample data
--          to test RecreateSummaries batch job across various scenarios
-- 
-- Tables Seeded:
--   - tlkpproject (parent projects)
--   - projectmonth (monthly baselines)
--   - milestone (project milestones with on-time, late, pending status)
--   - tlkptestcapability (test codes × workgroups)
--   - tlkptestreqmt (test requirements with unit prices)
--   - monthlyoutput (monthly test volumes)
--   - testorproduct (product/test codes)
--   - proj_subcontract (subcontracted work)
--   - proj_invoice (invoiced amounts)
--
-- Target Years: 2024, 2025, 2026
-- Note: Uses actual schema from cloud database (fps schema)
-- ============================================================================

BEGIN;

-- ============================================================================
-- 1. PARENT PROJECTS (tlkpproject)
-- ============================================================================
-- Use actual columns from cloud schema

INSERT INTO fps.tlkpproject 
  (parentproject, projecttitle, program, customer, manager, transferincome, custincome, projectstatus, disease, contract, fpsyear)
VALUES
  -- Aquatic Health programme (3 years)
  ('AH0001', 'Aquatic Health - Core Testing', 'AH', 'DEFRA', 'John Smith', 5000.00, 4500.00, 'Active', 'Fish', '0', 2024),
  ('AH0001', 'Aquatic Health - Core Testing', 'AH', 'DEFRA', 'John Smith', 5500.00, 4950.00, 'Active', 'Fish', '0', 2025),
  ('AH0001', 'Aquatic Health - Core Testing', 'AH', 'DEFRA', 'John Smith', 6000.00, 5400.00, 'Active', 'Fish', '0', 2026),
  
  -- Terrestrial Health programme (3 years)
  ('TH0002', 'Terrestrial Health - Disease Control', 'TH', 'DEFRA', 'Jane Brown', 4000.00, 3600.00, 'Active', 'Cattle', '0', 2024),
  ('TH0002', 'Terrestrial Health - Disease Control', 'TH', 'DEFRA', 'Jane Brown', 4400.00, 3960.00, 'Active', 'Cattle', '0', 2025),
  ('TH0002', 'Terrestrial Health - Disease Control', 'TH', 'DEFRA', 'Jane Brown', 4800.00, 4320.00, 'Active', 'Cattle', '0', 2026),
  
  -- Biosecurity programme (3 years)
  ('BS0003', 'Biosecurity - Import Inspections', 'BS', 'APHA', 'Mike Johnson', 3500.00, 3150.00, 'Active', 'Wildlife', '0', 2024),
  ('BS0003', 'Biosecurity - Import Inspections', 'BS', 'APHA', 'Mike Johnson', 3850.00, 3465.00, 'Active', 'Wildlife', '0', 2025),
  ('BS0003', 'Biosecurity - Import Inspections', 'BS', 'APHA', 'Mike Johnson', 4235.00, 3811.50, 'Active', 'Wildlife', '0', 2026),
  
  -- Research programme (3 years)
  ('RS0004', 'Research Support - Diagnostics', 'RS', 'Academia', 'Sarah Davis', 2500.00, 2250.00, 'Active', 'Zoonotic', '0', 2024),
  ('RS0004', 'Research Support - Diagnostics', 'RS', 'Academia', 'Sarah Davis', 2750.00, 2475.00, 'Active', 'Zoonotic', '0', 2025),
  ('RS0004', 'Research Support - Diagnostics', 'RS', 'Academia', 'Sarah Davis', 3025.00, 2722.50, 'Active', 'Zoonotic', '0', 2026)
ON CONFLICT (parentproject, fpsyear) DO NOTHING;

-- ============================================================================
-- 2. PROJECT MONTHLY BASELINE (projectmonth)
-- ============================================================================

INSERT INTO fps.projectmonth (project, monthno, fpsyear)
WITH months AS (SELECT generate_series(1, 12) AS monthno),
     projects AS (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject)
SELECT p.parentproject, m.monthno, p.fpsyear
FROM projects p
CROSS JOIN months m
ON CONFLICT (project, monthno, fpsyear) DO NOTHING;

-- ============================================================================
-- 3. PROJECT MILESTONES (milestone)
-- ============================================================================
-- Mix of on-time, late, and pending milestones

INSERT INTO fps.milestone 
  (project, milestoneref, plandate, actualdate, monthnofin, year, fpsyear)
VALUES
  -- AH0001 milestones (2024)
  ('AH0001', 'AQ-Q1-2024', '2024-03-31'::date, '2024-03-28'::date, 3, '2024', 2024),
  ('AH0001', 'AQ-Q2-2024', '2024-06-30'::date, '2024-07-15'::date, 6, '2024', 2024),
  ('AH0001', 'AQ-Q3-2024', '2024-09-30'::date, NULL, 9, '2024', 2024),
  ('AH0001', 'AQ-Q4-2024', '2024-12-31'::date, '2024-12-20'::date, 12, '2024', 2024),
  
  -- AH0001 milestones (2025)
  ('AH0001', 'AQ-Q1-2025', '2025-03-31'::date, '2025-04-05'::date, 3, '2025', 2025),
  ('AH0001', 'AQ-Q2-2025', '2025-06-30'::date, '2025-06-25'::date, 6, '2025', 2025),
  ('AH0001', 'AQ-Q3-2025', '2025-09-30'::date, NULL, 9, '2025', 2025),
  
  -- AH0001 milestones (2026)
  ('AH0001', 'AQ-Q1-2026', '2026-03-31'::date, '2026-03-30'::date, 3, '2026', 2026),
  ('AH0001', 'AQ-Q2-2026', '2026-06-30'::date, NULL, 6, '2026', 2026),
  
  -- TH0002 milestones (2024)
  ('TH0002', 'TH-Q1-2024', '2024-02-29'::date, '2024-02-25'::date, 2, '2024', 2024),
  ('TH0002', 'TH-Q2-2024', '2024-05-31'::date, '2024-06-10'::date, 5, '2024', 2024),
  ('TH0002', 'TH-Q3-2024', '2024-08-31'::date, '2024-08-31'::date, 8, '2024', 2024),
  ('TH0002', 'TH-Q4-2024', '2024-11-30'::date, NULL, 11, '2024', 2024),
  
  -- TH0002 milestones (2025)
  ('TH0002', 'TH-Q1-2025', '2025-02-28'::date, '2025-02-20'::date, 2, '2025', 2025),
  ('TH0002', 'TH-Q2-2025', '2025-05-31'::date, NULL, 5, '2025', 2025),
  ('TH0002', 'TH-Q3-2025', '2025-08-31'::date, '2025-09-05'::date, 8, '2025', 2025),
  
  -- BS0003 milestones (2024)
  ('BS0003', 'BS-Q1-2024', '2024-01-31'::date, '2024-01-29'::date, 1, '2024', 2024),
  ('BS0003', 'BS-Q2-2024', '2024-04-30'::date, '2024-04-30'::date, 4, '2024', 2024),
  ('BS0003', 'BS-Q3-2024', '2024-07-31'::date, '2024-08-20'::date, 7, '2024', 2024),
  ('BS0003', 'BS-Q4-2024', '2024-10-31'::date, NULL, 10, '2024', 2024),
  
  -- BS0003 milestones (2025)
  ('BS0003', 'BS-Q1-2025', '2025-01-31'::date, NULL, 1, '2025', 2025),
  ('BS0003', 'BS-Q2-2025', '2025-04-30'::date, '2025-04-28'::date, 4, '2025', 2025),
  
  -- RS0004 milestones (2024)
  ('RS0004', 'RS-Q1-2024', '2024-03-31'::date, '2024-03-15'::date, 3, '2024', 2024),
  ('RS0004', 'RS-Q3-2024', '2024-09-30'::date, '2024-10-30'::date, 9, '2024', 2024),
  
  -- RS0004 milestones (2025)
  ('RS0004', 'RS-Q1-2025', '2025-03-31'::date, NULL, 3, '2025', 2025),
  ('RS0004', 'RS-Q3-2025', '2025-09-30'::date, '2025-09-30'::date, 9, '2025', 2025)
ON CONFLICT (project, milestoneref, fpsyear) DO NOTHING;

-- ============================================================================
-- 4. TEST CAPABILITIES (tlkptestcapability)
-- ============================================================================

INSERT INTO fps.tlkptestcapability (testcode, workgroup, planportfolio, smscode, fpsyear)
VALUES
  ('AQUA-001', 'WG001', 'AH0001', 'SMS-AQ-001', 2024),
  ('AQUA-001', 'WG001', 'AH0001', 'SMS-AQ-001', 2025),
  ('AQUA-001', 'WG001', 'AH0001', 'SMS-AQ-001', 2026),
  ('TERR-001', 'WG002', 'TH0002', 'SMS-TR-001', 2024),
  ('TERR-001', 'WG002', 'TH0002', 'SMS-TR-001', 2025),
  ('INSP-001', 'WG003', 'BS0003', 'SMS-BS-001', 2024),
  ('INSP-001', 'WG003', 'BS0003', 'SMS-BS-001', 2025),
  ('DIAG-001', 'WG004', 'RS0004', 'SMS-RS-001', 2024),
  ('DIAG-001', 'WG004', 'RS0004', 'SMS-RS-001', 2025)
ON CONFLICT (testcode, workgroup, fpsyear) DO NOTHING;

-- ============================================================================
-- 5. TEST REQUIREMENTS (tlkptestreqmt)
-- ============================================================================

INSERT INTO fps.tlkptestreqmt (buyer, testcode, norequired, unitprice, fpsyear)
VALUES
  ('AH0001', 'AQUA-001', 5, 150.00, 2024),
  ('AH0001', 'AQUA-001', 5, 155.00, 2025),
  ('AH0001', 'AQUA-001', 5, 160.00, 2026),
  ('TH0002', 'TERR-001', 8, 120.00, 2024),
  ('TH0002', 'TERR-001', 8, 125.00, 2025),
  ('BS0003', 'INSP-001', 10, 100.00, 2024),
  ('BS0003', 'INSP-001', 10, 105.00, 2025),
  ('RS0004', 'DIAG-001', 3, 250.00, 2024),
  ('RS0004', 'DIAG-001', 3, 260.00, 2025)
ON CONFLICT (buyer, testcode, fpsyear) DO NOTHING;

-- ============================================================================
-- 6. MONTHLY TEST OUTPUTS (monthlyoutput)
-- ============================================================================

INSERT INTO fps.monthlyoutput 
  (buyer, testcode, workgroup, month, volume, fpsyear)
VALUES
  -- AH0001: 2024
  ('AH0001', 'AQUA-001', 'WG001', 1, 50, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 2, 60, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 3, 65, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 4, 55, 2024),
  
  -- AH0001: 2025
  ('AH0001', 'AQUA-001', 'WG001', 1, 52, 2025),
  ('AH0001', 'AQUA-001', 'WG001', 2, 62, 2025),
  
  -- TH0002: 2024
  ('TH0002', 'TERR-001', 'WG002', 1, 40, 2024),
  ('TH0002', 'TERR-001', 'WG002', 2, 45, 2024),
  ('TH0002', 'TERR-001', 'WG002', 3, 50, 2024),
  
  -- TH0002: 2025
  ('TH0002', 'TERR-001', 'WG002', 1, 42, 2025),
  
  -- BS0003: 2024
  ('BS0003', 'INSP-001', 'WG003', 1, 30, 2024),
  ('BS0003', 'INSP-001', 'WG003', 2, 35, 2024),
  
  -- BS0003: 2025
  ('BS0003', 'INSP-001', 'WG003', 1, 32, 2025),
  
  -- RS0004: 2024
  ('RS0004', 'DIAG-001', 'WG004', 1, 8, 2024),
  ('RS0004', 'DIAG-001', 'WG004', 2, 10, 2024)
ON CONFLICT (buyer, testcode, workgroup, month, fpsyear) DO NOTHING;

-- ============================================================================
-- 7. PRODUCT/TEST REFERENCES (testorproduct)
-- ============================================================================

INSERT INTO fps.testorproduct (itemcode, itemdescription, fpsyear)
VALUES
  ('AQUA-001', 'Aquatic Microbiology - Basic', 2024),
  ('AQUA-001', 'Aquatic Microbiology - Basic', 2025),
  ('AQUA-001', 'Aquatic Microbiology - Basic', 2026),
  ('TERR-001', 'Terrestrial Pathology', 2024),
  ('TERR-001', 'Terrestrial Pathology', 2025),
  ('INSP-001', 'Import Health Certificate Review', 2024),
  ('INSP-001', 'Import Health Certificate Review', 2025),
  ('DIAG-001', 'Research Diagnostic Services', 2024),
  ('DIAG-001', 'Research Diagnostic Services', 2025)
ON CONFLICT (itemcode, fpsyear) DO NOTHING;

-- ============================================================================
-- 8. SUBCONTRACTS (proj_subcontract)
-- ============================================================================

INSERT INTO fps.proj_subcontract 
  (project, month, acctcode, amount, fpsyear)
VALUES
  -- AH0001: 2024
  ('AH0001', 1, 'Consulting', 3000.00, 2024),
  ('AH0001', 2, 'Consulting', 3500.00, 2024),
  
  -- AH0001: 2025
  ('AH0001', 1, 'Consulting', 3200.00, 2025),
  
  -- TH0002: 2024
  ('TH0002', 1, 'Consulting', 2500.00, 2024),
  ('TH0002', 2, 'Consulting', 2700.00, 2024),
  
  -- TH0002: 2025
  ('TH0002', 1, 'Consulting', 2600.00, 2025),
  
  -- BS0003: 2024
  ('BS0003', 1, 'Other', 1500.00, 2024),
  ('BS0003', 2, 'Other', 1600.00, 2024),
  
  -- BS0003: 2025
  ('BS0003', 1, 'Other', 1550.00, 2025),
  
  -- RS0004: 2024
  ('RS0004', 1, 'Other', 1000.00, 2024),
  ('RS0004', 2, 'Other', 1200.00, 2024)
ON CONFLICT (project, month, acctcode, fpsyear) DO NOTHING;

-- ============================================================================
-- 9. INVOICES (proj_invoice)
-- ============================================================================

INSERT INTO fps.proj_invoice 
  (projectparent, month, amount, costofwork, fpsyear)
VALUES
  -- AH0001: 2024
  ('AH0001', 1, 12500.00, 8000.00, 2024),
  ('AH0001', 2, 13000.00, 8500.00, 2024),
  ('AH0001', 3, 14000.00, 9000.00, 2024),
  
  -- AH0001: 2025
  ('AH0001', 1, 13000.00, 8300.00, 2025),
  ('AH0001', 2, 13500.00, 8700.00, 2025),
  
  -- TH0002: 2024
  ('TH0002', 1, 10000.00, 6500.00, 2024),
  ('TH0002', 2, 10500.00, 6800.00, 2024),
  
  -- TH0002: 2025
  ('TH0002', 1, 10500.00, 6700.00, 2025),
  
  -- BS0003: 2024
  ('BS0003', 1, 8500.00, 5500.00, 2024),
  ('BS0003', 2, 9000.00, 5800.00, 2024),
  
  -- BS0003: 2025
  ('BS0003', 1, 9000.00, 5700.00, 2025),
  
  -- RS0004: 2024
  ('RS0004', 1, 6500.00, 4000.00, 2024),
  ('RS0004', 2, 7000.00, 4300.00, 2024)
ON CONFLICT (projectparent, month, fpsyear) DO NOTHING;

COMMIT;

-- ============================================================================
-- VERIFICATION QUERIES (Uncomment to run)
-- ============================================================================

/*
SELECT 'tlkpproject' AS table_name, COUNT(*) AS count FROM fps.tlkpproject WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL
SELECT 'milestone', COUNT(*) FROM fps.milestone WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL
SELECT 'projectmonth', COUNT(*) FROM fps.projectmonth WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL
SELECT 'monthlyoutput', COUNT(*) FROM fps.monthlyoutput WHERE fpsyear IN (2024, 2025, 2026);

-- Verify RecreateSummaries view
SELECT project, duemonth, COUNT(*) as count 
FROM fps.qrymilestone1 
WHERE fpsyear IN (2024, 2025, 2026)
GROUP BY project, duemonth 
ORDER BY project, duemonth;
*/
