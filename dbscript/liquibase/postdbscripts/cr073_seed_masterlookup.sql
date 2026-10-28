--liquibase formatted sql

--changeset repo-admin:CR073_seed labels:dml context:all
--comment: DML extracted from changesets/cr073_create_masterlookup_and_directorate.sql. Reference data for fps.tblmasterlookup, independent of the directorate FK work in that changeset.

delete FROM fps.tblmasterlookup;

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Disease');

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Customer');

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Directorate');

--ROLLBACK
--DELETE FROM fps.tblmasterlookup
--WHERE mastertablename IN ('Disease', 'Customer', 'Directorate');
