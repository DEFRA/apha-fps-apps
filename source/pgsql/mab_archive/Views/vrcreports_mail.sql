CREATE OR REPLACE VIEW mabarchive.vrcreports_mail AS
 SELECT my_tblprofitcentre.profitcentre,
    ((((((((('<a href="'::text || root.setting::text) || '/'::text) || my_tblprofitcentre.profitcentre::text) || ' '::text) || rcrepname1.setting::text) || '">'::text) || my_tblprofitcentre.profitcentre::text) || ' '::text) || rcrepname1.setting::text) || '</a><br> '::text AS hlink1,
    ((((((((('<a href="'::text || root.setting::text) || '/'::text) || my_tblprofitcentre.profitcentre::text) || ' '::text) || rcrepname2.setting::text) || '">'::text) || my_tblprofitcentre.profitcentre::text) || ' '::text) || rcrepname2.setting::text) || '</a><br> '::text AS hlink2,
        CASE
            WHEN tblprojectmanager.projectmanager::text ~~ '%,%'::text THEN ((SUBSTRING(tblprojectmanager.projectmanager FROM POSITION((','::text) IN (tblprojectmanager.projectmanager)) + 2 FOR 50) || ' '::text) || SUBSTRING(tblprojectmanager.projectmanager FROM 1 FOR POSITION((','::text) IN (tblprojectmanager.projectmanager)) - 1))::character varying
            ELSE tblprojectmanager.projectmanager
        END AS projectmanager,
    tblprojectmanager.mnumber,
    tblprojectmanager.email,
    tblprojectmanager.disable
   FROM mabarchive.vlatestmonthyear
     JOIN mabarchive.my_tblprofitcentre ON vlatestmonthyear.year = my_tblprofitcentre.year
     JOIN mabarchive.tblprofitcentre_manager_link ON my_tblprofitcentre.profitcentre::text = tblprofitcentre_manager_link.profitcentre::text
     JOIN mabarchive.tblprojectmanager ON tblprofitcentre_manager_link.manager::text = tblprojectmanager.projectmanager::text
     CROSS JOIN mabarchive.tbl_settings rcrepname1
     CROSS JOIN mabarchive.tbl_settings rcrepname2
     CROSS JOIN mabarchive.tbl_settings root
  WHERE root.id::text = 'PIMS_RC_Current_Root'::text AND rcrepname1.id::text = 'PIMS_RC_Report_Name1'::text AND rcrepname2.id::text = 'PIMS_RC_Report_Name2'::text;
