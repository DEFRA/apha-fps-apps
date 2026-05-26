-- Table: mabarchive.tblaccessprograms
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblaccessprograms; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblaccessprograms (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL COLLATE pg_catalog."und-x-icu",
    program character varying(10) NOT NULL COLLATE pg_catalog."und-x-icu"
);
-- Name: tblaccessprograms pk_tblaccessprograms; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblaccessprograms
    ADD CONSTRAINT pk_tblaccessprograms PRIMARY KEY (systemid, ntlogin, program);
