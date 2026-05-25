-- Table: mabarchive.tblradtrackcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblradtrackcontract; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblradtrackcontract (
    contract character varying(10) NOT NULL
);
-- Name: tblradtrackcontract pk_tblradtrackcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblradtrackcontract
    ADD CONSTRAINT pk_tblradtrackcontract PRIMARY KEY (contract);
