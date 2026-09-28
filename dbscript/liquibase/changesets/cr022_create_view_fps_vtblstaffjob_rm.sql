--liquibase formatted sql

--changeset repo-admin:CR022 labels:ddl context:all

CREATE OR REPLACE VIEW fps.vtblstaffjob_rm
 AS
 SELECT staffid,
    jobcode,
    plannedhours,
	fpsyear
   FROM fps.tblstaffjob
  WHERE (staffid::text IN ( SELECT vtblwgemployee.pactid
           FROM fps.vtblwgemployee));


--ROLLBACK
--Not Applicable
