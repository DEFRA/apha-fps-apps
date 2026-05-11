-- ============================================================================
-- Comprehensive Multi-Schema Seed Script
-- ============================================================================
-- Purpose: Seed both RecreateSummaries (fps schema) and MABArchive 
--          (mabarchive schema) with wide test data scenarios
--
-- Schemas Seeded:
--   - fps: RecreateSummaries batch job base tables
--   - mabarchive: Archive snapshot tables for scheduled load processes
--
-- Coverage:
--   - 4 parent projects × 3 years (2024-2026) = 12 project instances
--   - 25+ milestones with on-time/late/pending status
--   - Monthly outputs, invoices, subcontracts across 2025-2026
--   - Archive baseline totals and project snapshots for MABArchive
--
-- ============================================================================

BEGIN;

-- ============================================================================
-- FPS SCHEMA: RecreateSummaries Core Data
-- ============================================================================

-- ========= 1. PARENT PROJECTS (tlkpproject) =========
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

-- ========= 2. PROJECT MONTHLY BASELINE (projectmonth) =========
INSERT INTO fps.projectmonth (project, monthno, fpsyear)
WITH months AS (SELECT generate_series(1, 12) AS monthno),
     projects AS (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject)
SELECT p.parentproject, m.monthno, p.fpsyear
FROM projects p
CROSS JOIN months m
ON CONFLICT (project, monthno, fpsyear) DO NOTHING;

-- ========= 3. PROJECT MILESTONES (milestone) =========
-- Mix of on-time, late, and pending status
INSERT INTO fps.milestone 
  (project, milestoneref, plandate, actualdate, monthnofin, year, fpsyear)
VALUES
  -- AH0001 (2024)
  ('AH0001', 'AQ-Q1-2024', '2024-03-31'::date, '2024-03-28'::date, 3, '2024', 2024),
  ('AH0001', 'AQ-Q2-2024', '2024-06-30'::date, '2024-07-15'::date, 6, '2024', 2024),
  ('AH0001', 'AQ-Q3-2024', '2024-09-30'::date, NULL, 9, '2024', 2024),
  ('AH0001', 'AQ-Q4-2024', '2024-12-31'::date, '2024-12-20'::date, 12, '2024', 2024),
  -- AH0001 (2025)
  ('AH0001', 'AQ-Q1-2025', '2025-03-31'::date, '2025-04-05'::date, 3, '2025', 2025),
  ('AH0001', 'AQ-Q2-2025', '2025-06-30'::date, '2025-06-25'::date, 6, '2025', 2025),
  ('AH0001', 'AQ-Q3-2025', '2025-09-30'::date, NULL, 9, '2025', 2025),
  -- AH0001 (2026)
  ('AH0001', 'AQ-Q1-2026', '2026-03-31'::date, '2026-03-30'::date, 3, '2026', 2026),
  ('AH0001', 'AQ-Q2-2026', '2026-06-30'::date, NULL, 6, '2026', 2026),
  
  -- TH0002 (2024)
  ('TH0002', 'TH-Q1-2024', '2024-02-29'::date, '2024-02-25'::date, 2, '2024', 2024),
  ('TH0002', 'TH-Q2-2024', '2024-05-31'::date, '2024-06-10'::date, 5, '2024', 2024),
  ('TH0002', 'TH-Q3-2024', '2024-08-31'::date, '2024-08-31'::date, 8, '2024', 2024),
  ('TH0002', 'TH-Q4-2024', '2024-11-30'::date, NULL, 11, '2024', 2024),
  -- TH0002 (2025)
  ('TH0002', 'TH-Q1-2025', '2025-02-28'::date, '2025-02-20'::date, 2, '2025', 2025),
  ('TH0002', 'TH-Q2-2025', '2025-05-31'::date, NULL, 5, '2025', 2025),
  ('TH0002', 'TH-Q3-2025', '2025-08-31'::date, '2025-09-05'::date, 8, '2025', 2025),
  
  -- BS0003 (2024)
  ('BS0003', 'BS-Q1-2024', '2024-01-31'::date, '2024-01-29'::date, 1, '2024', 2024),
  ('BS0003', 'BS-Q2-2024', '2024-04-30'::date, '2024-04-30'::date, 4, '2024', 2024),
  ('BS0003', 'BS-Q3-2024', '2024-07-31'::date, '2024-08-20'::date, 7, '2024', 2024),
  ('BS0003', 'BS-Q4-2024', '2024-10-31'::date, NULL, 10, '2024', 2024),
  -- BS0003 (2025)
  ('BS0003', 'BS-Q1-2025', '2025-01-31'::date, NULL, 1, '2025', 2025),
  ('BS0003', 'BS-Q2-2025', '2025-04-30'::date, '2025-04-28'::date, 4, '2025', 2025),
  
  -- RS0004 (2024)
  ('RS0004', 'RS-Q1-2024', '2024-03-31'::date, '2024-03-15'::date, 3, '2024', 2024),
  ('RS0004', 'RS-Q3-2024', '2024-09-30'::date, '2024-10-30'::date, 9, '2024', 2024),
  -- RS0004 (2025)
  ('RS0004', 'RS-Q1-2025', '2025-03-31'::date, NULL, 3, '2025', 2025),
  ('RS0004', 'RS-Q3-2025', '2025-09-30'::date, '2025-09-30'::date, 9, '2025', 2025)
