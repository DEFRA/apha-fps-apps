-- Table: mabarchive.tblmaintenance
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblmaintenance; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblmaintenance (
    formname character varying(50) NOT NULL,
    description character varying(50),
    usernotes character varying(250),
    "obsolete?" boolean NOT NULL,
    displayseq integer
);
-- Name: tblmaintenance pk_tblmaintenance; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblmaintenance
    ADD CONSTRAINT pk_tblmaintenance PRIMARY KEY (formname);
