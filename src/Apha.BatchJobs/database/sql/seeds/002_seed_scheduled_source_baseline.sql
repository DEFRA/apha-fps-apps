-- Seed: deterministic fixture data for the ScheduledLoadFromFps source and archive-support footprint.
-- Scope:
--   - fps.fpsyeartotals
--   - fps.tlkpproject
--   - mabarchive.my_fpsyeartotals
--   - mabarchive.my_tlkpproject_all
--   - representative mabarchive.my_* support tables used in year-scoped archive reload flows
-- Safe to re-run: uses ON CONFLICT guards.

BEGIN;

-- Source totals used by the yearly archive refresh flow.
INSERT INTO fps.fpsyeartotals (
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
    totalpaycosts,
    fpsyear
)
VALUES
    -- 2025 fixtures
    ('P001_2025', 'PROG_A', 1000.00, 5000.00, 12000.00, 3000.00, 21500.00, 25000.00, 15000.00, 40000.00, 50000.00, 2000.00, 'John Doe',   'CUSTOMER_A', 'Active',    0.00, 500.00, 12000.00, 2025),
    ('P002_2025', 'PROG_B', 2000.00, 8000.00, 15000.00, 4000.00, 30000.00, 30000.00, 20000.00, 50000.00, 60000.00, 3000.00, 'Jane Smith', 'CUSTOMER_B', 'Active',  500.00, 1000.00, 15000.00, 2025),
    ('P003_2025', 'PROG_C', 1500.00, 6000.00, 14000.00, 3500.00, 25750.00, 28000.00, 18000.00, 46000.00, 55000.00, 2500.00, 'Bob Johnson','CUSTOMER_C', 'Completed',250.00, 750.00, 14000.00, 2025),

    -- 2026 fixtures
    ('P001_2026', 'PROG_A', 1100.00, 5500.00, 12500.00, 3200.00, 22850.00, 26000.00, 16000.00, 42000.00, 52000.00, 2200.00, 'John Doe',   'CUSTOMER_A', 'Active',    0.00, 550.00, 12500.00, 2026),
    ('P002_2026', 'PROG_B', 2100.00, 8500.00, 15500.00, 4200.00, 31400.00, 31000.00, 21000.00, 52000.00, 62000.00, 3100.00, 'Jane Smith', 'CUSTOMER_B', 'Active',  550.00, 1100.00, 15500.00, 2026),
    ('P003_2026', 'PROG_C', 1600.00, 6500.00, 14500.00, 3700.00, 27100.00, 29000.00, 19000.00, 48000.00, 57000.00, 2600.00, 'Bob Johnson','CUSTOMER_C', 'Active',  300.00, 800.00, 14500.00, 2026)
ON CONFLICT (parentproject)
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
    totalpaycosts = EXCLUDED.totalpaycosts,
    fpsyear = EXCLUDED.fpsyear;

-- Project master rows used by enrichment/archive projection logic.
INSERT INTO fps.tlkpproject (
    parentproject,
    projecttitle,
    program,
    customer,
    manager,
    transferincome,
    custincome,
    projectstatus,
    disease,
    isdefraproject,
    incomeaccountcode,
    fpsyear
)
VALUES
    ('P001_2025', 'Project 001 FY2025', 'PROG_A', 'CUSTOMER_A', 'John Doe',    15000.00, 25000.00, 'Active',    'GEN', 0, 'INC_A', 2025),
    ('P002_2025', 'Project 002 FY2025', 'PROG_B', 'CUSTOMER_B', 'Jane Smith',  20000.00, 30000.00, 'Active',    'GEN', 0, 'INC_B', 2025),
    ('P003_2025', 'Project 003 FY2025', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 18000.00, 28000.00, 'Completed', 'GEN', 0, 'INC_C', 2025),
    ('P001_2026', 'Project 001 FY2026', 'PROG_A', 'CUSTOMER_A', 'John Doe',    16000.00, 26000.00, 'Active',    'GEN', 0, 'INC_A', 2026),
    ('P002_2026', 'Project 002 FY2026', 'PROG_B', 'CUSTOMER_B', 'Jane Smith',  21000.00, 31000.00, 'Active',    'GEN', 0, 'INC_B', 2026),
    ('P003_2026', 'Project 003 FY2026', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 19000.00, 29000.00, 'Active',    'GEN', 0, 'INC_C', 2026)
ON CONFLICT (parentproject)
DO UPDATE SET
    projecttitle = EXCLUDED.projecttitle,
    program = EXCLUDED.program,
    customer = EXCLUDED.customer,
    manager = EXCLUDED.manager,
    transferincome = EXCLUDED.transferincome,
    custincome = EXCLUDED.custincome,
    projectstatus = EXCLUDED.projectstatus,
    disease = EXCLUDED.disease,
    isdefraproject = EXCLUDED.isdefraproject,
    incomeaccountcode = EXCLUDED.incomeaccountcode,
    fpsyear = EXCLUDED.fpsyear;