ON CONFLICT (project, milestoneref, fpsyear) DO NOTHING;

-- ========= 4. TEST CAPABILITIES (tlkptestcapability) =========
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

-- ========= 5. TEST REQUIREMENTS (tlkptestreqmt) =========
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

-- ========= 6. MONTHLY TEST OUTPUTS (monthlyoutput) =========
INSERT INTO fps.monthlyoutput 
  (buyer, testcode, workgroup, month, volume, fpsyear)
VALUES
  ('AH0001', 'AQUA-001', 'WG001', 1, 50, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 2, 60, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 3, 65, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 4, 55, 2024),
  ('AH0001', 'AQUA-001', 'WG001', 1, 52, 2025),
  ('AH0001', 'AQUA-001', 'WG001', 2, 62, 2025),
  ('TH0002', 'TERR-001', 'WG002', 1, 40, 2024),
  ('TH0002', 'TERR-001', 'WG002', 2, 45, 2024),
  ('TH0002', 'TERR-001', 'WG002', 3, 50, 2024),
  ('TH0002', 'TERR-001', 'WG002', 1, 42, 2025),
  ('BS0003', 'INSP-001', 'WG003', 1, 30, 2024),
  ('BS0003', 'INSP-001', 'WG003', 2, 35, 2024),
  ('BS0003', 'INSP-001', 'WG003', 1, 32, 2025),
  ('RS0004', 'DIAG-001', 'WG004', 1, 8, 2024),
  ('RS0004', 'DIAG-001', 'WG004', 2, 10, 2024)
ON CONFLICT (buyer, testcode, workgroup, month, fpsyear) DO NOTHING;

-- ========= 7. PRODUCT CODES (testorproduct) =========
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

-- ========= 8. SUBCONTRACTS (proj_subcontract) =========
INSERT INTO fps.proj_subcontract 
  (project, month, acctcode, amount, fpsyear)
VALUES
  ('AH0001', 1, 'Consulting', 3000.00, 2024),
  ('AH0001', 2, 'Consulting', 3500.00, 2024),
  ('AH0001', 1, 'Consulting', 3200.00, 2025),
  ('TH0002', 1, 'Consulting', 2500.00, 2024),
  ('TH0002', 2, 'Consulting', 2700.00, 2024),
  ('TH0002', 1, 'Consulting', 2600.00, 2025),
  ('BS0003', 1, 'Other', 1500.00, 2024),
  ('BS0003', 2, 'Other', 1600.00, 2024),
  ('BS0003', 1, 'Other', 1550.00, 2025),
  ('RS0004', 1, 'Other', 1000.00, 2024),
  ('RS0004', 2, 'Other', 1200.00, 2024)
