-- View: mabarchive.vprojectreports_programmmail

CREATE OR REPLACE VIEW mabarchive.vprojectreports_programmmail AS
 SELECT (((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (sq.program)::text) || '_'::text) || (prepname.setting)::text) || '">'::text) || (sq.program)::text) || '</a><br> '::text) AS hlink,
        CASE
            WHEN ((tblprojectmanager.projectmanager)::text ~~ '%,%'::text) THEN (((SUBSTRING(tblprojectmanager.projectmanager FROM (POSITION((','::text) IN (tblprojectmanager.projectmanager)) + 2) FOR 50) || ' '::text) || "left"((tblprojectmanager.projectmanager)::text, (POSITION((','::text) IN (tblprojectmanager.projectmanager)) - 1))))::character varying
            ELSE tblprojectmanager.projectmanager
        END AS projectmanager,
    tblprojectmanager.mnumber,
    tblprojectmanager.email,
    sq.program,
    sq.year,
    tblprojectmanager.disable
   FROM (((((mabarchive.tblprogram_manager_link
     JOIN mabarchive.tblradtrackprog ON (((tblprogram_manager_link.program)::text = (tblradtrackprog.program)::text)))
     JOIN ( SELECT vcurrent_projectinfo.year,
                CASE
                    WHEN ((vcurrent_projectinfo.projectgroup)::text = 'SCN_RES'::text) THEN vcurrent_projectinfo.projectgroup
                    ELSE vcurrent_projectinfo.program
                END AS program
           FROM mabarchive.vcurrent_projectinfo
          WHERE ((vcurrent_projectinfo.projectstatus)::text <> 'Completed'::text)) sq ON (((tblradtrackprog.program)::text = (sq.program)::text)))
     JOIN mabarchive.tblprojectmanager ON (((tblprogram_manager_link.manager)::text = (tblprojectmanager.projectmanager)::text)))
     CROSS JOIN mabarchive.tbl_settings root)
     CROSS JOIN mabarchive.tbl_settings prepname)
  WHERE (((prepname.id)::text = 'PIMS_Program_Report_Name'::text) AND (tblradtrackprog.radtrackprog = true) AND ((root.id)::text = 'PIMS_Program_Current_Root'::text))
  GROUP BY (((((((('<a href="'::text || (root.setting)::text) || '/'::text) || (sq.program)::text) || '_'::text) || (prepname.setting)::text) || '">'::text) || (sq.program)::text) || '</a><br> '::text), tblprojectmanager.mnumber, tblprojectmanager.email, sq.program, sq.year, tblprojectmanager.projectmanager, tblprojectmanager.disable
 HAVING ((sq.program)::text <> 'ZT_Prog'::text);