-- Archive totals baseline for current seeded years.
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
    (2025, 'P001_2025', 'PROG_A', 1000.00, 5000.00, 12000.00, 3000.00, 21500.00, 25000.00, 15000.00, 40000.00, 50000.00, 2000.00, 'John Doe',   'CUSTOMER_A', 'Active',    0.00,  500.00, 12000.00),
    (2025, 'P002_2025', 'PROG_B', 2000.00, 8000.00, 15000.00, 4000.00, 30000.00, 30000.00, 20000.00, 50000.00, 60000.00, 3000.00, 'Jane Smith', 'CUSTOMER_B', 'Active',  500.00, 1000.00, 15000.00),
    (2025, 'P003_2025', 'PROG_C', 1500.00, 6000.00, 14000.00, 3500.00, 25750.00, 28000.00, 18000.00, 46000.00, 55000.00, 2500.00, 'Bob Johnson','CUSTOMER_C', 'Completed',250.00,  750.00, 14000.00),
    (2026, 'P001_2026', 'PROG_A', 1100.00, 5500.00, 12500.00, 3200.00, 22850.00, 26000.00, 16000.00, 42000.00, 52000.00, 2200.00, 'John Doe',   'CUSTOMER_A', 'Active',    0.00,  550.00, 12500.00),
    (2026, 'P002_2026', 'PROG_B', 2100.00, 8500.00, 15500.00, 4200.00, 31400.00, 31000.00, 21000.00, 52000.00, 62000.00, 3100.00, 'Jane Smith', 'CUSTOMER_B', 'Active',  550.00, 1100.00, 15500.00),
    (2026, 'P003_2026', 'PROG_C', 1600.00, 6500.00, 14500.00, 3700.00, 27100.00, 29000.00, 19000.00, 48000.00, 57000.00, 2600.00, 'Bob Johnson','CUSTOMER_C', 'Active',  300.00,  800.00, 14500.00)
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
    (2025, 'P001_2025', 'PROG_A', 'CUSTOMER_A', 'John Doe',    15000.00, 25000.00, 1000.00, 5000.00, 2000.00, 'Active',    DATE '2025-01-15', 1200.00, 18500.00, 50000.00, 0.1000,   0.00,  500.00, 'FPS', 'GEN', 'C001', 0, 'Baseline archive row',  500.00, 0, 1001.0, 'ORA-P001-25', 'SUB_A', 'GROUP_A', 'INC_A'),
    (2025, 'P002_2025', 'PROG_B', 'CUSTOMER_B', 'Jane Smith',  20000.00, 30000.00, 1500.00, 6000.00, 2500.00, 'Active',    DATE '2025-02-10', 1400.00, 20000.00, 60000.00, 0.1500, 500.00, 1000.00, 'FPS', 'GEN', 'C002', 0, 'Baseline archive row',  750.00, 0, 1002.0, 'ORA-P002-25', 'SUB_B', 'GROUP_B', 'INC_B'),
    (2025, 'P003_2025', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 18000.00, 28000.00, 1200.00, 4500.00, 1900.00, 'Completed', DATE '2025-03-05', 1350.00, 16750.00, 55000.00, 0.1200, 250.00,  750.00, 'FPS', 'GEN', 'C003', 1, 'Baseline archive row',  600.00, 0, 1003.0, 'ORA-P003-25', 'SUB_C', 'GROUP_C', 'INC_C'),
    (2026, 'P001_2026', 'PROG_A', 'CUSTOMER_A', 'John Doe',    16000.00, 26000.00, 1100.00, 5100.00, 2100.00, 'Active',    DATE '2026-01-20', 1250.00, 19100.00, 52000.00, 0.1000,   0.00,  550.00, 'FPS', 'GEN', 'C001', 0, 'Baseline archive row',  520.00, 0, 1001.0, 'ORA-P001-26', 'SUB_A', 'GROUP_A', 'INC_A'),
    (2026, 'P002_2026', 'PROG_B', 'CUSTOMER_B', 'Jane Smith',  21000.00, 31000.00, 1600.00, 6100.00, 2550.00, 'Active',    DATE '2026-02-12', 1450.00, 20600.00, 62000.00, 0.1500, 550.00, 1100.00, 'FPS', 'GEN', 'C002', 0, 'Baseline archive row',  780.00, 0, 1002.0, 'ORA-P002-26', 'SUB_B', 'GROUP_B', 'INC_B'),
    (2026, 'P003_2026', 'PROG_C', 'CUSTOMER_C', 'Bob Johnson', 19000.00, 29000.00, 1250.00, 4600.00, 1950.00, 'Active',    DATE '2026-03-07', 1380.00, 17500.00, 57000.00, 0.1200, 300.00,  800.00, 'FPS', 'GEN', 'C003', 0, 'Baseline archive row',  630.00, 0, 1003.0, 'ORA-P003-26', 'SUB_C', 'GROUP_C', 'INC_C')
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

