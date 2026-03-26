-- View: mabarchive.vprojectlatestdetails

CREATE OR REPLACE VIEW mabarchive.vprojectlatestdetails AS
 SELECT g_tlkpproject.parentproject,
    my_tlkpproject.program,
    my_tlkpproject.manager,
    g_tlkpproject.projecttitle,
    g_tlkpproject.shorttitle,
    my_tlkpproject.customer,
    vlatestprojectyear.year AS lastyear,
        CASE
            WHEN (vlatestprojectyear.year = ( SELECT max(tlkpyear.year) AS max
               FROM mabarchive.tlkpyear)) THEN 'Y'::text
            ELSE 'N'::text
        END AS active,
    my_tlkpproject.projectgroup
   FROM ((mabarchive.g_tlkpproject
     JOIN mabarchive.vlatestprojectyear ON (((g_tlkpproject.parentproject)::text = (vlatestprojectyear.parentproject)::text)))
     JOIN mabarchive.my_tlkpproject ON (((my_tlkpproject.year = vlatestprojectyear.year) AND ((my_tlkpproject.parentproject)::text = (vlatestprojectyear.parentproject)::text))));
