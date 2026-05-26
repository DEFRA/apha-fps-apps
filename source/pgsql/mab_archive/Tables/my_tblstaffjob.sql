-- Table: mabarchive.my_tblstaffjob
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tblstaffjob; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tblstaffjob (
    year smallint NOT NULL,
    staffid character varying(50) NOT NULL,
    jobcode character varying(20) NOT NULL,
    plannedhours double precision NOT NULL,
    systimestamp bytea
);
-- Name: my_tblstaffjob pk_my_tblstaffjob; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblstaffjob
    ADD CONSTRAINT pk_my_tblstaffjob PRIMARY KEY (year, staffid, jobcode);