ON CONFLICT (project, month, acctcode, fpsyear) DO NOTHING;

-- ========= 9. INVOICES (proj_invoice) =========
INSERT INTO fps.proj_invoice 
  (projectparent, month, amount, costofwork, fpsyear)
VALUES
  ('AH0001', 1, 12500.00, 8000.00, 2024),
  ('AH0001', 2, 13000.00, 8500.00, 2024),
  ('AH0001', 3, 14000.00, 9000.00, 2024),
  ('AH0001', 1, 13000.00, 8300.00, 2025),
  ('AH0001', 2, 13500.00, 8700.00, 2025),
  ('TH0002', 1, 10000.00, 6500.00, 2024),
  ('TH0002', 2, 10500.00, 6800.00, 2024),
  ('TH0002', 1, 10500.00, 6700.00, 2025),
  ('BS0003', 1, 8500.00, 5500.00, 2024),
  ('BS0003', 2, 9000.00, 5800.00, 2024),
  ('BS0003', 1, 9000.00, 5700.00, 2025),
  ('RS0004', 1, 6500.00, 4000.00, 2024),
  ('RS0004', 2, 7000.00, 4300.00, 2024)
ON CONFLICT (projectparent, month, fpsyear) DO NOTHING;

-- ============================================================================
-- MABARCHIVE SCHEMA: Scheduled Load Archive Data
-- ============================================================================

-- ========= 10. ARCHIVE YEAR TOTALS (mabarchive.my_fpsyeartotals) =========
INSERT INTO mabarchive.my_fpsyeartotals (
    year,
    parentproject,
    program,
    totaladditionalcosts,
    totalanimalcosts,
    totalstaffcosts,
    totaltestcosts,
    totalcosts,
    custincome,
    transferincome,
    totalincome,
    budget_cvl,
    requiredprofit,
    manager,
    customer,
    projectstatus,
    pvsincome,
    plancaseworkdebit,
    totalpaycosts
)
VALUES
    (2024, 'AH0001', 'AH', 1200.00, 5500.00, 12200.00, 3100.00, 21600.00, 4500.00, 5000.00, 9500.00, 50000.00, 1500.00, 'John Smith', 'DEFRA', 'Active', 0.00, 600.00, 12200.00),
    (2024, 'TH0002', 'TH', 1100.00, 4500.00, 11000.00, 2800.00, 19400.00, 3600.00, 4000.00, 7600.00, 45000.00, 1300.00, 'Jane Brown', 'DEFRA', 'Active', 0.00, 500.00, 11000.00),
    (2024, 'BS0003', 'BS', 800.00, 3200.00, 8500.00, 2000.00, 14500.00, 3150.00, 3500.00, 6650.00, 40000.00, 900.00, 'Mike Johnson', 'APHA', 'Active', 0.00, 350.00, 8500.00),
    (2024, 'RS0004', 'RS', 600.00, 2000.00, 7000.00, 1500.00, 11100.00, 2250.00, 2500.00, 4750.00, 30000.00, 800.00, 'Sarah Davis', 'Academia', 'Active', 0.00, 250.00, 7000.00),
    
    (2025, 'AH0001', 'AH', 1300.00, 5800.00, 12600.00, 3200.00, 23000.00, 4950.00, 5500.00, 10450.00, 52000.00, 1600.00, 'John Smith', 'DEFRA', 'Active', 0.00, 650.00, 12600.00),
    (2025, 'TH0002', 'TH', 1200.00, 4800.00, 11500.00, 2900.00, 20400.00, 3960.00, 4400.00, 8360.00, 47000.00, 1400.00, 'Jane Brown', 'DEFRA', 'Active', 0.00, 550.00, 11500.00),
    (2025, 'BS0003', 'BS', 900.00, 3400.00, 8800.00, 2100.00, 15200.00, 3465.00, 3850.00, 7315.00, 42000.00, 1000.00, 'Mike Johnson', 'APHA', 'Active', 0.00, 380.00, 8800.00),
    (2025, 'RS0004', 'RS', 650.00, 2200.00, 7300.00, 1600.00, 11750.00, 2475.00, 2750.00, 5225.00, 32000.00, 850.00, 'Sarah Davis', 'Academia', 'Active', 0.00, 275.00, 7300.00),
    
    (2026, 'AH0001', 'AH', 1400.00, 6100.00, 13000.00, 3300.00, 24400.00, 5400.00, 6000.00, 11400.00, 54000.00, 1700.00, 'John Smith', 'DEFRA', 'Active', 0.00, 700.00, 13000.00),
    (2026, 'TH0002', 'TH', 1300.00, 5100.00, 12000.00, 3000.00, 21400.00, 4320.00, 4800.00, 9120.00, 49000.00, 1500.00, 'Jane Brown', 'DEFRA', 'Active', 0.00, 600.00, 12000.00),
    (2026, 'BS0003', 'BS', 1000.00, 3600.00, 9100.00, 2200.00, 15900.00, 3811.50, 4235.00, 8046.50, 44000.00, 1100.00, 'Mike Johnson', 'APHA', 'Active', 0.00, 420.00, 9100.00),
    (2026, 'RS0004', 'RS', 700.00, 2400.00, 7600.00, 1700.00, 12400.00, 2722.50, 3025.00, 5747.50, 34000.00, 900.00, 'Sarah Davis', 'Academia', 'Active', 0.00, 300.00, 7600.00)
