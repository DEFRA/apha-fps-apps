-- Table: mabarchive.tblcapsstaff
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcapsstaff; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblcapsstaff (
    mnumber character varying(50) NOT NULL,
    name character varying(50) NOT NULL,
    dt2number character varying(50)
);
-- Name: tblcapsstaff pk_tblcapsstaff; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblcapsstaff
    ADD CONSTRAINT pk_tblcapsstaff PRIMARY KEY (mnumber);