INSERT INTO mabarchive.my_tlkpproject (
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
SELECT
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
    datecreated::timestamp,
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
FROM mabarchive.my_tlkpproject_all
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

INSERT INTO mabarchive.my_tlkpprogram (
    year,
    programno,
    programname,
    directorate,
    minim,
    sector_name,
    customer,
    target,
    manager
)
VALUES
    (2025, 'PROG_A', 'Programme A 2025', 'DIR_A', 'MIN_A', 'Sector A', 'CUSTOMER_A', 40000.00, 'John Doe'),
    (2025, 'PROG_B', 'Programme B 2025', 'DIR_B', 'MIN_B', 'Sector B', 'CUSTOMER_B', 50000.00, 'Jane Smith'),
    (2025, 'PROG_C', 'Programme C 2025', 'DIR_C', 'MIN_C', 'Sector C', 'CUSTOMER_C', 46000.00, 'Bob Johnson'),
    (2026, 'PROG_A', 'Programme A 2026', 'DIR_A', 'MIN_A', 'Sector A', 'CUSTOMER_A', 42000.00, 'John Doe'),
    (2026, 'PROG_B', 'Programme B 2026', 'DIR_B', 'MIN_B', 'Sector B', 'CUSTOMER_B', 52000.00, 'Jane Smith'),
    (2026, 'PROG_C', 'Programme C 2026', 'DIR_C', 'MIN_C', 'Sector C', 'CUSTOMER_C', 48000.00, 'Bob Johnson')
ON CONFLICT (year, programno)
DO UPDATE SET
    programname = EXCLUDED.programname,
    directorate = EXCLUDED.directorate,
    minim = EXCLUDED.minim,
    sector_name = EXCLUDED.sector_name,
    customer = EXCLUDED.customer,
    target = EXCLUDED.target,
    manager = EXCLUDED.manager;

INSERT INTO mabarchive.g_tlkpproject (
    parentproject,
    projecttitle,
    costbookno,
    disease,
    contract,
    shorttitle,
    projectstatus
)
VALUES
    ('P001_2025', 'Project 001 FY2025', 'CB001', 'GEN', 'C001', 'P001-25', 'Active'),
    ('P002_2025', 'Project 002 FY2025', 'CB002', 'GEN', 'C002', 'P002-25', 'Active'),
    ('P003_2025', 'Project 003 FY2025', 'CB003', 'GEN', 'C003', 'P003-25', 'Completed'),
    ('P001_2026', 'Project 001 FY2026', 'CB004', 'GEN', 'C001', 'P001-26', 'Active'),
    ('P002_2026', 'Project 002 FY2026', 'CB005', 'GEN', 'C002', 'P002-26', 'Active'),
    ('P003_2026', 'Project 003 FY2026', 'CB006', 'GEN', 'C003', 'P003-26', 'Active')
ON CONFLICT (parentproject)
DO UPDATE SET
    projecttitle = EXCLUDED.projecttitle,
    costbookno = EXCLUDED.costbookno,
    disease = EXCLUDED.disease,
    contract = EXCLUDED.contract,
    shorttitle = EXCLUDED.shorttitle,
    projectstatus = EXCLUDED.projectstatus;

INSERT INTO mabarchive.tlkpyear (year, latestmonthreleased)
VALUES
    (2025, 12),
    (2026, 4)
ON CONFLICT (year)
DO UPDATE SET
    latestmonthreleased = EXCLUDED.latestmonthreleased;

INSERT INTO mabarchive.my_tblcontract (
    year,
    contractno,
    category,
    manager,
    customer,
    title,
    registereddate,
    startdate,
    enddate,
    duration
)
VALUES
    (2025, 'C001', 'Research', 'John Doe',   'CUSTOMER_A', 'Contract A 2025', DATE '2025-01-01', DATE '2025-01-01', DATE '2025-12-31', 12),
    (2025, 'C002', 'Research', 'Jane Smith', 'CUSTOMER_B', 'Contract B 2025', DATE '2025-01-01', DATE '2025-01-01', DATE '2025-12-31', 12),
    (2025, 'C003', 'Delivery', 'Bob Johnson','CUSTOMER_C', 'Contract C 2025', DATE '2025-01-01', DATE '2025-01-01', DATE '2025-12-31', 12),
    (2026, 'C001', 'Research', 'John Doe',   'CUSTOMER_A', 'Contract A 2026', DATE '2026-01-01', DATE '2026-01-01', DATE '2026-12-31', 12),
    (2026, 'C002', 'Research', 'Jane Smith', 'CUSTOMER_B', 'Contract B 2026', DATE '2026-01-01', DATE '2026-01-01', DATE '2026-12-31', 12),
    (2026, 'C003', 'Delivery', 'Bob Johnson','CUSTOMER_C', 'Contract C 2026', DATE '2026-01-01', DATE '2026-01-01', DATE '2026-12-31', 12)
ON CONFLICT (year, contractno)
DO UPDATE SET
    category = EXCLUDED.category,
    manager = EXCLUDED.manager,
    customer = EXCLUDED.customer,
    title = EXCLUDED.title,
    registereddate = EXCLUDED.registereddate,
    startdate = EXCLUDED.startdate,
    enddate = EXCLUDED.enddate,
    duration = EXCLUDED.duration;

INSERT INTO mabarchive.my_tblprofitcentre (
    year,
    profitcentre,
    profitcentrename,
    division,
    conttarget,
    profitcentrehead,
    divisionid
)
VALUES
    (2025, 'PC_A', 'Profit Centre A', 'DIV_A', 40000.00, 'John Doe',   1),
    (2025, 'PC_B', 'Profit Centre B', 'DIV_B', 50000.00, 'Jane Smith', 2),
    (2025, 'PC_C', 'Profit Centre C', 'DIV_C', 46000.00, 'Bob Johnson',3),
    (2026, 'PC_A', 'Profit Centre A', 'DIV_A', 42000.00, 'John Doe',   1),
    (2026, 'PC_B', 'Profit Centre B', 'DIV_B', 52000.00, 'Jane Smith', 2),
    (2026, 'PC_C', 'Profit Centre C', 'DIV_C', 48000.00, 'Bob Johnson',3)
ON CONFLICT (year, profitcentre)
DO UPDATE SET
    profitcentrename = EXCLUDED.profitcentrename,
    division = EXCLUDED.division,
    conttarget = EXCLUDED.conttarget,
    profitcentrehead = EXCLUDED.profitcentrehead,
    divisionid = EXCLUDED.divisionid;

INSERT INTO mabarchive.my_profitcentregrade (
    year,
    pcgrade,
    divisiongrade,
    gradecode,
    profitcentre,
    chargerate,
    directrate,
    payrate,
    npr,
    ohr
)
VALUES
    (2025, 'PCG_A', 'DIVG_A', 'G7', 'PC_A', 120.00, 90.00, 70.00, 10.00, 20.00),
    (2025, 'PCG_B', 'DIVG_B', 'G7', 'PC_B', 125.00, 95.00, 72.00, 11.00, 21.00),
    (2025, 'PCG_C', 'DIVG_C', 'G7', 'PC_C', 130.00, 98.00, 74.00, 12.00, 22.00),
    (2026, 'PCG_A', 'DIVG_A', 'G7', 'PC_A', 122.00, 92.00, 71.00, 10.50, 20.50),
    (2026, 'PCG_B', 'DIVG_B', 'G7', 'PC_B', 127.00, 97.00, 73.00, 11.50, 21.50),
    (2026, 'PCG_C', 'DIVG_C', 'G7', 'PC_C', 132.00, 99.00, 75.00, 12.50, 22.50)
ON CONFLICT (year, pcgrade)
DO UPDATE SET
    divisiongrade = EXCLUDED.divisiongrade,
    gradecode = EXCLUDED.gradecode,
    profitcentre = EXCLUDED.profitcentre,
    chargerate = EXCLUDED.chargerate,
    directrate = EXCLUDED.directrate,
    payrate = EXCLUDED.payrate,
    npr = EXCLUDED.npr,
    ohr = EXCLUDED.ohr;

INSERT INTO mabarchive.my_workgroup (
    year,
    workgroup,
    profitcentre,
    costcentre,
    owner,
    description,
    centraloverhead,
    sendemail,
    cos90,
    costcentreold,
    email_recipient
)
VALUES
    (2025, 'WG_A', 'PC_A', 1001.0, 'John Doe',   'Workgroup A', 1000.00, 1, 0, 2001.0, 'wg_a@example.test'),
    (2025, 'WG_B', 'PC_B', 1002.0, 'Jane Smith', 'Workgroup B', 1100.00, 1, 0, 2002.0, 'wg_b@example.test'),
    (2025, 'WG_C', 'PC_C', 1003.0, 'Bob Johnson','Workgroup C', 1200.00, 1, 0, 2003.0, 'wg_c@example.test'),
    (2026, 'WG_A', 'PC_A', 1001.0, 'John Doe',   'Workgroup A', 1020.00, 1, 0, 2001.0, 'wg_a@example.test'),
    (2026, 'WG_B', 'PC_B', 1002.0, 'Jane Smith', 'Workgroup B', 1120.00, 1, 0, 2002.0, 'wg_b@example.test'),
    (2026, 'WG_C', 'PC_C', 1003.0, 'Bob Johnson','Workgroup C', 1220.00, 1, 0, 2003.0, 'wg_c@example.test')
ON CONFLICT (year, workgroup)
DO UPDATE SET
    profitcentre = EXCLUDED.profitcentre,
    costcentre = EXCLUDED.costcentre,
    owner = EXCLUDED.owner,
    description = EXCLUDED.description,
    centraloverhead = EXCLUDED.centraloverhead,
    sendemail = EXCLUDED.sendemail,
    cos90 = EXCLUDED.cos90,
    costcentreold = EXCLUDED.costcentreold,
    email_recipient = EXCLUDED.email_recipient;

INSERT INTO mabarchive.my_workgroupgrade (
    year,
    wggrade,
    profitcentregrade,
    gradecode,
    workgroup
)
VALUES
    (2025, 'WGG_A', 'PCG_A', 'G7', 'WG_A'),
    (2025, 'WGG_B', 'PCG_B', 'G7', 'WG_B'),
    (2025, 'WGG_C', 'PCG_C', 'G7', 'WG_C'),
    (2026, 'WGG_A', 'PCG_A', 'G7', 'WG_A'),
    (2026, 'WGG_B', 'PCG_B', 'G7', 'WG_B'),
    (2026, 'WGG_C', 'PCG_C', 'G7', 'WG_C')
ON CONFLICT (year, wggrade)
DO UPDATE SET
    profitcentregrade = EXCLUDED.profitcentregrade,
    gradecode = EXCLUDED.gradecode,
    workgroup = EXCLUDED.workgroup;

INSERT INTO mabarchive.my_staff (
    year,
    staffid,
    workgroupgrade,
    name,
    title,
    personstatus,
    personclass,
    hrspaid,
    leave,
    sickspecial,
    hrsavail
)
VALUES
    (2025, 'ST001', 'WGG_A', 'Alice Analyst', 'Dr', 'ACTIVE', 'SCI', 1600.0, 120.0,  8.0, 1472.0),
    (2025, 'ST002', 'WGG_B', 'Ben Biologist', 'Mr', 'ACTIVE', 'SCI', 1600.0, 110.0, 12.0, 1478.0),
    (2025, 'ST003', 'WGG_C', 'Cara Chemist',  'Ms', 'ACTIVE', 'SCI', 1600.0, 100.0, 10.0, 1490.0),
    (2026, 'ST001', 'WGG_A', 'Alice Analyst', 'Dr', 'ACTIVE', 'SCI', 1600.0, 120.0,  8.0, 1472.0),
    (2026, 'ST002', 'WGG_B', 'Ben Biologist', 'Mr', 'ACTIVE', 'SCI', 1600.0, 110.0, 12.0, 1478.0),
    (2026, 'ST003', 'WGG_C', 'Cara Chemist',  'Ms', 'ACTIVE', 'SCI', 1600.0, 100.0, 10.0, 1490.0)
ON CONFLICT (year, staffid)
DO UPDATE SET
    workgroupgrade = EXCLUDED.workgroupgrade,
    name = EXCLUDED.name,
    title = EXCLUDED.title,
    personstatus = EXCLUDED.personstatus,
    personclass = EXCLUDED.personclass,
    hrspaid = EXCLUDED.hrspaid,
    leave = EXCLUDED.leave,
    sickspecial = EXCLUDED.sickspecial,
    hrsavail = EXCLUDED.hrsavail;

INSERT INTO mabarchive.my_tblstaffjob (
    year,
    staffid,
    jobcode,
    plannedhours,
    systimestamp
)
VALUES
    (2025, 'ST001', 'P001_2025', 420.0, decode('00', 'hex')),
    (2025, 'ST002', 'P002_2025', 430.0, decode('00', 'hex')),
    (2025, 'ST003', 'P003_2025', 410.0, decode('00', 'hex')),
    (2026, 'ST001', 'P001_2026', 425.0, decode('00', 'hex')),
    (2026, 'ST002', 'P002_2026', 435.0, decode('00', 'hex')),
    (2026, 'ST003', 'P003_2026', 415.0, decode('00', 'hex'))
ON CONFLICT (year, staffid, jobcode)
DO UPDATE SET
    plannedhours = EXCLUDED.plannedhours,
    systimestamp = EXCLUDED.systimestamp;

INSERT INTO mabarchive.my_tbladditionalcosts (
    year,
    jobcode,
    account,
    description,
    itemcost,
    freq,
    supplier,
    ac_counter
)
VALUES
    (2025, 'P001_2025', 'ACC_A', 'ADD_COST_A', 1000.00, 'MTH', 'SUPPLIER_A', 1001),
    (2025, 'P002_2025', 'ACC_B', 'ADD_COST_B', 2000.00, 'MTH', 'SUPPLIER_B', 1002),
    (2025, 'P003_2025', 'ACC_C', 'ADD_COST_C', 1500.00, 'MTH', 'SUPPLIER_C', 1003),
    (2026, 'P001_2026', 'ACC_A', 'ADD_COST_A', 1100.00, 'MTH', 'SUPPLIER_A', 1004),
    (2026, 'P002_2026', 'ACC_B', 'ADD_COST_B', 2100.00, 'MTH', 'SUPPLIER_B', 1005),
    (2026, 'P003_2026', 'ACC_C', 'ADD_COST_C', 1600.00, 'MTH', 'SUPPLIER_C', 1006)
ON CONFLICT (ac_counter)
DO UPDATE SET
    year = EXCLUDED.year,
    jobcode = EXCLUDED.jobcode,
    account = EXCLUDED.account,
    description = EXCLUDED.description,
    itemcost = EXCLUDED.itemcost,
    freq = EXCLUDED.freq,
    supplier = EXCLUDED.supplier;

INSERT INTO mabarchive.my_tblanimals (
    year,
    animaltype,
    species,
    security_level,
    dailyrate,
    planbyweek,
    defradailyrate
)
VALUES
    (2025, 'MOUSE',  'Mouse',  'LOW',    50.00, TRUE,  45.00),
    (2025, 'RAT',    'Rat',    'MEDIUM', 60.00, TRUE,  55.00),
    (2025, 'RABBIT', 'Rabbit', 'LOW',    75.00, FALSE, 70.00),
    (2026, 'MOUSE',  'Mouse',  'LOW',    52.00, TRUE,  46.00),
    (2026, 'RAT',    'Rat',    'MEDIUM', 62.00, TRUE,  56.00),
    (2026, 'RABBIT', 'Rabbit', 'LOW',    77.00, FALSE, 71.00)
ON CONFLICT (year, animaltype)
DO UPDATE SET
    species = EXCLUDED.species,
    security_level = EXCLUDED.security_level,
    dailyrate = EXCLUDED.dailyrate,
    planbyweek = EXCLUDED.planbyweek,
    defradailyrate = EXCLUDED.defradailyrate;

INSERT INTO mabarchive.my_tblanimalreq (
    year,
    jobcode,
    animaltype,
    numberofdays,
    numberofanimals,
    ar_counter
)
VALUES
    (2025, 'P001_2025', 'MOUSE',  10.0, 20.0, 2001),
    (2025, 'P002_2025', 'RAT',    12.0, 25.0, 2002),
    (2025, 'P003_2025', 'RABBIT',  8.0, 15.0, 2003),
    (2026, 'P001_2026', 'MOUSE',  11.0, 21.0, 2004),
    (2026, 'P002_2026', 'RAT',    13.0, 26.0, 2005),
    (2026, 'P003_2026', 'RABBIT',  9.0, 16.0, 2006)
ON CONFLICT (ar_counter)
DO UPDATE SET
    year = EXCLUDED.year,
    jobcode = EXCLUDED.jobcode,
    animaltype = EXCLUDED.animaltype,
    numberofdays = EXCLUDED.numberofdays,
    numberofanimals = EXCLUDED.numberofanimals;

INSERT INTO mabarchive.my_testorproduct (
    year,
    itemcode,
    itemdescription,
    testmanager,
    jobstatus,
    unitpricevla,
    priceahvg,
    owner,
    chargemethod,
    shortdescription,
    defraunitprice
)
VALUES
    (2025, 'TEST_A', 'Test Product A', 'John Doe',   'AC', 100.00,  90.00, 'A1', 'STD', 'TEST A',  80.00),
    (2025, 'TEST_B', 'Test Product B', 'Jane Smith', 'AC', 120.00, 110.00, 'B1', 'STD', 'TEST B', 100.00),
    (2025, 'TEST_C', 'Test Product C', 'Bob Johnson','CP', 140.00, 130.00, 'C1', 'STD', 'TEST C', 120.00),
    (2026, 'TEST_A', 'Test Product A', 'John Doe',   'AC', 105.00,  95.00, 'A1', 'STD', 'TEST A',  82.00),
    (2026, 'TEST_B', 'Test Product B', 'Jane Smith', 'AC', 125.00, 115.00, 'B1', 'STD', 'TEST B', 102.00),
    (2026, 'TEST_C', 'Test Product C', 'Bob Johnson','CP', 145.00, 135.00, 'C1', 'STD', 'TEST C', 122.00)
ON CONFLICT (year, itemcode)
DO UPDATE SET
    itemdescription = EXCLUDED.itemdescription,
    testmanager = EXCLUDED.testmanager,
    jobstatus = EXCLUDED.jobstatus,
    unitpricevla = EXCLUDED.unitpricevla,
    priceahvg = EXCLUDED.priceahvg,
    owner = EXCLUDED.owner,
    chargemethod = EXCLUDED.chargemethod,
    shortdescription = EXCLUDED.shortdescription,
    defraunitprice = EXCLUDED.defraunitprice;

INSERT INTO mabarchive.my_tlkptestreqmt (
    year,
    testcode,
    buyer,
    unitprice,
    norequired,
    projectbuyercode,
    testbuyercode,
    source
)
VALUES
    (2025, 'TEST_A', 'BUYER_A', 100.00, 5.0, 'P001_2025-B', 'TEST_A-B', 'FPS'),
    (2025, 'TEST_B', 'BUYER_B', 120.00, 6.0, 'P002_2025-B', 'TEST_B-B', 'FPS'),
    (2025, 'TEST_C', 'BUYER_C', 140.00, 4.0, 'P003_2025-B', 'TEST_C-B', 'FPS'),
    (2026, 'TEST_A', 'BUYER_A', 105.00, 5.0, 'P001_2026-B', 'TEST_A-B', 'FPS'),
    (2026, 'TEST_B', 'BUYER_B', 125.00, 6.0, 'P002_2026-B', 'TEST_B-B', 'FPS'),
    (2026, 'TEST_C', 'BUYER_C', 145.00, 4.0, 'P003_2026-B', 'TEST_C-B', 'FPS')
ON CONFLICT (year, testcode, buyer)
DO UPDATE SET
    unitprice = EXCLUDED.unitprice,
    norequired = EXCLUDED.norequired,
    projectbuyercode = EXCLUDED.projectbuyercode,
    testbuyercode = EXCLUDED.testbuyercode,
    source = EXCLUDED.source;

INSERT INTO mabarchive.my_monthlyoutput (
    year,
    testcode,
    buyer,
    month,
    workgroup,
    volume,
    wgbuyer
)
VALUES
    (2025, 'TEST_A', 'BUYER_A', 1, 'WG_A', 10.0, 'WG_A_BUYER_A'),
    (2025, 'TEST_B', 'BUYER_B', 1, 'WG_B', 12.0, 'WG_B_BUYER_B'),
    (2025, 'TEST_C', 'BUYER_C', 1, 'WG_C',  8.0, 'WG_C_BUYER_C'),
    (2026, 'TEST_A', 'BUYER_A', 1, 'WG_A', 11.0, 'WG_A_BUYER_A'),
    (2026, 'TEST_B', 'BUYER_B', 1, 'WG_B', 13.0, 'WG_B_BUYER_B'),
    (2026, 'TEST_C', 'BUYER_C', 1, 'WG_C',  9.0, 'WG_C_BUYER_C')
ON CONFLICT (year, testcode, buyer, month, workgroup)
DO UPDATE SET
    volume = EXCLUDED.volume,
    wgbuyer = EXCLUDED.wgbuyer;

INSERT INTO mabarchive.my_monthlytime (
    year,
    pactstaffid,
    timecode,
    month,
    parentproject,
    workgroup,
    hours
)
VALUES
    (2025, 'ST001', 'LAB', 1, 'P001_2025', 'WG_A', 120.0),
    (2025, 'ST002', 'LAB', 1, 'P002_2025', 'WG_B', 130.0),
    (2025, 'ST003', 'LAB', 1, 'P003_2025', 'WG_C', 110.0),
    (2026, 'ST001', 'LAB', 1, 'P001_2026', 'WG_A', 122.0),
    (2026, 'ST002', 'LAB', 1, 'P002_2026', 'WG_B', 132.0),
    (2026, 'ST003', 'LAB', 1, 'P003_2026', 'WG_C', 112.0)
ON CONFLICT (year, pactstaffid, timecode, month, parentproject)
DO UPDATE SET
    workgroup = EXCLUDED.workgroup,
    hours = EXCLUDED.hours;

INSERT INTO mabarchive.my_timecostcalcs (
    year,
    workgroup,
    jobcode,
    project,
    month,
    staffid,
    gradecode,
    name,
    chargerate,
    class,
    time,
    cost,
    division,
    jobcodeold,
    pay,
    nonpay,
    overhead
)
VALUES
    (2025, 'WG_A', 'P001_2025', 'P001_2025', 1, 'ST001', 'G7', 'Alice Analyst', 120.00, 'SCI', 120.0, 14400.0, 'DIV_A', 'OLD_P001_25',  8400.00, 3000.00, 3000.00),
    (2025, 'WG_B', 'P002_2025', 'P002_2025', 1, 'ST002', 'G7', 'Ben Biologist', 125.00, 'SCI', 130.0, 16250.0, 'DIV_B', 'OLD_P002_25',  9360.00, 3445.00, 3445.00),
    (2025, 'WG_C', 'P003_2025', 'P003_2025', 1, 'ST003', 'G7', 'Cara Chemist',  130.00, 'SCI', 110.0, 14300.0, 'DIV_C', 'OLD_P003_25',  8140.00, 3080.00, 3080.00),
    (2026, 'WG_A', 'P001_2026', 'P001_2026', 1, 'ST001', 'G7', 'Alice Analyst', 122.00, 'SCI', 122.0, 14884.0, 'DIV_A', 'OLD_P001_26',  8662.00, 3111.00, 3111.00),
    (2026, 'WG_B', 'P002_2026', 'P002_2026', 1, 'ST002', 'G7', 'Ben Biologist', 127.00, 'SCI', 132.0, 16764.0, 'DIV_B', 'OLD_P002_26',  9636.00, 3564.00, 3564.00),
    (2026, 'WG_C', 'P003_2026', 'P003_2026', 1, 'ST003', 'G7', 'Cara Chemist',  132.00, 'SCI', 112.0, 14784.0, 'DIV_C', 'OLD_P003_26',  8400.00, 3192.00, 3192.00)
ON CONFLICT (year, workgroup, jobcode, project, month, staffid)
DO UPDATE SET
    gradecode = EXCLUDED.gradecode,
    name = EXCLUDED.name,
    chargerate = EXCLUDED.chargerate,
    class = EXCLUDED.class,
    time = EXCLUDED.time,
    cost = EXCLUDED.cost,
    division = EXCLUDED.division,
    jobcodeold = EXCLUDED.jobcodeold,
    pay = EXCLUDED.pay,
    nonpay = EXCLUDED.nonpay,
    overhead = EXCLUDED.overhead;

INSERT INTO mabarchive.my_projectmonthfinal (
    year,
    project,
    monthno,
    periodname,
    cumflag,
    costprofile,
    subcontracts,
    animals,
    nonanimals,
    timecosts,
    transfercosts,
    totalcost,
    invoices,
    coiw,
    portsales,
    cumcost,
    cumprofile,
    sumofcostprofile,
    cuminvoices,
    cumcoiw,
    cumportsales,
    mstonedue,
    due__done,
    ontime,
    sumofmstonedue,
    sumofdue__done,
    sumofontime,
    cwdebit,
    cwcredit,
    cumcwdebit,
    cumcwcredit,
    totalhours,
    cumtotalhours,
    cumsubcontracts,
    cumtestcosts,
    paycosts,
    cumpaycosts
)
VALUES
    (2025, 'P001_2025', 1, 'Jan-2025', 0, 2000.00,  500.00, 1000.00,  500.00, 12000.00, 1000.00, 17000.00, 25000.00, 0.00, 15000.00, 17000.00, 2000.00, 2000.00, 25000.00, 0.00, 15000.00, 1, 1, 1, 1, 1, 1,  500.00, 100.00,  500.00, 100.00, 120.0, 120.0,  500.0, 3000.0, 12000.0, 12000.0),
    (2025, 'P002_2025', 1, 'Jan-2025', 0, 2500.00,  800.00, 1200.00,  600.00, 15000.00, 1200.00, 21300.00, 30000.00, 0.00, 20000.00, 21300.00, 2500.00, 2500.00, 30000.00, 0.00, 20000.00, 1, 1, 1, 1, 1, 1, 1000.00, 150.00, 1000.00, 150.00, 130.0, 130.0,  800.0, 4000.0, 15000.0, 15000.0),
    (2025, 'P003_2025', 1, 'Jan-2025', 0, 2200.00,  600.00,  900.00,  550.00, 14000.00, 1100.00, 19350.00, 28000.00, 0.00, 18000.00, 19350.00, 2200.00, 2200.00, 28000.00, 0.00, 18000.00, 1, 1, 1, 1, 1, 1,  750.00, 125.00,  750.00, 125.00, 110.0, 110.0,  600.0, 3500.0, 14000.0, 14000.0),
    (2026, 'P001_2026', 1, 'Jan-2026', 0, 2100.00,  550.00, 1050.00,  520.00, 12500.00, 1050.00, 17770.00, 26000.00, 0.00, 16000.00, 17770.00, 2100.00, 2100.00, 26000.00, 0.00, 16000.00, 1, 1, 1, 1, 1, 1,  550.00, 110.00,  550.00, 110.00, 122.0, 122.0,  550.0, 3200.0, 12500.0, 12500.0),
    (2026, 'P002_2026', 1, 'Jan-2026', 0, 2600.00,  850.00, 1250.00,  650.00, 15500.00, 1250.00, 22100.00, 31000.00, 0.00, 21000.00, 22100.00, 2600.00, 2600.00, 31000.00, 0.00, 21000.00, 1, 1, 1, 1, 1, 1, 1100.00, 160.00, 1100.00, 160.00, 132.0, 132.0,  850.0, 4200.0, 15500.0, 15500.0),
    (2026, 'P003_2026', 1, 'Jan-2026', 0, 2300.00,  650.00,  950.00,  580.00, 14500.00, 1150.00, 20130.00, 29000.00, 0.00, 19000.00, 20130.00, 2300.00, 2300.00, 29000.00, 0.00, 19000.00, 1, 1, 1, 1, 1, 1,  800.00, 130.00,  800.00, 130.00, 112.0, 112.0,  650.0, 3700.0, 14500.0, 14500.0)
ON CONFLICT (year, project, monthno)
DO UPDATE SET
    periodname = EXCLUDED.periodname,
    cumflag = EXCLUDED.cumflag,
    costprofile = EXCLUDED.costprofile,
    subcontracts = EXCLUDED.subcontracts,
    animals = EXCLUDED.animals,
    nonanimals = EXCLUDED.nonanimals,
    timecosts = EXCLUDED.timecosts,
    transfercosts = EXCLUDED.transfercosts,
    totalcost = EXCLUDED.totalcost,
    invoices = EXCLUDED.invoices,
    coiw = EXCLUDED.coiw,
    portsales = EXCLUDED.portsales,
    cumcost = EXCLUDED.cumcost,
    cumprofile = EXCLUDED.cumprofile,
    sumofcostprofile = EXCLUDED.sumofcostprofile,
    cuminvoices = EXCLUDED.cuminvoices,
    cumcoiw = EXCLUDED.cumcoiw,
    cumportsales = EXCLUDED.cumportsales,
    mstonedue = EXCLUDED.mstonedue,
    due__done = EXCLUDED.due__done,
    ontime = EXCLUDED.ontime,
    sumofmstonedue = EXCLUDED.sumofmstonedue,
    sumofdue__done = EXCLUDED.sumofdue__done,
    sumofontime = EXCLUDED.sumofontime,
    cwdebit = EXCLUDED.cwdebit,
    cwcredit = EXCLUDED.cwcredit,
    cumcwdebit = EXCLUDED.cumcwdebit,
    cumcwcredit = EXCLUDED.cumcwcredit,
    totalhours = EXCLUDED.totalhours,
    cumtotalhours = EXCLUDED.cumtotalhours,
    cumsubcontracts = EXCLUDED.cumsubcontracts,
    cumtestcosts = EXCLUDED.cumtestcosts,
    paycosts = EXCLUDED.paycosts,
    cumpaycosts = EXCLUDED.cumpaycosts;

INSERT INTO mabarchive.my_proj_invoice (
    year,
    projectparent,
    month,
    amount,
    costofwork,
    wip,
    profitloss,
    detail,
    invoicecounter,
    type
)
VALUES
    (2025, 'P001_2025', 1, 25000.00, 17000.00, 8000.00,  8000.00, 'Invoice Jan 2025', 3001, 'STD'),
    (2025, 'P002_2025', 1, 30000.00, 21300.00, 8700.00,  8700.00, 'Invoice Jan 2025', 3002, 'STD'),
    (2025, 'P003_2025', 1, 28000.00, 19350.00, 8650.00,  8650.00, 'Invoice Jan 2025', 3003, 'STD'),
    (2026, 'P001_2026', 1, 26000.00, 17770.00, 8230.00,  8230.00, 'Invoice Jan 2026', 3004, 'STD'),
    (2026, 'P002_2026', 1, 31000.00, 22100.00, 8900.00,  8900.00, 'Invoice Jan 2026', 3005, 'STD'),
    (2026, 'P003_2026', 1, 29000.00, 20130.00, 8870.00,  8870.00, 'Invoice Jan 2026', 3006, 'STD')
ON CONFLICT (year, projectparent, invoicecounter)
DO UPDATE SET
    month = EXCLUDED.month,
    amount = EXCLUDED.amount,
    costofwork = EXCLUDED.costofwork,
    wip = EXCLUDED.wip,
    profitloss = EXCLUDED.profitloss,
    detail = EXCLUDED.detail,
    type = EXCLUDED.type;

INSERT INTO mabarchive.my_proj_subcontract (
    year,
    subcontcounter,
    project,
    testjob,
    month,
    amount,
    workgroup,
    acctcode,
    supplier,
    description,
    suppliernumber,
    dailyrate,
    animaldays
)
VALUES
    (2025, 4001, 'P001_2025', 'TEST_A', 1, 500.00, 'WG_A', 'SUB_ACC_A', 'SUPPLIER_A', 'Subcontract A', 1,  50.00, 10),
    (2025, 4002, 'P002_2025', 'TEST_B', 1, 800.00, 'WG_B', 'SUB_ACC_B', 'SUPPLIER_B', 'Subcontract B', 2,  60.00, 12),
    (2025, 4003, 'P003_2025', 'TEST_C', 1, 600.00, 'WG_C', 'SUB_ACC_C', 'SUPPLIER_C', 'Subcontract C', 3,  75.00,  8),
    (2026, 4004, 'P001_2026', 'TEST_A', 1, 550.00, 'WG_A', 'SUB_ACC_A', 'SUPPLIER_A', 'Subcontract A', 1,  52.00, 11),
    (2026, 4005, 'P002_2026', 'TEST_B', 1, 850.00, 'WG_B', 'SUB_ACC_B', 'SUPPLIER_B', 'Subcontract B', 2,  62.00, 13),
    (2026, 4006, 'P003_2026', 'TEST_C', 1, 650.00, 'WG_C', 'SUB_ACC_C', 'SUPPLIER_C', 'Subcontract C', 3,  77.00,  9)
ON CONFLICT (year, subcontcounter)
DO UPDATE SET
    project = EXCLUDED.project,
    testjob = EXCLUDED.testjob,
    month = EXCLUDED.month,
    amount = EXCLUDED.amount,
    workgroup = EXCLUDED.workgroup,
    acctcode = EXCLUDED.acctcode,
    supplier = EXCLUDED.supplier,
    description = EXCLUDED.description,
    suppliernumber = EXCLUDED.suppliernumber,
    dailyrate = EXCLUDED.dailyrate,
    animaldays = EXCLUDED.animaldays;

SELECT setval(
    'mabarchive."my_tblanimalreq_AR_Counter_seq"',
    GREATEST((SELECT COALESCE(MAX(ar_counter), 1) FROM mabarchive.my_tblanimalreq), 1),
    TRUE
);

COMMIT;
