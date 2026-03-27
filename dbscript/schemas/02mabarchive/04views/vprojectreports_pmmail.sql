-- View: mabarchive.vprojectreports_pmmail

CREATE OR REPLACE VIEW mabarchive.vprojectreports_pmmail AS
 SELECT (((((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (vcurrent_projectinfo.projectgroup)::text) || '/'::text) || replace((vcurrent_projectinfo.parentproject)::text, '/'::text, '-'::text)) || ' '::text) || (prepname.setting)::text) || '" target="_blank">'::text) || (vcurrent_projectinfo.parentproject)::text) || '</a><br> '::text) AS hlink,
        CASE
            WHEN ((tblprojectmanager.projectmanager)::text ~~ '%,%'::text) THEN (((SUBSTRING(tblprojectmanager.projectmanager FROM (POSITION((','::text) IN (tblprojectmanager.projectmanager)) + 2) FOR 50) || ' '::text) || "left"((tblprojectmanager.projectmanager)::text, (POSITION((','::text) IN (tblprojectmanager.projectmanager)) - 1))))::character varying
            ELSE tblprojectmanager.projectmanager
        END AS projectmanager,
    tblprojectmanager.mnumber,
    vcurrent_projectinfo.parentproject,
    tblprojectmanager.email,
    ((((('<a href="'::text || (editroot.setting)::text) || (vcurrent_projectinfo.parentproject)::text) || '">'::text) || (vcurrent_projectinfo.parentproject)::text) || '</a><br>'::text) AS editlink,
    vcurrent_projectinfo.projectgroup,
    vcurrent_projectinfo.year,
    tblprojectmanager.disable
   FROM ((((mabarchive.tblradtrackprog
     JOIN (mabarchive.tblprojectmanager
     JOIN mabarchive.vcurrent_projectinfo ON (((tblprojectmanager.projectmanager)::text = (vcurrent_projectinfo.manager)::text))) ON (((tblradtrackprog.program)::text = (vcurrent_projectinfo.program)::text)))
     CROSS JOIN mabarchive.tbl_settings prepname)
     CROSS JOIN mabarchive.tbl_settings root)
     CROSS JOIN mabarchive.tbl_settings editroot)
  WHERE (((root.id)::text = 'PIMS_Project_Current_Root'::text) AND ((prepname.id)::text = 'PIMS_Project_Report_Name'::text) AND ((editroot.id)::text = 'PIMS_Project_Edit_Link'::text) AND (tblradtrackprog.radtrackprog = true) AND ((vcurrent_projectinfo.projectstatus)::text <> 'Completed'::text) AND ((vcurrent_projectinfo.projectgroup)::text <> ALL (ARRAY[('EU_PROG'::character varying)::text, ('ZT_Prog'::character varying)::text])));
