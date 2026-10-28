--liquibase formatted sql

--changeset migration-fix:003-key-normalization labels:dml context:all splitStatements:false runInTransaction:true
SET LOCAL session_replication_role = replica;

-- SECTION 3 - Remove leading/trailing whitespace from key columns.
-- ---------------------------------------------------------------------------
UPDATE fps.monthlyoutput SET buyer = btrim(buyer) WHERE buyer <> btrim(buyer);
UPDATE fps.monthlytime SET parentproject = btrim(parentproject) WHERE parentproject <> btrim(parentproject);
UPDATE fps.monthlytime SET timecode = btrim(timecode) WHERE timecode <> btrim(timecode);
UPDATE fps.tbltestrccost SET profitcentre = btrim(profitcentre) WHERE profitcentre <> btrim(profitcentre);
UPDATE fps.tbltestrccost SET testcode = btrim(testcode) WHERE testcode <> btrim(testcode);
UPDATE fps.timecodevalid SET parentproject = btrim(parentproject) WHERE parentproject <> btrim(parentproject);
UPDATE fps.tlkptestcapability SET planportfolio = btrim(planportfolio) WHERE planportfolio <> btrim(planportfolio);

-- ---------------------------------------------------------------------------
-- SECTION 4.1 - Align child key values to their parent spelling.
-- ---------------------------------------------------------------------------
-- fk_costcentre_profitcentre : fps.costcentre -> fps.tblkpprofitcentre

UPDATE fps.costcentre c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_divisiongrade_division : fps.divisiongrade -> fps.tlkpdivision

UPDATE fps.divisiongrade c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_divisiongrade_gradecode : fps.divisiongrade -> fps.grade

UPDATE fps.divisiongrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_milestone_project : fps.milestone -> fps.tlkpproject

UPDATE fps.milestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_buyer : fps.monthlyoutput -> fps.tlkptestreqmt

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_workgroup : fps.monthlyoutput -> fps.tlkptestcapability

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, workgroup = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.workgroup::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.workgroup::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, workgroup, fpsyear FROM fps.tlkptestcapability WHERE testcode IS NOT NULL AND workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.workgroup::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.workgroup::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestcapability px WHERE px.testcode = c.testcode AND px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_pactstaffid : fps.monthlytime -> fps.tblwgemployee

UPDATE fps.monthlytime c

   SET pactstaffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.pactstaffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.pactstaffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.pactstaffid AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_timecodevalid : fps.monthlytime -> fps.timecodevalid

UPDATE fps.monthlytime c

   SET workgroup = m.p1, timecode = m.p2, parentproject = m.p3

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.timecode::text)) AS k2, lower(btrim(d.parentproject::text)) AS k3, lower(btrim(d.fpsyear::text)) AS k4, max(d.workgroup::text) AS p1, max(d.timecode::text) AS p2, max(d.parentproject::text) AS p3, max(d.fpsyear::text) AS p4, count(*) AS n

          FROM (SELECT DISTINCT workgroup, timecode, parentproject, fpsyear FROM fps.timecodevalid WHERE workgroup IS NOT NULL AND timecode IS NOT NULL AND parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3, 4) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.timecode::text)) = m.k2

        AND lower(btrim(c.parentproject::text)) = m.k3

        AND lower(btrim(c.fpsyear::text)) = m.k4

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.timecode::text IS DISTINCT FROM m.p2 OR c.parentproject::text IS DISTINCT FROM m.p3)

   AND NOT EXISTS (SELECT 1 FROM fps.timecodevalid px WHERE px.workgroup = c.workgroup AND px.timecode = c.timecode AND px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_plancatwggrade_plancategory : fps.plancatwggrade -> fps.tblkpplanningcategory

UPDATE fps.plancatwggrade c

   SET plancategory = m.p1

  FROM (SELECT lower(btrim(d.planningcategory::text)) AS k1, max(d.planningcategory::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT planningcategory FROM fps.tblkpplanningcategory WHERE planningcategory IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.plancategory::text)) = m.k1

   AND (c.plancategory::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpplanningcategory px WHERE px.planningcategory = c.plancategory);

-- fk_plancatwggrade_wggrade : fps.plancatwggrade -> fps.workgroupgrade

UPDATE fps.plancatwggrade c

   SET wggrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.wggrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.wggrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.wggrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_divisiongrade : fps.profitcentregrade -> fps.divisiongrade

UPDATE fps.profitcentregrade c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_gradecode : fps.profitcentregrade -> fps.grade

UPDATE fps.profitcentregrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_profitcentre : fps.profitcentregrade -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_profitcentregrade_nondefra_divisiongrade : fps.profitcentregrade_nondefra -> fps.divisiongrade

UPDATE fps.profitcentregrade_nondefra c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_gradecode : fps.profitcentregrade_nondefra -> fps.grade

UPDATE fps.profitcentregrade_nondefra c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_profitcentre : fps.profitcentregrade_nondefra -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade_nondefra c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_proj_invoice_projectparent : fps.proj_invoice -> fps.tlkpproject

UPDATE fps.proj_invoice c

   SET projectparent = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectparent::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectparent::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.projectparent AND px.fpsyear = c.fpsyear);

-- fk_proj_subcontract_project : fps.proj_subcontract -> fps.tlkpproject

UPDATE fps.proj_subcontract c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_account : fps.tbladditionalcosts -> fps.tblkpaccountcategory

UPDATE fps.tbladditionalcosts c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_jobcode : fps.tbladditionalcosts -> fps.tlkpproject

UPDATE fps.tbladditionalcosts c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_animaltype : fps.tblanimalreq -> fps.tblanimals