ON CONFLICT (year, parentproject)
DO UPDATE SET
    program = EXCLUDED.program,
    totaladditionalcosts = EXCLUDED.totaladditionalcosts,
    totalanimalcosts = EXCLUDED.totalanimalcosts,
    totalstaffcosts = EXCLUDED.totalstaffcosts,
    totaltestcosts = EXCLUDED.totaltestcosts,
    totalcosts = EXCLUDED.totalcosts,
    custincome = EXCLUDED.custincome,
    transferincome = EXCLUDED.transferincome,
    totalincome = EXCLUDED.totalincome,
    budget_cvl = EXCLUDED.budget_cvl,
    requiredprofit = EXCLUDED.requiredprofit,
    manager = EXCLUDED.manager,
    customer = EXCLUDED.customer,
    projectstatus = EXCLUDED.projectstatus,
    pvsincome = EXCLUDED.pvsincome,
    plancaseworkdebit = EXCLUDED.plancaseworkdebit,
    totalpaycosts = EXCLUDED.totalpaycosts;

-- ========= 11. ARCHIVE PROJECT SNAPSHOT (mabarchive.my_tlkpproject_all) =========
INSERT INTO mabarchive.my_tlkpproject_all (
    year,
    parentproject,
    program,
    customer,
    manager,
    transferincome,
    custincome,
    wip_eoy,
    wip_limit,
    wip_current,
    projectstatus,
    datecreated,
    feccost,
    profit,
    budget_cvl,
    caseworksub,
    pvsincome,
    plancaseworkdebit,
    source,
    disease,
    contract,
    finished,
    comments,
    carryover,
    isdefraproject,
    costcentre,
    oracleprojectcode,
    subaccountcode,
    projectgroup,
    incomeaccountcode
)
VALUES
    (2024, 'AH0001', 'AH', 'DEFRA', 'John Smith', 5000.00, 4500.00, 500.00, 2500.00, 1000.00, 'Active', DATE '2024-01-15', 600.00, 9000.00, 50000.00, 0.0500, 0.00, 600.00, 'FPS', 'Fish', 'C001', 0, '2024 baseline archive', 250.00, 1, 1001.0, 'ORA-AH0001-24', 'SUB_A', 'GROUP_AQ', 'INC_AQ'),
    (2024, 'TH0002', 'TH', 'DEFRA', 'Jane Brown', 4000.00, 3600.00, 400.00, 2000.00, 800.00, 'Active', DATE '2024-02-10', 550.00, 8100.00, 45000.00, 0.0450, 0.00, 500.00, 'FPS', 'Cattle', 'C002', 0, '2024 baseline archive', 200.00, 1, 1002.0, 'ORA-TH0002-24', 'SUB_B', 'GROUP_TH', 'INC_TH'),
    (2024, 'BS0003', 'BS', 'APHA', 'Mike Johnson', 3500.00, 3150.00, 300.00, 1500.00, 600.00, 'Active', DATE '2024-03-05', 450.00, 6500.00, 40000.00, 0.0400, 0.00, 350.00, 'FPS', 'Wildlife', 'C003', 0, '2024 baseline archive', 150.00, 0, 1003.0, 'ORA-BS0003-24', 'SUB_C', 'GROUP_BS', 'INC_BS'),
    (2024, 'RS0004', 'RS', 'Academia', 'Sarah Davis', 2500.00, 2250.00, 200.00, 1000.00, 400.00, 'Active', DATE '2024-03-20', 350.00, 4500.00, 30000.00, 0.0350, 0.00, 250.00, 'FPS', 'Zoonotic', 'C004', 0, '2024 baseline archive', 100.00, 0, 1004.0, 'ORA-RS0004-24', 'SUB_D', 'GROUP_RS', 'INC_RS'),
    
    (2025, 'AH0001', 'AH', 'DEFRA', 'John Smith', 5500.00, 4950.00, 550.00, 2700.00, 1100.00, 'Active', DATE '2025-01-20', 650.00, 9500.00, 52000.00, 0.0500, 0.00, 650.00, 'FPS', 'Fish', 'C001', 0, '2025 baseline archive', 275.00, 1, 1001.0, 'ORA-AH0001-25', 'SUB_A', 'GROUP_AQ', 'INC_AQ'),
    (2025, 'TH0002', 'TH', 'DEFRA', 'Jane Brown', 4400.00, 3960.00, 440.00, 2200.00, 880.00, 'Active', DATE '2025-02-12', 600.00, 8600.00, 47000.00, 0.0450, 0.00, 550.00, 'FPS', 'Cattle', 'C002', 0, '2025 baseline archive', 220.00, 1, 1002.0, 'ORA-TH0002-25', 'SUB_B', 'GROUP_TH', 'INC_TH'),
    (2025, 'BS0003', 'BS', 'APHA', 'Mike Johnson', 3850.00, 3465.00, 330.00, 1650.00, 660.00, 'Active', DATE '2025-03-07', 500.00, 7000.00, 42000.00, 0.0400, 0.00, 380.00, 'FPS', 'Wildlife', 'C003', 0, '2025 baseline archive', 165.00, 0, 1003.0, 'ORA-BS0003-25', 'SUB_C', 'GROUP_BS', 'INC_BS'),
    (2025, 'RS0004', 'RS', 'Academia', 'Sarah Davis', 2750.00, 2475.00, 220.00, 1100.00, 440.00, 'Active', DATE '2025-03-15', 380.00, 4900.00, 32000.00, 0.0350, 0.00, 275.00, 'FPS', 'Zoonotic', 'C004', 0, '2025 baseline archive', 110.00, 0, 1004.0, 'ORA-RS0004-25', 'SUB_D', 'GROUP_RS', 'INC_RS'),
    
    (2026, 'AH0001', 'AH', 'DEFRA', 'John Smith', 6000.00, 5400.00, 600.00, 2900.00, 1200.00, 'Active', DATE '2026-01-25', 700.00, 10000.00, 54000.00, 0.0500, 0.00, 700.00, 'FPS', 'Fish', 'C001', 0, '2026 baseline archive', 300.00, 1, 1001.0, 'ORA-AH0001-26', 'SUB_A', 'GROUP_AQ', 'INC_AQ'),
    (2026, 'TH0002', 'TH', 'DEFRA', 'Jane Brown', 4800.00, 4320.00, 480.00, 2400.00, 960.00, 'Active', DATE '2026-02-18', 650.00, 9100.00, 49000.00, 0.0450, 0.00, 600.00, 'FPS', 'Cattle', 'C002', 0, '2026 baseline archive', 240.00, 1, 1002.0, 'ORA-TH0002-26', 'SUB_B', 'GROUP_TH', 'INC_TH'),
    (2026, 'BS0003', 'BS', 'APHA', 'Mike Johnson', 4235.00, 3811.50, 360.00, 1800.00, 720.00, 'Active', DATE '2026-03-10', 550.00, 7500.00, 44000.00, 0.0400, 0.00, 420.00, 'FPS', 'Wildlife', 'C003', 0, '2026 baseline archive', 180.00, 0, 1003.0, 'ORA-BS0003-26', 'SUB_C', 'GROUP_BS', 'INC_BS'),
    (2026, 'RS0004', 'RS', 'Academia', 'Sarah Davis', 3025.00, 2722.50, 240.00, 1200.00, 480.00, 'Active', DATE '2026-03-22', 420.00, 5400.00, 34000.00, 0.0350, 0.00, 300.00, 'FPS', 'Zoonotic', 'C004', 0, '2026 baseline archive', 120.00, 0, 1004.0, 'ORA-RS0004-26', 'SUB_D', 'GROUP_RS', 'INC_RS')
