-- Table: mabarchive.tbleugrade_conversion
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tbleugrade_conversion; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tbleugrade_conversion (
    vlagrade character varying(50) NOT NULL,
    eugrade character varying(50)
);
-- Name: tbleugrade_conversion pk_tbleugrade_conversion; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tbleugrade_conversion
    ADD CONSTRAINT pk_tbleugrade_conversion PRIMARY KEY (vlagrade);
