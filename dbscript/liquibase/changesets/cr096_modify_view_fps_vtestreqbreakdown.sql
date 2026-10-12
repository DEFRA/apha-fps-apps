--liquibase formatted sql

--changeset Nandkishor:CR096 labels:ddl context:all runOnChange:true splitStatements:false

-- Corrected view: fps.vtestreqbreakdown

CREATE OR REPLACE VIEW fps.vtestreqbreakdown
AS
 WITH default_workgroup AS (
		 SELECT tc.testcode,
			tc.fpsyear,
			min(tc.workgroup::text) AS wg
		   FROM fps.tlkptestcapability tc
		  GROUP BY tc.testcode, tc.fpsyear
		), project_lookup AS (
		 SELECT DISTINCT ON (tlkpproject.parentproject, tlkpproject.fpsyear) tlkpproject.parentproject,
			tlkpproject.program,
			tlkpproject.fpsyear
		   FROM fps.tlkpproject
		  ORDER BY tlkpproject.parentproject, tlkpproject.fpsyear
		), test_description_lookup AS (
		 SELECT DISTINCT ON (testorproduct.itemcode, testorproduct.fpsyear) testorproduct.itemcode,
			testorproduct.shortdescription,
			testorproduct.fpsyear
		   FROM fps.testorproduct
		  ORDER BY testorproduct.itemcode, testorproduct.fpsyear
		), testrccost_lookup AS (
		 SELECT DISTINCT ON (tbltestrccost.testcode, tbltestrccost.profitcentre, tbltestrccost.fpsyear) tbltestrccost.testcode,
			tbltestrccost.profitcentre,
			tbltestrccost.price,
			tbltestrccost.fpsyear
		   FROM fps.tbltestrccost
		  ORDER BY tbltestrccost.testcode, tbltestrccost.profitcentre, tbltestrccost.fpsyear
		), workgroup_lookup AS (
		 SELECT DISTINCT ON (workgroup.workgroup, workgroup.fpsyear) workgroup.workgroup,
			workgroup.profitcentre,
			workgroup.fpsyear
		   FROM fps.workgroup
		  ORDER BY workgroup.workgroup, workgroup.fpsyear
		), qry_test_req_breakdown AS (
		 SELECT tr.testcode,
			td.shortdescription,
			p.program,
			tr.buyer AS jobcode,
			COALESCE(trwg.workgroup, dw.wg::character varying) AS workg,
			trc.profitcentre,
			COALESCE(trc.price, tr.unitprice) AS wgprice,
			COALESCE(trwg.amount::double precision, tr.norequired) AS noreq,
			tr.fpsyear
		   FROM fps.tlkptestreqmt tr
			 JOIN project_lookup p ON p.parentproject::text = tr.buyer::text AND p.fpsyear = tr.fpsyear
			 JOIN default_workgroup dw ON dw.testcode::text = tr.testcode::text AND dw.fpsyear = tr.fpsyear
			 LEFT JOIN fps.tbltestreqwg trwg ON trwg.testcode::text = tr.testcode::text AND trwg.buyer::text = tr.buyer::text AND trwg.fpsyear = tr.fpsyear
			 LEFT JOIN testrccost_lookup trc ON trc.testcode::text = tr.testcode::text AND trc.fpsyear = tr.fpsyear
			 JOIN test_description_lookup td ON td.itemcode::text = tr.testcode::text AND td.fpsyear = tr.fpsyear
		)
 SELECT q.testcode,
	q.shortdescription,
	q.program,
	q.jobcode AS project,
	COALESCE(q.profitcentre, wg.profitcentre) AS pc,
	q.workg,
	q.wgprice,
	q.noreq::numeric * q.wgprice::numeric AS totalcost,
	q.fpsyear
   FROM qry_test_req_breakdown q
	 LEFT JOIN workgroup_lookup wg ON wg.workgroup::text = q.workg::text AND q.fpsyear = wg.fpsyear;