ON CONFLICT (year, parentproject)
DO UPDATE SET
    program = EXCLUDED.program,
    customer = EXCLUDED.customer,
    manager = EXCLUDED.manager,
    transferincome = EXCLUDED.transferincome,
    custincome = EXCLUDED.custincome,
    wip_eoy = EXCLUDED.wip_eoy,
    wip_limit = EXCLUDED.wip_limit,
    wip_current = EXCLUDED.wip_current,
    projectstatus = EXCLUDED.projectstatus,
    datecreated = EXCLUDED.datecreated,
    feccost = EXCLUDED.feccost,
    profit = EXCLUDED.profit,
    budget_cvl = EXCLUDED.budget_cvl,
    caseworksub = EXCLUDED.caseworksub,
    pvsincome = EXCLUDED.pvsincome,
    plancaseworkdebit = EXCLUDED.plancaseworkdebit,
    source = EXCLUDED.source,
    disease = EXCLUDED.disease,
    contract = EXCLUDED.contract,
    finished = EXCLUDED.finished,
    comments = EXCLUDED.comments,
    carryover = EXCLUDED.carryover,
    isdefraproject = EXCLUDED.isdefraproject,
    costcentre = EXCLUDED.costcentre,
    oracleprojectcode = EXCLUDED.oracleprojectcode,
    subaccountcode = EXCLUDED.subaccountcode,
    projectgroup = EXCLUDED.projectgroup,
    incomeaccountcode = EXCLUDED.incomeaccountcode;

