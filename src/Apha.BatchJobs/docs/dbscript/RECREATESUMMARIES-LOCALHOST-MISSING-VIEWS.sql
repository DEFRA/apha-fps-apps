-- RecreateSummaries missing views bootstrap for local batch_jobs_foundation_db
-- Generated on 2026-05-11
-- Apply in pgAdmin query tool connected to database: batch_jobs_foundation_db

BEGIN;

-- =====================================================================
-- vpacttblstaff
-- Source: dbscript/schemas/01fps/04views/vpacttblstaff.sql
-- =====================================================================
-- View: fps.vpacttblstaff

CREATE OR REPLACE VIEW fps.vpacttblstaff AS
 SELECT tblwgemployee.pactid,
    tblemployee.spnumber,
    (((COALESCE(tblemployee.lastname, ''::character varying))::text || ', '::text) || (COALESCE(tblemployee.firstname, ''::character varying))::text) AS name,
    tblwgemployee.workgroupgrade,
    tblemployee.title,
    tblwgemployee.personstatus,
    tblwgemployee.personclass,
    tblwgemployee.hrspaid,
    tblwgemployee.leave,
    tblwgemployee.sickspecial,
    tblwgemployee.hrsavail
   FROM (fps.tblemployee
     JOIN fps.tblwgemployee ON (((tblemployee.spnumber)::text = (tblwgemployee.spnumber)::text)));

-- =====================================================================
-- vpacttlkptestcapability
-- Source: dbscript/schemas/01fps/04views/vpacttlkptestcapability.sql
-- =====================================================================
-- View: fps.vpacttlkptestcapability

CREATE OR REPLACE VIEW fps.vpacttlkptestcapability AS
 SELECT testcode,
    workgroup,
    planportfolio,
    smscode,
    ((testcode)::text || (workgroup)::text) AS wgtestcode
   FROM fps.tlkptestcapability;

-- =====================================================================
-- qrymilestone1
-- Source: dbscript/schemas/01fps/04views/qrymilestone1.sql
-- =====================================================================
-- View: fps.qrymilestone1

CREATE OR REPLACE VIEW fps.qrymilestone1 AS
 SELECT DISTINCT project,
    milestoneref,
    plandate,
    actualdate,
    monthnofin AS duemonth,
        CASE
            WHEN (actualdate <= plandate) THEN (1)::numeric
            ELSE (0)::numeric
        END AS ontimeflag,
        CASE
            WHEN (actualdate IS NULL) THEN 0
            ELSE 1
        END AS completeflag,
   year,
   fpsyear
   FROM fps.milestone;

-- =====================================================================
-- qryjobmonthmilestone
-- Source: dbscript/schemas/01fps/04views/qryjobmonthmilestone.sql
-- =====================================================================
-- View: fps.qryjobmonthmilestone
-- Source: cloud export from dbscript/docs/CloudDump/viewsInCloud

CREATE OR REPLACE VIEW fps.qryjobmonthmilestone AS
SELECT
    project,
    duemonth,
    count(milestoneref) AS mstonedue,
    sum(completeflag) AS due__done,
    sum(ontimeflag) AS ontime
FROM fps.qrymilestone1
GROUP BY project, duemonth;

-- =====================================================================
-- qryprojectmonthcw
-- Source: dbscript/schemas/01fps/04views/qryprojectmonthcw.sql
-- =====================================================================
-- View: fps.qryprojectmonthcw

CREATE OR REPLACE VIEW fps.qryprojectmonthcw AS
 SELECT DISTINCT projectmonth.project,
    projectmonth.monthno,
    (tlkpproject.plancaseworkdebit / 12) AS cwdebit,
    ((tlkpproject.transferincome * (tlkpproject.caseworksub)::double precision) / 12) AS cwcredit
   FROM (fps.tlkpproject
     JOIN fps.projectmonth ON (((tlkpproject.parentproject)::text = (projectmonth.project)::text)));

-- =====================================================================
-- qryjobmonth_subcontracts1
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_subcontracts1.sql
-- =====================================================================
-- View: fps.qryjobmonth_subcontracts1

CREATE OR REPLACE VIEW fps.qryjobmonth_subcontracts1 AS
 SELECT DISTINCT project,
    month,
    acctcode,
    sum((amount)::numeric) AS total,
        CASE
            WHEN ((acctcode)::text = ANY ((ARRAY['LargeAnimals'::character varying, 'SmallAnimals'::character varying, 'Mice'::character varying])::text[])) THEN sum((amount)::numeric)
            ELSE (0)::numeric
        END AS animals1,
        CASE
            WHEN ((acctcode)::text = ANY ((ARRAY['LargeAnimals'::character varying, 'SmallAnimals'::character varying, 'Mice'::character varying])::text[])) THEN (0)::numeric
            ELSE sum((amount)::numeric)
        END AS other1
   FROM fps.proj_subcontract
  GROUP BY project, month, acctcode;

-- =====================================================================
-- qryjobmonth_subcontracts
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_subcontracts.sql
-- =====================================================================
-- View: fps.qryjobmonth_subcontracts

CREATE OR REPLACE VIEW fps.qryjobmonth_subcontracts AS
 SELECT project,
    month,
    sum(animals1) AS animals,
    sum(other1) AS other,
    (sum(animals1) + sum(other1)) AS total
   FROM fps.qryjobmonth_subcontracts1
  GROUP BY project, month;

