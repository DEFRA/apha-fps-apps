-- Table: mabarchive.my_tlkpprogram
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tlkpprogram; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tlkpprogram (
    year smallint NOT NULL,
    programno character varying(10) NOT NULL COLLATE pg_catalog."und-x-icu",
    programname character varying(80) COLLATE pg_catalog."und-x-icu",
    directorate character varying(15) COLLATE pg_catalog."und-x-icu",
    minim character varying(7) COLLATE pg_catalog."und-x-icu",
    sector_name character varying(50) COLLATE pg_catalog."und-x-icu",
    customer character varying(50) COLLATE pg_catalog."und-x-icu",
    target money,
    manager character varying(50) COLLATE pg_catalog."und-x-icu"
);
-- Name: my_tlkpprogram pk_my_tlkpprogram; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tlkpprogram
    ADD CONSTRAINT pk_my_tlkpprogram PRIMARY KEY (year, programno);
