--liquibase formatted sql

--changeset abhijeet:CR073 labels:ddl context:all splitStatements:false

CREATE TABLE IF NOT EXISTS fps.tblmasterlookup ( mastertablename varchar(100) NOT NULL, 
CONSTRAINT pk_tblmasterlookup PRIMARY KEY (mastertablename));

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Disease');

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Customer');

INSERT INTO fps.tblmasterlookup (mastertablename) VALUES
    ('Directorate');


CREATE TABLE IF NOT EXISTS fps.tbldirectorate
(
    directorate character varying(15) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT pk_tbldirectorate PRIMARY KEY (directorate)
);

INSERT INTO fps.tbldirectorate (directorate)
SELECT DISTINCT directorate
FROM fps.tlkpprogram
WHERE directorate IS NOT NULL
  AND btrim(directorate) <> ''
ON CONFLICT (directorate) DO NOTHING;


DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conname = 'fk_tlkpprogram_directorate'
          AND conrelid = 'fps.tlkpprogram'::regclass
    ) THEN
       ALTER TABLE fps.tlkpprogram
    		ADD CONSTRAINT fk_tlkpprogram_directorate FOREIGN KEY (directorate)
        	REFERENCES fps.tbldirectorate (directorate) MATCH SIMPLE
        	ON UPDATE NO ACTION
        	ON DELETE NO ACTION;
    END IF;
END $$;

--ROLLBACK
--ALTER TABLE fps.tlkpprogram DROP CONSTRAINT IF EXISTS fk_tlkpprogram_directorate;
--DROP TABLE IF EXISTS fps.tbldirectorate;
--DELETE FROM fps.tblmasterlookup
--WHERE mastertablename IN ('tbldisease', 'tlkpcustomer', 'tbldirectorate');
--DROP TABLE IF EXISTS fps.tblmasterlookup;