-- =====================================================================
-- qryjobmonth_invoices
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_invoices.sql
-- =====================================================================
-- View: fps.qryjobmonth_invoices

CREATE OR REPLACE VIEW fps.qryjobmonth_invoices AS
 SELECT projectparent,
    month,
    sum(amount) AS sumofamount1,
    sum(costofwork) AS workcost
   FROM fps.proj_invoice
  GROUP BY projectparent, month;

-- =====================================================================
-- qryjobmonthportfoliosales
-- Source: dbscript/schemas/01fps/04views/qryjobmonthportfoliosales.sql
-- =====================================================================
-- View: fps.qryjobmonthportfoliosales

CREATE OR REPLACE VIEW fps.qryjobmonthportfoliosales AS
 SELECT DISTINCT tlkptestcapability.planportfolio,
    monthlyoutput.month,
    sum((tlkptestreqmt.unitprice * monthlyoutput.volume)) AS fee
   FROM (fps.tlkptestreqmt
     JOIN (fps.tlkptestcapability
     JOIN fps.monthlyoutput ON ((((tlkptestcapability.workgroup)::text = (monthlyoutput.workgroup)::text) AND ((tlkptestcapability.testcode)::text = (monthlyoutput.testcode)::text)))) ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text))))
  GROUP BY tlkptestcapability.planportfolio, monthlyoutput.month;

-- =====================================================================
-- qryjobmonth_tctransfers
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_tctransfers.sql
-- =====================================================================
-- View: fps.qryjobmonth_tctransfers

CREATE OR REPLACE VIEW fps.qryjobmonth_tctransfers AS
 SELECT vpacttlkptestcapability.planportfolio AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.monthlyoutput
     JOIN fps.tlkptestreqmt ON ((((monthlyoutput.testcode)::text = (tlkptestreqmt.testcode)::text) AND ((monthlyoutput.buyer)::text = (tlkptestreqmt.buyer)::text))))
     JOIN fps.vpacttlkptestcapability ON (((tlkptestreqmt.buyer)::text = vpacttlkptestcapability.wgtestcode)))
  GROUP BY vpacttlkptestcapability.planportfolio, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice;

-- =====================================================================
-- qryjobmonth_transfers1
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_transfers1.sql
-- =====================================================================
-- View: fps.qryjobmonth_transfers1

CREATE OR REPLACE VIEW fps.qryjobmonth_transfers1 AS
 SELECT DISTINCT monthlyoutput.buyer AS project,
    monthlyoutput.month,
    monthlyoutput.testcode,
    monthlyoutput.volume,
    tlkptestreqmt.unitprice AS intunitprice,
    sum((monthlyoutput.volume * tlkptestreqmt.unitprice)) AS transfercost
   FROM ((fps.testorproduct
     JOIN fps.tlkptestreqmt ON (((testorproduct.itemcode)::text = (tlkptestreqmt.testcode)::text)))
     JOIN fps.monthlyoutput ON ((((tlkptestreqmt.buyer)::text = (monthlyoutput.buyer)::text) AND ((tlkptestreqmt.testcode)::text = (monthlyoutput.testcode)::text))))
  GROUP BY monthlyoutput.buyer, monthlyoutput.month, monthlyoutput.testcode, monthlyoutput.volume, tlkptestreqmt.unitprice;

-- =====================================================================
-- qryjobmonth_transferunion
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_transferunion.sql
-- =====================================================================
-- View: fps.qryjobmonth_transferunion

CREATE OR REPLACE VIEW fps.qryjobmonth_transferunion AS
 SELECT qryjobmonth_tctransfers.project,
    qryjobmonth_tctransfers.month,
    qryjobmonth_tctransfers.transfercost
   FROM fps.qryjobmonth_tctransfers
UNION ALL
 SELECT qryjobmonth_transfers1.project,
    qryjobmonth_transfers1.month,
    qryjobmonth_transfers1.transfercost
   FROM fps.qryjobmonth_transfers1;

-- =====================================================================
-- qryjobmonth_transferstotal
-- Source: dbscript/schemas/01fps/04views/qryjobmonth_transferstotal.sql
-- =====================================================================
-- View: fps.qryjobmonth_transferstotal

CREATE OR REPLACE VIEW fps.qryjobmonth_transferstotal AS
 SELECT DISTINCT project,
    month,
    sum(transfercost) AS sumoftransfercost
   FROM fps.qryjobmonth_transferunion
  GROUP BY project, month;

COMMIT;

-- Validation: list any still-missing required RecreateSummaries views
WITH req(obj_name) AS (
  VALUES
  ('vpacttblstaff'),
  ('vpacttlkptestcapability'),
  ('qrymilestone1'),
  ('qryjobmonthmilestone'),
  ('qryprojectmonthcw'),
  ('qryjobmonth_subcontracts1'),
  ('qryjobmonth_subcontracts'),
  ('qryjobmonth_invoices'),
  ('qryjobmonthportfoliosales'),
  ('qryjobmonth_tctransfers'),
  ('qryjobmonth_transfers1'),
  ('qryjobmonth_transferunion'),
  ('qryjobmonth_transferstotal')
)
SELECT r.obj_name
FROM req r
LEFT JOIN information_schema.views v
  ON v.table_schema = 'fps' AND lower(v.table_name) = lower(r.obj_name)
WHERE v.table_name IS NULL
ORDER BY r.obj_name;
