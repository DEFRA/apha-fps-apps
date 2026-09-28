--liquibase formatted sql

--changeset repo-admin:CR059 labels:ddl context:all runOnChange:true

-- View: mabarchive.vmy_projectanimalplan

DROP VIEW IF EXISTS mabarchive.vmy_projectanimalplan;

CREATE OR REPLACE VIEW mabarchive.vmy_projectanimalplan
 AS
 SELECT my_tlkpproject.year,
    my_tlkpproject.parentproject,
    my_tblanimalreq.animaltype,
    my_tblanimalreq.numberofdays,
    my_tblanimalreq.numberofanimals,
        CASE
            WHEN my_tlkpproject.isdefraproject <> 0 AND my_tlkpproject.year >= 2013 THEN my_tblanimals.defradailyrate
            ELSE my_tblanimals.dailyrate
        END AS rate,
        CASE
            WHEN my_tlkpproject.isdefraproject <> 0 AND my_tlkpproject.year >= 2013 THEN my_tblanimals.defradailyrate
            ELSE my_tblanimals.dailyrate
        END * (my_tblanimalreq.numberofdays * my_tblanimalreq.numberofanimals)::numeric AS cost
   FROM mabarchive.my_tlkpproject
     JOIN mabarchive.my_tblanimalreq ON my_tlkpproject.year = my_tblanimalreq.year AND my_tlkpproject.parentproject::text = my_tblanimalreq.jobcode::text
     JOIN mabarchive.my_tblanimals ON my_tblanimalreq.year = my_tblanimals.year AND my_tblanimalreq.animaltype::text = my_tblanimals.animaltype::text;

--ROLLBACK
--DROP VIEW IF EXISTS mabarchive.vmy_projectanimalplan;