COMMIT;

-- ============================================================================
-- VERIFICATION QUERIES (Uncomment to run)
-- ============================================================================

/*
-- Count records by schema
SELECT 'fps.tlkpproject' AS name, COUNT(*) FROM fps.tlkpproject WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL SELECT 'fps.milestone', COUNT(*) FROM fps.milestone WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL SELECT 'fps.projectmonth', COUNT(*) FROM fps.projectmonth WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL SELECT 'fps.monthlyoutput', COUNT(*) FROM fps.monthlyoutput WHERE fpsyear IN (2024, 2025, 2026)
UNION ALL SELECT 'mabarchive.my_fpsyeartotals', COUNT(*) FROM mabarchive.my_fpsyeartotals WHERE year IN (2024, 2025, 2026)
UNION ALL SELECT 'mabarchive.my_tlkpproject_all', COUNT(*) FROM mabarchive.my_tlkpproject_all WHERE year IN (2024, 2025, 2026);

-- Verify RecreateSummaries view
SELECT project, duemonth, COUNT(*) as milestone_count 
FROM fps.qrymilestone1 
WHERE fpsyear IN (2024, 2025, 2026)
GROUP BY project, duemonth 
ORDER BY project, duemonth;

-- Verify MABArchive baseline totals
SELECT year, parentproject, totalcosts, totalincome, requiredprofit 
FROM mabarchive.my_fpsyeartotals 
WHERE year IN (2024, 2025, 2026)
ORDER BY year, parentproject;
*/
