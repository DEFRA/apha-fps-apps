-- Table: mabarchive.tlkprisk
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tlkprisk; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tlkprisk (
    riskid integer NOT NULL,
    riskrating character varying(15) NOT NULL
);
-- Name: tlkprisk pk_tlkprisk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tlkprisk
    ADD CONSTRAINT pk_tlkprisk PRIMARY KEY (riskid);
