-- View: mabarchive.vg_tlkpproject

CREATE OR REPLACE VIEW mabarchive.vg_tlkpproject AS
 SELECT g_tlkpproject.parentproject,
    g_tlkpproject.projecttitle,
    g_tlkpproject.costbookno,
    g_tlkpproject.disease,
    g_tlkpproject.contract,
    g_tlkpproject.shorttitle,
    g_tlkpproject.projectstatus,
    vcurrent_tlkpprojectradtrackdata.bfbudget AS currentbfbudget
   FROM (mabarchive.g_tlkpproject
     LEFT JOIN mabarchive.vcurrent_tlkpprojectradtrackdata ON (((g_tlkpproject.parentproject)::text = (vcurrent_tlkpprojectradtrackdata.project)::text)));
