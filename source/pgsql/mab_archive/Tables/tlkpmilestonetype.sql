-- Table: mabarchive.tlkpmilestonetype
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpmilestonetype; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpmilestonetype (
    idtype character(1) NOT NULL COLLATE pg_catalog."und-x-icu",
    type character varying(50) COLLATE pg_catalog."und-x-icu",
    milestonedeliverable character(1) COLLATE pg_catalog."und-x-icu"
);
-- Name: tlkpmilestonetype pk_tlkpmilestonetype; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpmilestonetype
    ADD CONSTRAINT pk_tlkpmilestonetype PRIMARY KEY (idtype);
