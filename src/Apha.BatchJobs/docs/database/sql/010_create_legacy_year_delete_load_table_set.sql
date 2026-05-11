-- Migration 010: Provision broader legacy year delete/load table set in current Postgres.
-- Scope: mabarchive MY_*, G_*, and tlkpyear tables required by ScheduledLoadFromFps orchestration.
-- Source of truth: dbscript/schemas/02mabarchive/01tables/*.sql (cloud DDL).
-- Safe to re-run via IF NOT EXISTS wrappers.

BEGIN;

CREATE SCHEMA IF NOT EXISTS mabarchive;

-- Sequence dependency used by mabarchive.my_tblanimalreq default expression.
CREATE SEQUENCE IF NOT EXISTS mabarchive."my_tblanimalreq_AR_Counter_seq"
    AS integer
    INCREMENT BY 1
    MINVALUE 1
    MAXVALUE 2147483647
    START WITH 1
    CACHE 1
    NO CYCLE;


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_fpsyeartotals.sql
-- Table: mabarchive.my_fpsyeartotals

CREATE TABLE IF NOT EXISTS mabarchive.my_fpsyeartotals (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    program character varying(10) NOT NULL,
    totaladditionalcosts money,
    totalanimalcosts double precision,
    totalstaffcosts double precision,
    totaltestcosts double precision,
    totalcosts double precision,
    custincome money NOT NULL,
    transferincome money NOT NULL,
    totalincome money NOT NULL,
    budget_cvl money,
    requiredprofit money,
    manager character varying(50),
    customer character varying(50),
    projectstatus character varying(50) NOT NULL,
    pvsincome money,
    plancaseworkdebit money,
    totalpaycosts double precision,
    CONSTRAINT pk_my_fpsyeartotals PRIMARY KEY (year, parentproject)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tlkpproject_all.sql
-- Table: mabarchive.my_tlkpproject_all

CREATE TABLE IF NOT EXISTS mabarchive.my_tlkpproject_all (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    program character varying(10),
    customer character varying(50),
    manager character varying(50),
    transferincome money,
    custincome money,
    wip_eoy money,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50),
    datecreated date,
    feccost money,
    profit money,
    budget_cvl money,
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    source character(5),
    disease character varying(50),
    contract character varying(10),
    finished smallint,
    comments text,
    carryover money,
    isdefraproject smallint,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50),
    CONSTRAINT pk_my_tlkpproject_all PRIMARY KEY (year, parentproject)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_monthlyoutput.sql
-- Table: mabarchive.my_monthlyoutput

CREATE TABLE IF NOT EXISTS mabarchive.my_monthlyoutput (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    month double precision NOT NULL,
    workgroup character varying(50) NOT NULL,
    volume double precision,
    wgbuyer character varying(50),
    CONSTRAINT pk_my_monthlyoutput PRIMARY KEY (year, testcode, buyer, month, workgroup)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_monthlytime.sql
-- Table: mabarchive.my_monthlytime

CREATE TABLE IF NOT EXISTS mabarchive.my_monthlytime (
    year smallint NOT NULL,
    pactstaffid character varying(50) NOT NULL,
    timecode character varying(50) NOT NULL,
    month double precision NOT NULL,
    parentproject character varying(20) NOT NULL,
    workgroup character varying(50),
    hours double precision,
    CONSTRAINT pk_my_monthlytime PRIMARY KEY (year, pactstaffid, timecode, month, parentproject)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_projectmonthfinal.sql
-- Table: mabarchive.my_projectmonthfinal

CREATE TABLE IF NOT EXISTS mabarchive.my_projectmonthfinal (
    year smallint NOT NULL,
    project character varying(20) NOT NULL,
    monthno double precision NOT NULL,
    periodname character varying(50),
    cumflag double precision,
    costprofile money,
    subcontracts money,
    animals money,
    nonanimals money,
    timecosts money,
    transfercosts money,
    totalcost money,
    invoices money,
    coiw money,
    portsales money,
    cumcost money,
    cumprofile money,
    sumofcostprofile money,
    cuminvoices money,
    cumcoiw money,
    cumportsales money,
    mstonedue integer,
    due__done double precision,
    ontime double precision,
    sumofmstonedue double precision,
    sumofdue__done double precision,
    sumofontime double precision,
    cwdebit money,
    cwcredit money,
    cumcwdebit money,
    cumcwcredit money,
    totalhours double precision,
    cumtotalhours double precision,
    cumsubcontracts double precision,
    cumtestcosts double precision,
    paycosts double precision,
    cumpaycosts double precision,
    CONSTRAINT pk_my_projectmonthfinal PRIMARY KEY (year, project, monthno)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_proj_invoice.sql
-- Table: mabarchive.my_proj_invoice

CREATE TABLE IF NOT EXISTS mabarchive.my_proj_invoice (
    year smallint NOT NULL,
    projectparent character varying(20) NOT NULL,
    month integer,
    amount money,
    costofwork money,
    wip money,
    profitloss money,
    detail character varying(100),
    invoicecounter integer NOT NULL,
    type character varying(10),
    CONSTRAINT pk_my_proj_invoice PRIMARY KEY (year, projectparent, invoicecounter)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_proj_subcontract.sql
-- Table: mabarchive.my_proj_subcontract

CREATE TABLE IF NOT EXISTS mabarchive.my_proj_subcontract (
    year smallint NOT NULL,
    subcontcounter integer NOT NULL,
    project character varying(20),
    testjob character varying(50),
    month real,
    amount money,
    workgroup character varying(50),
    acctcode character varying(30),
    supplier character varying(50),
    description character varying(255),
    suppliernumber integer,
    dailyrate money,
    animaldays integer,
    CONSTRAINT pk_my_proj_subcontract PRIMARY KEY (year, subcontcounter)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_profitcentregrade.sql
-- Table: mabarchive.my_profitcentregrade

CREATE TABLE IF NOT EXISTS mabarchive.my_profitcentregrade (
    year integer NOT NULL,
    pcgrade character varying(20) NOT NULL,
    divisiongrade character varying(10) NOT NULL,
    gradecode character varying(10) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    chargerate money,
    directrate money,
    payrate money,
    npr money,
    ohr money,
    CONSTRAINT pk__my_profitcentregrad__2bde8e15 PRIMARY KEY (year, pcgrade)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_staff.sql
-- Table: mabarchive.my_staff

CREATE TABLE IF NOT EXISTS mabarchive.my_staff (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL,
    workgroupgrade character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    title character varying(4),
    personstatus character varying(10),
    personclass character varying(10),
    hrspaid double precision,
    leave double precision,
    sickspecial double precision,
    hrsavail double precision,
    CONSTRAINT pk_my_staff PRIMARY KEY (year, staffid)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tbladditionalcosts.sql
-- Table: mabarchive.my_tbladditionalcosts

CREATE TABLE IF NOT EXISTS mabarchive.my_tbladditionalcosts (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL,
    account character varying(50) NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    ac_counter integer NOT NULL,
    CONSTRAINT pk_my_tbladditionalcosts PRIMARY KEY (ac_counter)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tblanimalreq.sql
-- Table: mabarchive.my_tblanimalreq

CREATE TABLE IF NOT EXISTS mabarchive.my_tblanimalreq (
    year smallint NOT NULL,
    jobcode character varying(20) NOT NULL,
    animaltype character varying(50) NOT NULL,
    numberofdays double precision NOT NULL,
    numberofanimals double precision NOT NULL,
    ar_counter integer DEFAULT nextval('mabarchive."my_tblanimalreq_AR_Counter_seq"'::regclass) NOT NULL,
    CONSTRAINT pk_my_tblanimalreq PRIMARY KEY (ar_counter)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tblanimals.sql
-- Table: mabarchive.my_tblanimals

CREATE TABLE IF NOT EXISTS mabarchive.my_tblanimals (
    year smallint NOT NULL,
    animaltype character varying(50) NOT NULL,
    species character varying(50),
    security_level character varying(50),
    dailyrate money,
    planbyweek boolean,
    defradailyrate money,
    CONSTRAINT pk__my_tblanimals__18ebb532 PRIMARY KEY (year, animaltype)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tblcontract.sql
-- Table: mabarchive.my_tblcontract

CREATE TABLE IF NOT EXISTS mabarchive.my_tblcontract (
    year smallint NOT NULL,
    contractno character varying(10) NOT NULL,
    category character varying(20) NOT NULL,
    manager character varying(50),
    customer character varying(50),
    title character varying(100),
    registereddate date,
    startdate date,
    enddate date,
    contractdoc bytea,
    duration integer,
    CONSTRAINT pk_my_tblcontract PRIMARY KEY (year, contractno)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tblprofitcentre.sql
-- Table: mabarchive.my_tblprofitcentre

CREATE TABLE IF NOT EXISTS mabarchive.my_tblprofitcentre (
    year smallint NOT NULL,
    profitcentre character varying(50) NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division character varying(10) NOT NULL,
    conttarget money,
    profitcentrehead character varying(50),
    divisionid integer,
    CONSTRAINT pk__tblkpprofitcentr__1db06a4f PRIMARY KEY (year, profitcentre)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tblstaffjob.sql
-- Table: mabarchive.my_tblstaffjob

CREATE TABLE IF NOT EXISTS mabarchive.my_tblstaffjob (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision NOT NULL,
    systimestamp bytea,
    CONSTRAINT pk_my_tblstaffjob PRIMARY KEY (year, staffid, jobcode)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_testorproduct.sql
-- Table: mabarchive.my_testorproduct

CREATE TABLE IF NOT EXISTS mabarchive.my_testorproduct (
    year smallint NOT NULL,
    itemcode character varying(20) NOT NULL,
    itemdescription character varying(200),
    testmanager character varying(50),
    jobstatus character varying(2),
    unitpricevla money,
    priceahvg money,
    owner character varying(2),
    chargemethod character varying(5),
    shortdescription character(18),
    defraunitprice money,
    CONSTRAINT pk_my_testorproduct PRIMARY KEY (year, itemcode)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_timecostcalcs.sql
-- Table: mabarchive.my_timecostcalcs

CREATE TABLE IF NOT EXISTS mabarchive.my_timecostcalcs (
    year smallint NOT NULL,
    workgroup character varying(50) NOT NULL,
    jobcode character varying(50) NOT NULL,
    project character varying(20) NOT NULL,
    month double precision NOT NULL,
    staffid character varying(50) NOT NULL,
    gradecode character varying(10),
    name character varying(50),
    chargerate money,
    class character varying(255),
    time double precision,
    cost double precision,
    division character varying(10),
    jobcodeold character varying(14),
    pay money,
    nonpay money,
    overhead money,
    CONSTRAINT pk_my_timecostcalcs PRIMARY KEY (year, workgroup, jobcode, project, month, staffid)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tlkpprogram.sql
-- Table: mabarchive.my_tlkpprogram

CREATE TABLE IF NOT EXISTS mabarchive.my_tlkpprogram (
    year smallint NOT NULL,
    programno character varying(10) NOT NULL,
    programname character varying(80),
    directorate character varying(15),
    minim character varying(7),
    sector_name character varying(50),
    customer character varying(50),
    target money,
    manager character varying(50),
    CONSTRAINT pk_my_tlkpprogram PRIMARY KEY (year, programno)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tlkpproject.sql
-- Table: mabarchive.my_tlkpproject

CREATE TABLE IF NOT EXISTS mabarchive.my_tlkpproject (
    year smallint NOT NULL,
    parentproject character varying(20) NOT NULL,
    program character varying(10),
    customer character varying(50),
    manager character varying(50),
    transferincome money,
    custincome money,
    wip_eoy money,
    wip_limit money,
    wip_current money,
    projectstatus character varying(50),
    datecreated timestamp without time zone,
    feccost money,
    profit money,
    budget_cvl money,
    caseworksub numeric(5,4),
    pvsincome money,
    plancaseworkdebit money,
    source character(5),
    disease character varying(50),
    contract character varying(10),
    finished smallint,
    comments text,
    carryover money,
    isdefraproject smallint,
    costcentre double precision,
    oracleprojectcode character varying(50),
    subaccountcode character varying(50),
    projectgroup character varying(50),
    incomeaccountcode character varying(50),
    CONSTRAINT pk_my_tlkpproject PRIMARY KEY (year, parentproject)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_tlkptestreqmt.sql
-- Table: mabarchive.my_tlkptestreqmt

CREATE TABLE IF NOT EXISTS mabarchive.my_tlkptestreqmt (
    year smallint NOT NULL,
    testcode character varying(20) NOT NULL,
    buyer character varying(20) NOT NULL,
    unitprice money,
    norequired double precision,
    projectbuyercode character varying(50),
    testbuyercode character varying(50),
    source character(5),
    CONSTRAINT pk_my_tlkptestreqmt PRIMARY KEY (year, testcode, buyer)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_workgroup.sql
-- Table: mabarchive.my_workgroup

CREATE TABLE IF NOT EXISTS mabarchive.my_workgroup (
    year smallint NOT NULL,
    workgroup character varying(50) NOT NULL,
    profitcentre character varying(50) NOT NULL,
    costcentre double precision,
    owner character varying(50),
    description character varying(45),
    centraloverhead money,
    sendemail smallint,
    cos90 smallint,
    costcentreold double precision,
    email_recipient character varying(50),
    CONSTRAINT pk_my_workgroup PRIMARY KEY (year, workgroup)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/my_workgroupgrade.sql
-- Table: mabarchive.my_workgroupgrade

CREATE TABLE IF NOT EXISTS mabarchive.my_workgroupgrade (
    year integer NOT NULL,
    wggrade character varying(50) NOT NULL,
    profitcentregrade character varying(20) NOT NULL,
    gradecode character varying(10) NOT NULL,
    workgroup character varying(50) NOT NULL,
    CONSTRAINT pk__my_workgroupgrade__2de6d218 PRIMARY KEY (year, wggrade)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/g_tlkpproject.sql
-- Table: mabarchive.g_tlkpproject

CREATE TABLE IF NOT EXISTS mabarchive.g_tlkpproject (
    parentproject character varying(20) NOT NULL,
    projecttitle character varying(200),
    costbookno character varying(50),
    disease character varying(50),
    contract character varying(10),
    shorttitle character varying(30),
    projectstatus character varying(50),
    CONSTRAINT pk_g_tlkpproject PRIMARY KEY (parentproject)
);


-- Imported from: dbscript/schemas/02mabarchive/01tables/tlkpyear.sql
-- Table: mabarchive.tlkpyear

CREATE TABLE IF NOT EXISTS mabarchive.tlkpyear (
    year integer NOT NULL,
    latestmonthreleased integer,
    CONSTRAINT pk_tlkpyear PRIMARY KEY (year)
);


-- Align sequence ownership when table/column exists.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'mabarchive'
          AND table_name = 'my_tblanimalreq'
          AND column_name = 'ar_counter'
    ) THEN
        ALTER SEQUENCE mabarchive."my_tblanimalreq_AR_Counter_seq"
            OWNED BY mabarchive.my_tblanimalreq.ar_counter;
    END IF;
END $$;

COMMIT;