UPDATE fps.tblanimalreq c

   SET animaltype = m.p1

  FROM (SELECT lower(btrim(d.animaltype::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.animaltype::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT animaltype, fpsyear FROM fps.tblanimals WHERE animaltype IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.animaltype::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.animaltype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblanimals px WHERE px.animaltype = c.animaltype AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_jobcode : fps.tblanimalreq -> fps.tlkpproject

UPDATE fps.tblanimalreq c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblbid_account : fps.tblbid -> fps.tblkpaccountcategory

UPDATE fps.tblbid c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblbid_workgroup : fps.tblbid -> fps.workgroup

UPDATE fps.tblbid c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tblcontract_3__10 : fps.tblcontract -> fps.tblcategory

UPDATE fps.tblcontract c

   SET category = m.p1

  FROM (SELECT lower(btrim(d.category::text)) AS k1, max(d.category::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT category FROM fps.tblcategory WHERE category IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.category::text)) = m.k1

   AND (c.category::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcategory px WHERE px.category = c.category);

-- fk_tblcontract_customer : fps.tblcontract -> fps.tlkpcustomer

UPDATE fps.tblcontract c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tblkpprofitcentre_division : fps.tblkpprofitcentre -> fps.tlkpdivision

UPDATE fps.tblkpprofitcentre c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_tblpaymentschedule_contract : fps.tblpaymentschedule -> fps.tblcontract

UPDATE fps.tblpaymentschedule c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tblpurchase_workgroup_account : fps.tblpurchase -> fps.tblbid

UPDATE fps.tblpurchase c

   SET workgroup = m.p1, account = m.p2

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.account::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.workgroup::text) AS p1, max(d.account::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT workgroup, account, fpsyear FROM fps.tblbid WHERE workgroup IS NOT NULL AND account IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.account::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.account::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tblbid px WHERE px.workgroup = c.workgroup AND px.account = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_jobcode : fps.tblstaffjob -> fps.tlkpproject

UPDATE fps.tblstaffjob c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_staffid : fps.tblstaffjob -> fps.tblwgemployee

UPDATE fps.tblstaffjob c

   SET staffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.staffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.staffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.staffid AND px.fpsyear = c.fpsyear);

-- fk_tbltestrccost_profitcentre : fps.tbltestrccost -> fps.tblkpprofitcentre

UPDATE fps.tbltestrccost c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_tbltestrccost_testcode : fps.tbltestrccost -> fps.testorproduct

UPDATE fps.tbltestrccost c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_buyer : fps.tbltestrequirementrccost -> fps.tlkptestreqmt

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_profitcentre : fps.tbltestrequirementrccost -> fps.tbltestrccost

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, profitcentre = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.profitcentre::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.profitcentre::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, profitcentre, fpsyear FROM fps.tbltestrccost WHERE testcode IS NOT NULL AND profitcentre IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.profitcentre::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.profitcentre::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tbltestrccost px WHERE px.testcode = c.testcode AND px.profitcentre = c.profitcentre AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_spnumber : fps.tblwgemployee -> fps.tblemployee

UPDATE fps.tblwgemployee c

   SET spnumber = m.p1

  FROM (SELECT lower(btrim(d.spnumber::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.spnumber::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT spnumber, fpsyear FROM fps.tblemployee WHERE spnumber IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.spnumber::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.spnumber::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblemployee px WHERE px.spnumber = c.spnumber AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_workgroupgrade : fps.tblwgemployee -> fps.workgroupgrade

UPDATE fps.tblwgemployee c

   SET workgroupgrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroupgrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroupgrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.workgroupgrade AND px.fpsyear = c.fpsyear);

-- fk_timecodevalid_parentproject : fps.timecodevalid -> fps.tlkpproject

UPDATE fps.timecodevalid c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpjobcode_parentproject : fps.tlkpjobcode -> fps.tlkpproject

UPDATE fps.tlkpjobcode c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_1__10 : fps.tlkpproject -> fps.tblstatus

UPDATE fps.tlkpproject c

   SET projectstatus = m.p1

  FROM (SELECT lower(btrim(d.status::text)) AS k1, max(d.status::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT status FROM fps.tblstatus WHERE status IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.projectstatus::text)) = m.k1

   AND (c.projectstatus::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblstatus px WHERE px.status = c.projectstatus);

-- fk_tlkpproject_1__16 : fps.tlkpproject -> fps.tlkpcustomer

UPDATE fps.tlkpproject c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tlkpproject_contract : fps.tlkpproject -> fps.tblcontract

UPDATE fps.tlkpproject c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_disease : fps.tlkpproject -> fps.tbldisease

UPDATE fps.tlkpproject c

   SET disease = m.p1

  FROM (SELECT lower(btrim(d.disease::text)) AS k1, max(d.disease::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT disease FROM fps.tbldisease WHERE disease IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.disease::text)) = m.k1

   AND (c.disease::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tbldisease px WHERE px.disease = c.disease);

-- fk_tlkpproject_incomeaccountcode : fps.tlkpproject -> fps.tlkpaccountcode

UPDATE fps.tlkpproject c

   SET incomeaccountcode = m.p1

  FROM (SELECT lower(btrim(d.code::text)) AS k1, max(d.code::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT code FROM fps.tlkpaccountcode WHERE code IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.incomeaccountcode::text)) = m.k1

   AND (c.incomeaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpaccountcode px WHERE px.code = c.incomeaccountcode);

-- fk_tlkpproject_program : fps.tlkpproject -> fps.tlkpprogram

UPDATE fps.tlkpproject c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.programno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.programno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT programno, fpsyear FROM fps.tlkpprogram WHERE programno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprogram px WHERE px.programno = c.program AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_projectgroup : fps.tlkpproject -> fps.tlkpprojectgroup

UPDATE fps.tlkpproject c

   SET projectgroup = m.p1

  FROM (SELECT lower(btrim(d.projectgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.projectgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT projectgroup, fpsyear FROM fps.tlkpprojectgroup WHERE projectgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprojectgroup px WHERE px.projectgroup = c.projectgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_subaccountcode : fps.tlkpproject -> fps.tlkpsubaccount

UPDATE fps.tlkpproject c

   SET subaccountcode = m.p1

  FROM (SELECT lower(btrim(d.subaccountcode::text)) AS k1, max(d.subaccountcode::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT subaccountcode FROM fps.tlkpsubaccount WHERE subaccountcode IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.subaccountcode::text)) = m.k1

   AND (c.subaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpsubaccount px WHERE px.subaccountcode = c.subaccountcode);

-- fk_tlkptestcapability_planportfolio : fps.tlkptestcapability -> fps.tlkpproject

UPDATE fps.tlkptestcapability c

   SET planportfolio = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.planportfolio::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.planportfolio::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.planportfolio AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_testcode : fps.tlkptestcapability -> fps.testorproduct

UPDATE fps.tlkptestcapability c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_workgroup : fps.tlkptestcapability -> fps.workgroup

UPDATE fps.tlkptestcapability c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkptestreqmt_testcode : fps.tlkptestreqmt -> fps.testorproduct

UPDATE fps.tlkptestreqmt c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_workgroup_profitcentre : fps.workgroup -> fps.tblkpprofitcentre

UPDATE fps.workgroup c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_workgroupgrade_gradecode : fps.workgroupgrade -> fps.grade

UPDATE fps.workgroupgrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_profitcentregrade : fps.workgroupgrade -> fps.profitcentregrade

UPDATE fps.workgroupgrade c

   SET profitcentregrade = m.p1

  FROM (SELECT lower(btrim(d.pcgrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pcgrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pcgrade, fpsyear FROM fps.profitcentregrade WHERE pcgrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentregrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.profitcentregrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.profitcentregrade px WHERE px.pcgrade = c.profitcentregrade AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_workgroup : fps.workgroupgrade -> fps.workgroup

UPDATE fps.workgroupgrade c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_my_milestoneformdates_g_tlkpproject_radtrackdata : mabarchive.my_milestoneformdates -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_milestoneformdates c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.parentproject);

-- fk_my_radtrack_reports_g_tlkpproject_radtrackdata : mabarchive.my_radtrack_reports -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_radtrack_reports c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_my_tlkpprojectradtrackdata_g_tlkpproject_radtrackdata : mabarchive.my_tlkpprojectradtrackdata -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_tlkpprojectradtrackdata c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblaccessprograms_tblaccessusers : mabarchive.tblaccessprograms -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessprograms c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tblaccessprograms_tblradtrackprog : mabarchive.tblaccessprograms -> mabarchive.tblradtrackprog

UPDATE mabarchive.tblaccessprograms c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.program::text)) AS k1, max(d.program::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT program FROM mabarchive.tblradtrackprog WHERE program IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackprog px WHERE px.program = c.program);

-- fk_tblaccessusers_levels_tblaccessusers : mabarchive.tblaccessusers_levels -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessusers_levels c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tbladditionalcosts_tblprojectyear : mabarchive.tbladditionalcosts -> mabarchive.tblprojectyear

UPDATE mabarchive.tbladditionalcosts c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tblanimalreq_tblprojectyear : mabarchive.tblanimalreq -> mabarchive.tblprojectyear

UPDATE mabarchive.tblanimalreq c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, lower(btrim(d.yearno::text)) AS k2, max(d.project::text) AS p1, max(d.yearno::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT project, yearno FROM mabarchive.tblprojectyear WHERE project IS NOT NULL AND yearno IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.year::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.project = c.project AND px.yearno = c.year);

-- fk_tblcomments_tlkpcommenttopics : mabarchive.tblcomments -> mabarchive.tlkpcommenttopics

UPDATE mabarchive.tblcomments c

   SET topic = m.p1

  FROM (SELECT lower(btrim(d.topic::text)) AS k1, max(d.topic::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT topic FROM mabarchive.tlkpcommenttopics WHERE topic IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.topic::text)) = m.k1

   AND (c.topic::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpcommenttopics px WHERE px.topic = c.topic);

-- fk_tblmilestone_g_tlkpproject_radtrackdata : mabarchive.tblmilestone -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblmilestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblmilestone_tlkpmilestonetype : mabarchive.tblmilestone -> mabarchive.tlkpmilestonetype

UPDATE mabarchive.tblmilestone c

   SET idtype = m.p1

  FROM (SELECT lower(btrim(d.idtype::text)) AS k1, max(d.idtype::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT idtype FROM mabarchive.tlkpmilestonetype WHERE idtype IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.idtype::text)) = m.k1

   AND (c.idtype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpmilestonetype px WHERE px.idtype = c.idtype);

-- fk_tblprojectyear_tblproject : mabarchive.tblprojectyear -> mabarchive.tblproject

UPDATE mabarchive.tblprojectyear c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, max(d.project::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT project FROM mabarchive.tblproject WHERE project IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblproject px WHERE px.project = c.project);

-- fk_tblpublication_tlkppublicationtype : mabarchive.tblpublication -> mabarchive.tlkppublicationtype

UPDATE mabarchive.tblpublication c

   SET type = m.p1

  FROM (SELECT lower(btrim(d.type::text)) AS k1, max(d.type::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT type FROM mabarchive.tlkppublicationtype WHERE type IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.type::text)) = m.k1

   AND (c.type::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkppublicationtype px WHERE px.type = c.type);

-- fk_tblradtrackinvoice_g_tlkpproject_radtrackdata : mabarchive.tblradtrackinvoice -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblradtrackinvoice c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblradtrackinvoice_tblradtrackcontract : mabarchive.tblradtrackinvoice -> mabarchive.tblradtrackcontract

UPDATE mabarchive.tblradtrackinvoice c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contract::text)) AS k1, max(d.contract::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT contract FROM mabarchive.tblradtrackcontract WHERE contract IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackcontract px WHERE px.contract = c.contract);

-- fk_tblstaffrequ_tblprojectyear : mabarchive.tblstaffrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tblstaffrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tbltestrequ_tblprojectyear : mabarchive.tbltestrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tbltestrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);


-- ---------------------------------------------------------------------------
-- SECTION 4.2 - Second pass: fixes children orphaned by pass 1 changing a parent value.
-- ---------------------------------------------------------------------------
-- fk_costcentre_profitcentre : fps.costcentre -> fps.tblkpprofitcentre

UPDATE fps.costcentre c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_divisiongrade_division : fps.divisiongrade -> fps.tlkpdivision

UPDATE fps.divisiongrade c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_divisiongrade_gradecode : fps.divisiongrade -> fps.grade

UPDATE fps.divisiongrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_milestone_project : fps.milestone -> fps.tlkpproject

UPDATE fps.milestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_buyer : fps.monthlyoutput -> fps.tlkptestreqmt

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_workgroup : fps.monthlyoutput -> fps.tlkptestcapability

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, workgroup = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.workgroup::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.workgroup::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, workgroup, fpsyear FROM fps.tlkptestcapability WHERE testcode IS NOT NULL AND workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.workgroup::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.workgroup::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestcapability px WHERE px.testcode = c.testcode AND px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_pactstaffid : fps.monthlytime -> fps.tblwgemployee

UPDATE fps.monthlytime c

   SET pactstaffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.pactstaffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.pactstaffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.pactstaffid AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_timecodevalid : fps.monthlytime -> fps.timecodevalid

UPDATE fps.monthlytime c

   SET workgroup = m.p1, timecode = m.p2, parentproject = m.p3

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.timecode::text)) AS k2, lower(btrim(d.parentproject::text)) AS k3, lower(btrim(d.fpsyear::text)) AS k4, max(d.workgroup::text) AS p1, max(d.timecode::text) AS p2, max(d.parentproject::text) AS p3, max(d.fpsyear::text) AS p4, count(*) AS n

          FROM (SELECT DISTINCT workgroup, timecode, parentproject, fpsyear FROM fps.timecodevalid WHERE workgroup IS NOT NULL AND timecode IS NOT NULL AND parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3, 4) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.timecode::text)) = m.k2

        AND lower(btrim(c.parentproject::text)) = m.k3

        AND lower(btrim(c.fpsyear::text)) = m.k4

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.timecode::text IS DISTINCT FROM m.p2 OR c.parentproject::text IS DISTINCT FROM m.p3)

   AND NOT EXISTS (SELECT 1 FROM fps.timecodevalid px WHERE px.workgroup = c.workgroup AND px.timecode = c.timecode AND px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_plancatwggrade_plancategory : fps.plancatwggrade -> fps.tblkpplanningcategory

UPDATE fps.plancatwggrade c

   SET plancategory = m.p1

  FROM (SELECT lower(btrim(d.planningcategory::text)) AS k1, max(d.planningcategory::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT planningcategory FROM fps.tblkpplanningcategory WHERE planningcategory IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.plancategory::text)) = m.k1

   AND (c.plancategory::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpplanningcategory px WHERE px.planningcategory = c.plancategory);

-- fk_plancatwggrade_wggrade : fps.plancatwggrade -> fps.workgroupgrade

UPDATE fps.plancatwggrade c

   SET wggrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.wggrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.wggrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.wggrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_divisiongrade : fps.profitcentregrade -> fps.divisiongrade

UPDATE fps.profitcentregrade c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_gradecode : fps.profitcentregrade -> fps.grade

UPDATE fps.profitcentregrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_profitcentre : fps.profitcentregrade -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_profitcentregrade_nondefra_divisiongrade : fps.profitcentregrade_nondefra -> fps.divisiongrade

UPDATE fps.profitcentregrade_nondefra c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_gradecode : fps.profitcentregrade_nondefra -> fps.grade

UPDATE fps.profitcentregrade_nondefra c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_profitcentre : fps.profitcentregrade_nondefra -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade_nondefra c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_proj_invoice_projectparent : fps.proj_invoice -> fps.tlkpproject

UPDATE fps.proj_invoice c

   SET projectparent = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectparent::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectparent::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.projectparent AND px.fpsyear = c.fpsyear);

