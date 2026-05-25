-- Table: mabarchive.tlkpfrequency
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkpfrequency; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkpfrequency (
    frequencyid integer NOT NULL,
    frequency character varying(50)
);
-- Name: tlkpfrequency pk_tlkpfrequency; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkpfrequency
    ADD CONSTRAINT pk_tlkpfrequency PRIMARY KEY (frequencyid);
