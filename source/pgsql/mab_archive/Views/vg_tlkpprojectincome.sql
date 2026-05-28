CREATE OR REPLACE VIEW mabarchive.vg_tlkpprojectincome AS
 SELECT vmy_projectcustincome.project,
    COALESCE(g_tlkpproject_radtrackdata.overallcustincome, sum(vmy_projectcustincome.custinc)) AS totalprojectvalue
   FROM mabarchive.vmy_projectcustincome
     LEFT JOIN mabarchive.g_tlkpproject_radtrackdata ON vmy_projectcustincome.project::text = g_tlkpproject_radtrackdata.parentproject::text
  GROUP BY vmy_projectcustincome.project, g_tlkpproject_radtrackdata.overallcustincome;