-- fk_proj_subcontract_project : fps.proj_subcontract -> fps.tlkpproject

UPDATE fps.proj_subcontract c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_account : fps.tbladditionalcosts -> fps.tblkpaccountcategory

UPDATE fps.tbladditionalcosts c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_jobcode : fps.tbladditionalcosts -> fps.tlkpproject

UPDATE fps.tbladditionalcosts c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_animaltype : fps.tblanimalreq -> fps.tblanimals

UPDATE fps.tblanimalreq c

   SET animaltype = m.p1

  FROM (SELECT lower(btrim(d.animaltype::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.animaltype::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT animaltype, fpsyear FROM fps.tblanimals WHERE animaltype IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.animaltype::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.animaltype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblanimals px WHERE px.animaltype = c.animaltype AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_jobcode : fps.tblanimalreq -> fps.tlkpproject

UPDATE fps.tblanimalreq c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblbid_account : fps.tblbid -> fps.tblkpaccountcategory

UPDATE fps.tblbid c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblbid_workgroup : fps.tblbid -> fps.workgroup

UPDATE fps.tblbid c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tblcontract_3__10 : fps.tblcontract -> fps.tblcategory

UPDATE fps.tblcontract c

   SET category = m.p1

  FROM (SELECT lower(btrim(d.category::text)) AS k1, max(d.category::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT category FROM fps.tblcategory WHERE category IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.category::text)) = m.k1

   AND (c.category::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcategory px WHERE px.category = c.category);

-- fk_tblcontract_customer : fps.tblcontract -> fps.tlkpcustomer

UPDATE fps.tblcontract c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tblkpprofitcentre_division : fps.tblkpprofitcentre -> fps.tlkpdivision

UPDATE fps.tblkpprofitcentre c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_tblpaymentschedule_contract : fps.tblpaymentschedule -> fps.tblcontract

UPDATE fps.tblpaymentschedule c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tblpurchase_workgroup_account : fps.tblpurchase -> fps.tblbid

UPDATE fps.tblpurchase c

   SET workgroup = m.p1, account = m.p2

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.account::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.workgroup::text) AS p1, max(d.account::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT workgroup, account, fpsyear FROM fps.tblbid WHERE workgroup IS NOT NULL AND account IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.account::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.account::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tblbid px WHERE px.workgroup = c.workgroup AND px.account = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_jobcode : fps.tblstaffjob -> fps.tlkpproject

UPDATE fps.tblstaffjob c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_staffid : fps.tblstaffjob -> fps.tblwgemployee

UPDATE fps.tblstaffjob c

   SET staffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.staffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.staffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.staffid AND px.fpsyear = c.fpsyear);

-- fk_tbltestrccost_profitcentre : fps.tbltestrccost -> fps.tblkpprofitcentre

UPDATE fps.tbltestrccost c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_tbltestrccost_testcode : fps.tbltestrccost -> fps.testorproduct

UPDATE fps.tbltestrccost c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_buyer : fps.tbltestrequirementrccost -> fps.tlkptestreqmt

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_profitcentre : fps.tbltestrequirementrccost -> fps.tbltestrccost

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, profitcentre = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.profitcentre::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.profitcentre::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, profitcentre, fpsyear FROM fps.tbltestrccost WHERE testcode IS NOT NULL AND profitcentre IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.profitcentre::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.profitcentre::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tbltestrccost px WHERE px.testcode = c.testcode AND px.profitcentre = c.profitcentre AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_spnumber : fps.tblwgemployee -> fps.tblemployee

UPDATE fps.tblwgemployee c

   SET spnumber = m.p1

  FROM (SELECT lower(btrim(d.spnumber::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.spnumber::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT spnumber, fpsyear FROM fps.tblemployee WHERE spnumber IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.spnumber::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.spnumber::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblemployee px WHERE px.spnumber = c.spnumber AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_workgroupgrade : fps.tblwgemployee -> fps.workgroupgrade

UPDATE fps.tblwgemployee c

   SET workgroupgrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroupgrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroupgrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.workgroupgrade AND px.fpsyear = c.fpsyear);

-- fk_timecodevalid_parentproject : fps.timecodevalid -> fps.tlkpproject

UPDATE fps.timecodevalid c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpjobcode_parentproject : fps.tlkpjobcode -> fps.tlkpproject

UPDATE fps.tlkpjobcode c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_1__10 : fps.tlkpproject -> fps.tblstatus

UPDATE fps.tlkpproject c

   SET projectstatus = m.p1

  FROM (SELECT lower(btrim(d.status::text)) AS k1, max(d.status::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT status FROM fps.tblstatus WHERE status IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.projectstatus::text)) = m.k1

   AND (c.projectstatus::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblstatus px WHERE px.status = c.projectstatus);

-- fk_tlkpproject_1__16 : fps.tlkpproject -> fps.tlkpcustomer

UPDATE fps.tlkpproject c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tlkpproject_contract : fps.tlkpproject -> fps.tblcontract

UPDATE fps.tlkpproject c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_disease : fps.tlkpproject -> fps.tbldisease

UPDATE fps.tlkpproject c

   SET disease = m.p1

  FROM (SELECT lower(btrim(d.disease::text)) AS k1, max(d.disease::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT disease FROM fps.tbldisease WHERE disease IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.disease::text)) = m.k1

   AND (c.disease::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tbldisease px WHERE px.disease = c.disease);

-- fk_tlkpproject_incomeaccountcode : fps.tlkpproject -> fps.tlkpaccountcode

UPDATE fps.tlkpproject c

   SET incomeaccountcode = m.p1

  FROM (SELECT lower(btrim(d.code::text)) AS k1, max(d.code::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT code FROM fps.tlkpaccountcode WHERE code IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.incomeaccountcode::text)) = m.k1

   AND (c.incomeaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpaccountcode px WHERE px.code = c.incomeaccountcode);

-- fk_tlkpproject_program : fps.tlkpproject -> fps.tlkpprogram

UPDATE fps.tlkpproject c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.programno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.programno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT programno, fpsyear FROM fps.tlkpprogram WHERE programno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprogram px WHERE px.programno = c.program AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_projectgroup : fps.tlkpproject -> fps.tlkpprojectgroup

UPDATE fps.tlkpproject c

   SET projectgroup = m.p1

  FROM (SELECT lower(btrim(d.projectgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.projectgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT projectgroup, fpsyear FROM fps.tlkpprojectgroup WHERE projectgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprojectgroup px WHERE px.projectgroup = c.projectgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_subaccountcode : fps.tlkpproject -> fps.tlkpsubaccount

UPDATE fps.tlkpproject c

   SET subaccountcode = m.p1

  FROM (SELECT lower(btrim(d.subaccountcode::text)) AS k1, max(d.subaccountcode::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT subaccountcode FROM fps.tlkpsubaccount WHERE subaccountcode IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.subaccountcode::text)) = m.k1

   AND (c.subaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpsubaccount px WHERE px.subaccountcode = c.subaccountcode);

-- fk_tlkptestcapability_planportfolio : fps.tlkptestcapability -> fps.tlkpproject

UPDATE fps.tlkptestcapability c

   SET planportfolio = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.planportfolio::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.planportfolio::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.planportfolio AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_testcode : fps.tlkptestcapability -> fps.testorproduct

UPDATE fps.tlkptestcapability c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_workgroup : fps.tlkptestcapability -> fps.workgroup

UPDATE fps.tlkptestcapability c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkptestreqmt_testcode : fps.tlkptestreqmt -> fps.testorproduct

UPDATE fps.tlkptestreqmt c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_workgroup_profitcentre : fps.workgroup -> fps.tblkpprofitcentre

UPDATE fps.workgroup c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_workgroupgrade_gradecode : fps.workgroupgrade -> fps.grade

UPDATE fps.workgroupgrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_profitcentregrade : fps.workgroupgrade -> fps.profitcentregrade

UPDATE fps.workgroupgrade c

   SET profitcentregrade = m.p1

  FROM (SELECT lower(btrim(d.pcgrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pcgrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pcgrade, fpsyear FROM fps.profitcentregrade WHERE pcgrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentregrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.profitcentregrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.profitcentregrade px WHERE px.pcgrade = c.profitcentregrade AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_workgroup : fps.workgroupgrade -> fps.workgroup

UPDATE fps.workgroupgrade c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_my_milestoneformdates_g_tlkpproject_radtrackdata : mabarchive.my_milestoneformdates -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_milestoneformdates c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.parentproject);

-- fk_my_radtrack_reports_g_tlkpproject_radtrackdata : mabarchive.my_radtrack_reports -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_radtrack_reports c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_my_tlkpprojectradtrackdata_g_tlkpproject_radtrackdata : mabarchive.my_tlkpprojectradtrackdata -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_tlkpprojectradtrackdata c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblaccessprograms_tblaccessusers : mabarchive.tblaccessprograms -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessprograms c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tblaccessprograms_tblradtrackprog : mabarchive.tblaccessprograms -> mabarchive.tblradtrackprog

UPDATE mabarchive.tblaccessprograms c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.program::text)) AS k1, max(d.program::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT program FROM mabarchive.tblradtrackprog WHERE program IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackprog px WHERE px.program = c.program);

-- fk_tblaccessusers_levels_tblaccessusers : mabarchive.tblaccessusers_levels -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessusers_levels c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tbladditionalcosts_tblprojectyear : mabarchive.tbladditionalcosts -> mabarchive.tblprojectyear

UPDATE mabarchive.tbladditionalcosts c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tblanimalreq_tblprojectyear : mabarchive.tblanimalreq -> mabarchive.tblprojectyear

UPDATE mabarchive.tblanimalreq c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, lower(btrim(d.yearno::text)) AS k2, max(d.project::text) AS p1, max(d.yearno::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT project, yearno FROM mabarchive.tblprojectyear WHERE project IS NOT NULL AND yearno IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.year::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.project = c.project AND px.yearno = c.year);

-- fk_tblcomments_tlkpcommenttopics : mabarchive.tblcomments -> mabarchive.tlkpcommenttopics

UPDATE mabarchive.tblcomments c

   SET topic = m.p1

  FROM (SELECT lower(btrim(d.topic::text)) AS k1, max(d.topic::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT topic FROM mabarchive.tlkpcommenttopics WHERE topic IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.topic::text)) = m.k1

   AND (c.topic::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpcommenttopics px WHERE px.topic = c.topic);

-- fk_tblmilestone_g_tlkpproject_radtrackdata : mabarchive.tblmilestone -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblmilestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblmilestone_tlkpmilestonetype : mabarchive.tblmilestone -> mabarchive.tlkpmilestonetype

UPDATE mabarchive.tblmilestone c

   SET idtype = m.p1

  FROM (SELECT lower(btrim(d.idtype::text)) AS k1, max(d.idtype::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT idtype FROM mabarchive.tlkpmilestonetype WHERE idtype IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.idtype::text)) = m.k1

   AND (c.idtype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpmilestonetype px WHERE px.idtype = c.idtype);

-- fk_tblprojectyear_tblproject : mabarchive.tblprojectyear -> mabarchive.tblproject

UPDATE mabarchive.tblprojectyear c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, max(d.project::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT project FROM mabarchive.tblproject WHERE project IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblproject px WHERE px.project = c.project);

-- fk_tblpublication_tlkppublicationtype : mabarchive.tblpublication -> mabarchive.tlkppublicationtype

UPDATE mabarchive.tblpublication c

   SET type = m.p1

  FROM (SELECT lower(btrim(d.type::text)) AS k1, max(d.type::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT type FROM mabarchive.tlkppublicationtype WHERE type IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.type::text)) = m.k1

   AND (c.type::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkppublicationtype px WHERE px.type = c.type);

-- fk_tblradtrackinvoice_g_tlkpproject_radtrackdata : mabarchive.tblradtrackinvoice -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblradtrackinvoice c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblradtrackinvoice_tblradtrackcontract : mabarchive.tblradtrackinvoice -> mabarchive.tblradtrackcontract

UPDATE mabarchive.tblradtrackinvoice c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contract::text)) AS k1, max(d.contract::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT contract FROM mabarchive.tblradtrackcontract WHERE contract IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackcontract px WHERE px.contract = c.contract);

-- fk_tblstaffrequ_tblprojectyear : mabarchive.tblstaffrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tblstaffrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tbltestrequ_tblprojectyear : mabarchive.tbltestrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tbltestrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);


-- ---------------------------------------------------------------------------
-- SECTION 4.3 - Third pass: safety margin. Normally changes nothing.
-- ---------------------------------------------------------------------------
-- fk_costcentre_profitcentre : fps.costcentre -> fps.tblkpprofitcentre

UPDATE fps.costcentre c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_divisiongrade_division : fps.divisiongrade -> fps.tlkpdivision

UPDATE fps.divisiongrade c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_divisiongrade_gradecode : fps.divisiongrade -> fps.grade

UPDATE fps.divisiongrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_milestone_project : fps.milestone -> fps.tlkpproject

UPDATE fps.milestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_buyer : fps.monthlyoutput -> fps.tlkptestreqmt

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_monthlyoutput_testcode_workgroup : fps.monthlyoutput -> fps.tlkptestcapability

UPDATE fps.monthlyoutput c

   SET testcode = m.p1, workgroup = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.workgroup::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.workgroup::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, workgroup, fpsyear FROM fps.tlkptestcapability WHERE testcode IS NOT NULL AND workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.workgroup::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.workgroup::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestcapability px WHERE px.testcode = c.testcode AND px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_pactstaffid : fps.monthlytime -> fps.tblwgemployee

UPDATE fps.monthlytime c

   SET pactstaffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.pactstaffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.pactstaffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.pactstaffid AND px.fpsyear = c.fpsyear);

-- fk_monthlytime_timecodevalid : fps.monthlytime -> fps.timecodevalid

UPDATE fps.monthlytime c

   SET workgroup = m.p1, timecode = m.p2, parentproject = m.p3

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.timecode::text)) AS k2, lower(btrim(d.parentproject::text)) AS k3, lower(btrim(d.fpsyear::text)) AS k4, max(d.workgroup::text) AS p1, max(d.timecode::text) AS p2, max(d.parentproject::text) AS p3, max(d.fpsyear::text) AS p4, count(*) AS n

          FROM (SELECT DISTINCT workgroup, timecode, parentproject, fpsyear FROM fps.timecodevalid WHERE workgroup IS NOT NULL AND timecode IS NOT NULL AND parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3, 4) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.timecode::text)) = m.k2

        AND lower(btrim(c.parentproject::text)) = m.k3

        AND lower(btrim(c.fpsyear::text)) = m.k4

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.timecode::text IS DISTINCT FROM m.p2 OR c.parentproject::text IS DISTINCT FROM m.p3)

   AND NOT EXISTS (SELECT 1 FROM fps.timecodevalid px WHERE px.workgroup = c.workgroup AND px.timecode = c.timecode AND px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_plancatwggrade_plancategory : fps.plancatwggrade -> fps.tblkpplanningcategory

UPDATE fps.plancatwggrade c

   SET plancategory = m.p1

  FROM (SELECT lower(btrim(d.planningcategory::text)) AS k1, max(d.planningcategory::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT planningcategory FROM fps.tblkpplanningcategory WHERE planningcategory IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.plancategory::text)) = m.k1

   AND (c.plancategory::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpplanningcategory px WHERE px.planningcategory = c.plancategory);

-- fk_plancatwggrade_wggrade : fps.plancatwggrade -> fps.workgroupgrade

UPDATE fps.plancatwggrade c

   SET wggrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.wggrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.wggrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.wggrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_divisiongrade : fps.profitcentregrade -> fps.divisiongrade

UPDATE fps.profitcentregrade c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_gradecode : fps.profitcentregrade -> fps.grade

UPDATE fps.profitcentregrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_profitcentre : fps.profitcentregrade -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_profitcentregrade_nondefra_divisiongrade : fps.profitcentregrade_nondefra -> fps.divisiongrade

UPDATE fps.profitcentregrade_nondefra c

   SET divisiongrade = m.p1

  FROM (SELECT lower(btrim(d.divisiongrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.divisiongrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT divisiongrade, fpsyear FROM fps.divisiongrade WHERE divisiongrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.divisiongrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.divisiongrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.divisiongrade px WHERE px.divisiongrade = c.divisiongrade AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_gradecode : fps.profitcentregrade_nondefra -> fps.grade

UPDATE fps.profitcentregrade_nondefra c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_profitcentregrade_nondefra_profitcentre : fps.profitcentregrade_nondefra -> fps.tblkpprofitcentre

UPDATE fps.profitcentregrade_nondefra c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_proj_invoice_projectparent : fps.proj_invoice -> fps.tlkpproject

UPDATE fps.proj_invoice c

   SET projectparent = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectparent::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectparent::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.projectparent AND px.fpsyear = c.fpsyear);

-- fk_proj_subcontract_project : fps.proj_subcontract -> fps.tlkpproject

UPDATE fps.proj_subcontract c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.project AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_account : fps.tbladditionalcosts -> fps.tblkpaccountcategory

UPDATE fps.tbladditionalcosts c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tbladditionalcosts_jobcode : fps.tbladditionalcosts -> fps.tlkpproject

UPDATE fps.tbladditionalcosts c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_animaltype : fps.tblanimalreq -> fps.tblanimals

UPDATE fps.tblanimalreq c

   SET animaltype = m.p1

  FROM (SELECT lower(btrim(d.animaltype::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.animaltype::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT animaltype, fpsyear FROM fps.tblanimals WHERE animaltype IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.animaltype::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.animaltype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblanimals px WHERE px.animaltype = c.animaltype AND px.fpsyear = c.fpsyear);

-- fk_tblanimalreq_jobcode : fps.tblanimalreq -> fps.tlkpproject

UPDATE fps.tblanimalreq c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblbid_account : fps.tblbid -> fps.tblkpaccountcategory

UPDATE fps.tblbid c

   SET account = m.p1

  FROM (SELECT lower(btrim(d.accshortname::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.accshortname::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT accshortname, fpsyear FROM fps.tblkpaccountcategory WHERE accshortname IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.account::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.account::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpaccountcategory px WHERE px.accshortname = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblbid_workgroup : fps.tblbid -> fps.workgroup

UPDATE fps.tblbid c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tblcontract_3__10 : fps.tblcontract -> fps.tblcategory

UPDATE fps.tblcontract c

   SET category = m.p1

  FROM (SELECT lower(btrim(d.category::text)) AS k1, max(d.category::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT category FROM fps.tblcategory WHERE category IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.category::text)) = m.k1

   AND (c.category::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcategory px WHERE px.category = c.category);

-- fk_tblcontract_customer : fps.tblcontract -> fps.tlkpcustomer

UPDATE fps.tblcontract c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tblkpprofitcentre_division : fps.tblkpprofitcentre -> fps.tlkpdivision

UPDATE fps.tblkpprofitcentre c

   SET division = m.p1

  FROM (SELECT lower(btrim(d.divname::text)) AS k1, max(d.divname::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT divname FROM fps.tlkpdivision WHERE divname IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.division::text)) = m.k1

   AND (c.division::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpdivision px WHERE px.divname = c.division);

-- fk_tblpaymentschedule_contract : fps.tblpaymentschedule -> fps.tblcontract

UPDATE fps.tblpaymentschedule c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tblpurchase_workgroup_account : fps.tblpurchase -> fps.tblbid

UPDATE fps.tblpurchase c

   SET workgroup = m.p1, account = m.p2

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.account::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.workgroup::text) AS p1, max(d.account::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT workgroup, account, fpsyear FROM fps.tblbid WHERE workgroup IS NOT NULL AND account IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.account::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.workgroup::text IS DISTINCT FROM m.p1 OR c.account::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tblbid px WHERE px.workgroup = c.workgroup AND px.account = c.account AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_jobcode : fps.tblstaffjob -> fps.tlkpproject

UPDATE fps.tblstaffjob c

   SET jobcode = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.jobcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.jobcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.jobcode AND px.fpsyear = c.fpsyear);

-- fk_tblstaffjob_staffid : fps.tblstaffjob -> fps.tblwgemployee

UPDATE fps.tblstaffjob c

   SET staffid = m.p1

  FROM (SELECT lower(btrim(d.pactid::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pactid::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pactid, fpsyear FROM fps.tblwgemployee WHERE pactid IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.staffid::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.staffid::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblwgemployee px WHERE px.pactid = c.staffid AND px.fpsyear = c.fpsyear);

-- fk_tbltestrccost_profitcentre : fps.tbltestrccost -> fps.tblkpprofitcentre

UPDATE fps.tbltestrccost c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_tbltestrccost_testcode : fps.tbltestrccost -> fps.testorproduct

UPDATE fps.tbltestrccost c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_buyer : fps.tbltestrequirementrccost -> fps.tlkptestreqmt

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, buyer = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.buyer::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.buyer::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, buyer, fpsyear FROM fps.tlkptestreqmt WHERE testcode IS NOT NULL AND buyer IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.buyer::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.buyer::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkptestreqmt px WHERE px.testcode = c.testcode AND px.buyer = c.buyer AND px.fpsyear = c.fpsyear);

-- fk_tbltestrequirementrccost_testcode_profitcentre : fps.tbltestrequirementrccost -> fps.tbltestrccost

UPDATE fps.tbltestrequirementrccost c

   SET testcode = m.p1, profitcentre = m.p2

  FROM (SELECT lower(btrim(d.testcode::text)) AS k1, lower(btrim(d.profitcentre::text)) AS k2, lower(btrim(d.fpsyear::text)) AS k3, max(d.testcode::text) AS p1, max(d.profitcentre::text) AS p2, max(d.fpsyear::text) AS p3, count(*) AS n

          FROM (SELECT DISTINCT testcode, profitcentre, fpsyear FROM fps.tbltestrccost WHERE testcode IS NOT NULL AND profitcentre IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2, 3) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.profitcentre::text)) = m.k2

        AND lower(btrim(c.fpsyear::text)) = m.k3

   AND (c.testcode::text IS DISTINCT FROM m.p1 OR c.profitcentre::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM fps.tbltestrccost px WHERE px.testcode = c.testcode AND px.profitcentre = c.profitcentre AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_spnumber : fps.tblwgemployee -> fps.tblemployee

UPDATE fps.tblwgemployee c

   SET spnumber = m.p1

  FROM (SELECT lower(btrim(d.spnumber::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.spnumber::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT spnumber, fpsyear FROM fps.tblemployee WHERE spnumber IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.spnumber::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.spnumber::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblemployee px WHERE px.spnumber = c.spnumber AND px.fpsyear = c.fpsyear);

-- fk_tblwgemployee_workgroupgrade : fps.tblwgemployee -> fps.workgroupgrade

UPDATE fps.tblwgemployee c

   SET workgroupgrade = m.p1

  FROM (SELECT lower(btrim(d.wggrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.wggrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT wggrade, fpsyear FROM fps.workgroupgrade WHERE wggrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroupgrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroupgrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroupgrade px WHERE px.wggrade = c.workgroupgrade AND px.fpsyear = c.fpsyear);

-- fk_timecodevalid_parentproject : fps.timecodevalid -> fps.tlkpproject

UPDATE fps.timecodevalid c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpjobcode_parentproject : fps.tlkpjobcode -> fps.tlkpproject

UPDATE fps.tlkpjobcode c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.parentproject AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_1__10 : fps.tlkpproject -> fps.tblstatus

UPDATE fps.tlkpproject c

   SET projectstatus = m.p1

  FROM (SELECT lower(btrim(d.status::text)) AS k1, max(d.status::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT status FROM fps.tblstatus WHERE status IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.projectstatus::text)) = m.k1

   AND (c.projectstatus::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblstatus px WHERE px.status = c.projectstatus);

-- fk_tlkpproject_1__16 : fps.tlkpproject -> fps.tlkpcustomer

UPDATE fps.tlkpproject c

   SET customer = m.p1

  FROM (SELECT lower(btrim(d.customer::text)) AS k1, max(d.customer::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT customer FROM fps.tlkpcustomer WHERE customer IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.customer::text)) = m.k1

   AND (c.customer::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpcustomer px WHERE px.customer = c.customer);

-- fk_tlkpproject_contract : fps.tlkpproject -> fps.tblcontract

UPDATE fps.tlkpproject c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contractno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.contractno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT contractno, fpsyear FROM fps.tblcontract WHERE contractno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblcontract px WHERE px.contractno = c.contract AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_disease : fps.tlkpproject -> fps.tbldisease

UPDATE fps.tlkpproject c

   SET disease = m.p1

  FROM (SELECT lower(btrim(d.disease::text)) AS k1, max(d.disease::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT disease FROM fps.tbldisease WHERE disease IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.disease::text)) = m.k1

   AND (c.disease::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tbldisease px WHERE px.disease = c.disease);

-- fk_tlkpproject_incomeaccountcode : fps.tlkpproject -> fps.tlkpaccountcode

UPDATE fps.tlkpproject c

   SET incomeaccountcode = m.p1

  FROM (SELECT lower(btrim(d.code::text)) AS k1, max(d.code::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT code FROM fps.tlkpaccountcode WHERE code IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.incomeaccountcode::text)) = m.k1

   AND (c.incomeaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpaccountcode px WHERE px.code = c.incomeaccountcode);

-- fk_tlkpproject_program : fps.tlkpproject -> fps.tlkpprogram

UPDATE fps.tlkpproject c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.programno::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.programno::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT programno, fpsyear FROM fps.tlkpprogram WHERE programno IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprogram px WHERE px.programno = c.program AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_projectgroup : fps.tlkpproject -> fps.tlkpprojectgroup

UPDATE fps.tlkpproject c

   SET projectgroup = m.p1

  FROM (SELECT lower(btrim(d.projectgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.projectgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT projectgroup, fpsyear FROM fps.tlkpprojectgroup WHERE projectgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.projectgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.projectgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpprojectgroup px WHERE px.projectgroup = c.projectgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkpproject_subaccountcode : fps.tlkpproject -> fps.tlkpsubaccount

UPDATE fps.tlkpproject c

   SET subaccountcode = m.p1

  FROM (SELECT lower(btrim(d.subaccountcode::text)) AS k1, max(d.subaccountcode::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT subaccountcode FROM fps.tlkpsubaccount WHERE subaccountcode IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.subaccountcode::text)) = m.k1

   AND (c.subaccountcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpsubaccount px WHERE px.subaccountcode = c.subaccountcode);

-- fk_tlkptestcapability_planportfolio : fps.tlkptestcapability -> fps.tlkpproject

UPDATE fps.tlkptestcapability c

   SET planportfolio = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.parentproject::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT parentproject, fpsyear FROM fps.tlkpproject WHERE parentproject IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.planportfolio::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.planportfolio::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tlkpproject px WHERE px.parentproject = c.planportfolio AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_testcode : fps.tlkptestcapability -> fps.testorproduct

UPDATE fps.tlkptestcapability c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_tlkptestcapability_workgroup : fps.tlkptestcapability -> fps.workgroup

UPDATE fps.tlkptestcapability c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_tlkptestreqmt_testcode : fps.tlkptestreqmt -> fps.testorproduct

UPDATE fps.tlkptestreqmt c

   SET testcode = m.p1

  FROM (SELECT lower(btrim(d.itemcode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.itemcode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT itemcode, fpsyear FROM fps.testorproduct WHERE itemcode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.testcode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.testcode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.testorproduct px WHERE px.itemcode = c.testcode AND px.fpsyear = c.fpsyear);

-- fk_workgroup_profitcentre : fps.workgroup -> fps.tblkpprofitcentre

UPDATE fps.workgroup c

   SET profitcentre = m.p1

  FROM (SELECT lower(btrim(d.profitcentre::text)) AS k1, max(d.profitcentre::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT profitcentre FROM fps.tblkpprofitcentre WHERE profitcentre IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentre::text)) = m.k1

   AND (c.profitcentre::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.tblkpprofitcentre px WHERE px.profitcentre = c.profitcentre);

-- fk_workgroupgrade_gradecode : fps.workgroupgrade -> fps.grade

UPDATE fps.workgroupgrade c

   SET gradecode = m.p1

  FROM (SELECT lower(btrim(d.gradecode::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.gradecode::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT gradecode, fpsyear FROM fps.grade WHERE gradecode IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.gradecode::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.gradecode::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.grade px WHERE px.gradecode = c.gradecode AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_profitcentregrade : fps.workgroupgrade -> fps.profitcentregrade

UPDATE fps.workgroupgrade c

   SET profitcentregrade = m.p1

  FROM (SELECT lower(btrim(d.pcgrade::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.pcgrade::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT pcgrade, fpsyear FROM fps.profitcentregrade WHERE pcgrade IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.profitcentregrade::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.profitcentregrade::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.profitcentregrade px WHERE px.pcgrade = c.profitcentregrade AND px.fpsyear = c.fpsyear);

-- fk_workgroupgrade_workgroup : fps.workgroupgrade -> fps.workgroup

UPDATE fps.workgroupgrade c

   SET workgroup = m.p1

  FROM (SELECT lower(btrim(d.workgroup::text)) AS k1, lower(btrim(d.fpsyear::text)) AS k2, max(d.workgroup::text) AS p1, max(d.fpsyear::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT workgroup, fpsyear FROM fps.workgroup WHERE workgroup IS NOT NULL AND fpsyear IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.workgroup::text)) = m.k1

        AND lower(btrim(c.fpsyear::text)) = m.k2

   AND (c.workgroup::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM fps.workgroup px WHERE px.workgroup = c.workgroup AND px.fpsyear = c.fpsyear);

-- fk_my_milestoneformdates_g_tlkpproject_radtrackdata : mabarchive.my_milestoneformdates -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_milestoneformdates c

   SET parentproject = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.parentproject::text)) = m.k1

   AND (c.parentproject::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.parentproject);

-- fk_my_radtrack_reports_g_tlkpproject_radtrackdata : mabarchive.my_radtrack_reports -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_radtrack_reports c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_my_tlkpprojectradtrackdata_g_tlkpproject_radtrackdata : mabarchive.my_tlkpprojectradtrackdata -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.my_tlkpprojectradtrackdata c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblaccessprograms_tblaccessusers : mabarchive.tblaccessprograms -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessprograms c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tblaccessprograms_tblradtrackprog : mabarchive.tblaccessprograms -> mabarchive.tblradtrackprog

UPDATE mabarchive.tblaccessprograms c

   SET program = m.p1

  FROM (SELECT lower(btrim(d.program::text)) AS k1, max(d.program::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT program FROM mabarchive.tblradtrackprog WHERE program IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.program::text)) = m.k1

   AND (c.program::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackprog px WHERE px.program = c.program);

-- fk_tblaccessusers_levels_tblaccessusers : mabarchive.tblaccessusers_levels -> mabarchive.tblaccessusers

UPDATE mabarchive.tblaccessusers_levels c

   SET ntlogin = m.p2

  FROM (SELECT lower(btrim(d.systemid::text)) AS k1, lower(btrim(d.ntlogin::text)) AS k2, max(d.systemid::text) AS p1, max(d.ntlogin::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT systemid, ntlogin FROM mabarchive.tblaccessusers WHERE systemid IS NOT NULL AND ntlogin IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.systemid::text)) = m.k1

        AND lower(btrim(c.ntlogin::text)) = m.k2

   AND (c.ntlogin::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblaccessusers px WHERE px.systemid = c.systemid AND px.ntlogin = c.ntlogin);

-- fk_tbladditionalcosts_tblprojectyear : mabarchive.tbladditionalcosts -> mabarchive.tblprojectyear

UPDATE mabarchive.tbladditionalcosts c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tblanimalreq_tblprojectyear : mabarchive.tblanimalreq -> mabarchive.tblprojectyear

UPDATE mabarchive.tblanimalreq c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, lower(btrim(d.yearno::text)) AS k2, max(d.project::text) AS p1, max(d.yearno::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT project, yearno FROM mabarchive.tblprojectyear WHERE project IS NOT NULL AND yearno IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

        AND lower(btrim(c.year::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.project = c.project AND px.yearno = c.year);

-- fk_tblcomments_tlkpcommenttopics : mabarchive.tblcomments -> mabarchive.tlkpcommenttopics

UPDATE mabarchive.tblcomments c

   SET topic = m.p1

  FROM (SELECT lower(btrim(d.topic::text)) AS k1, max(d.topic::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT topic FROM mabarchive.tlkpcommenttopics WHERE topic IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.topic::text)) = m.k1

   AND (c.topic::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpcommenttopics px WHERE px.topic = c.topic);

-- fk_tblmilestone_g_tlkpproject_radtrackdata : mabarchive.tblmilestone -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblmilestone c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblmilestone_tlkpmilestonetype : mabarchive.tblmilestone -> mabarchive.tlkpmilestonetype

UPDATE mabarchive.tblmilestone c

   SET idtype = m.p1

  FROM (SELECT lower(btrim(d.idtype::text)) AS k1, max(d.idtype::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT idtype FROM mabarchive.tlkpmilestonetype WHERE idtype IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.idtype::text)) = m.k1

   AND (c.idtype::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkpmilestonetype px WHERE px.idtype = c.idtype);

-- fk_tblprojectyear_tblproject : mabarchive.tblprojectyear -> mabarchive.tblproject

UPDATE mabarchive.tblprojectyear c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.project::text)) AS k1, max(d.project::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT project FROM mabarchive.tblproject WHERE project IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblproject px WHERE px.project = c.project);

-- fk_tblpublication_tlkppublicationtype : mabarchive.tblpublication -> mabarchive.tlkppublicationtype

UPDATE mabarchive.tblpublication c

   SET type = m.p1

  FROM (SELECT lower(btrim(d.type::text)) AS k1, max(d.type::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT type FROM mabarchive.tlkppublicationtype WHERE type IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.type::text)) = m.k1

   AND (c.type::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tlkppublicationtype px WHERE px.type = c.type);

-- fk_tblradtrackinvoice_g_tlkpproject_radtrackdata : mabarchive.tblradtrackinvoice -> mabarchive.g_tlkpproject_radtrackdata

UPDATE mabarchive.tblradtrackinvoice c

   SET project = m.p1

  FROM (SELECT lower(btrim(d.parentproject::text)) AS k1, max(d.parentproject::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT parentproject FROM mabarchive.g_tlkpproject_radtrackdata WHERE parentproject IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.project::text)) = m.k1

   AND (c.project::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.g_tlkpproject_radtrackdata px WHERE px.parentproject = c.project);

-- fk_tblradtrackinvoice_tblradtrackcontract : mabarchive.tblradtrackinvoice -> mabarchive.tblradtrackcontract

UPDATE mabarchive.tblradtrackinvoice c

   SET contract = m.p1

  FROM (SELECT lower(btrim(d.contract::text)) AS k1, max(d.contract::text) AS p1, count(*) AS n

          FROM (SELECT DISTINCT contract FROM mabarchive.tblradtrackcontract WHERE contract IS NOT NULL) d

         GROUP BY 1) m

 WHERE m.n = 1

        AND lower(btrim(c.contract::text)) = m.k1

   AND (c.contract::text IS DISTINCT FROM m.p1)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblradtrackcontract px WHERE px.contract = c.contract);

-- fk_tblstaffrequ_tblprojectyear : mabarchive.tblstaffrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tblstaffrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);

-- fk_tbltestrequ_tblprojectyear : mabarchive.tbltestrequ -> mabarchive.tblprojectyear

UPDATE mabarchive.tbltestrequ c

   SET project = m.p2

  FROM (SELECT lower(btrim(d.yearno::text)) AS k1, lower(btrim(d.project::text)) AS k2, max(d.yearno::text) AS p1, max(d.project::text) AS p2, count(*) AS n

          FROM (SELECT DISTINCT yearno, project FROM mabarchive.tblprojectyear WHERE yearno IS NOT NULL AND project IS NOT NULL) d

         GROUP BY 1, 2) m

 WHERE m.n = 1

        AND lower(btrim(c.year::text)) = m.k1

        AND lower(btrim(c.project::text)) = m.k2

   AND (c.project::text IS DISTINCT FROM m.p2)

   AND NOT EXISTS (SELECT 1 FROM mabarchive.tblprojectyear px WHERE px.yearno = c.year AND px.project = c.project);


-- ---------------------------------------------------------------------------
-- Verification. Counts remaining foreign key violations across every declared
-- constraint and aborts the transaction if any survive.
-- ---------------------------------------------------------------------------
DO $verify$
DECLARE
    fk       record;
    n        integer;
    i        integer;
    pred     text;
    nn_pred  text;   -- "notnull" is a reserved word and cannot be a variable name
    bad      bigint;
    total    bigint := 0;
    detail   text := '';
BEGIN
    FOR fk IN
        SELECT c.conname, cn.nspname AS cs, cr.relname AS ct, pn.nspname AS ps, pr.relname AS pt,
               (SELECT array_agg(a.attname ORDER BY k.ord)
                  FROM unnest(c.conkey) WITH ORDINALITY AS k(attnum, ord)
                  JOIN pg_attribute a ON a.attrelid = c.conrelid AND a.attnum = k.attnum) AS ccols,
               (SELECT array_agg(a.attname ORDER BY k.ord)
                  FROM unnest(c.confkey) WITH ORDINALITY AS k(attnum, ord)
                  JOIN pg_attribute a ON a.attrelid = c.confrelid AND a.attnum = k.attnum) AS pcols
        FROM pg_constraint c
        JOIN pg_class cr     ON cr.oid = c.conrelid
        JOIN pg_namespace cn ON cn.oid = cr.relnamespace
        JOIN pg_class pr     ON pr.oid = c.confrelid
        JOIN pg_namespace pn ON pn.oid = pr.relnamespace
        WHERE c.contype = 'f'
          AND c.conparentid = 0          -- skip PostgreSQL's per-partition clones
          AND cn.nspname IN ('fps', 'mabarchive')
        ORDER BY 1
    LOOP
        n := array_length(fk.ccols, 1);
        pred := ''; nn_pred := '';
        FOR i IN 1..n LOOP
            pred    := pred    || format('%sp.%I = c.%I', CASE WHEN i > 1 THEN ' AND ' ELSE '' END, fk.pcols[i], fk.ccols[i]);
            nn_pred := nn_pred || format('%sc.%I IS NOT NULL', CASE WHEN i > 1 THEN ' AND ' ELSE '' END, fk.ccols[i]);
        END LOOP;

        EXECUTE format('SELECT count(*) FROM %I.%I c WHERE %s AND NOT EXISTS (SELECT 1 FROM %I.%I p WHERE %s)',
                       fk.cs, fk.ct, nn_pred, fk.ps, fk.pt, pred) INTO bad;

        IF bad > 0 THEN
            total := total + bad;
            detail := detail || format(E'\n  %s: %s.%s -> %s.%s = %s row(s)',
                                       fk.conname, fk.cs, fk.ct, fk.ps, fk.pt, bad);
        END IF;
    END LOOP;

    IF total > 0 THEN
        RAISE EXCEPTION E'FAIL - % foreign key row(s) still broken:%\nNothing has been committed.', total, detail;
    END IF;

    RAISE NOTICE 'PASS - no foreign key violations remain.';
END
$verify$;


-- Re-enable normal replication behaviour for this session.
RESET session_replication_role;
