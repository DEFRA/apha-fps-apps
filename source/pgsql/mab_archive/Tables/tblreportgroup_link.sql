-- Table: mabarchive.tblreportgroup_link
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblreportgroup_link; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblreportgroup_link (
    reportid integer NOT NULL,
    groupid integer NOT NULL
);
-- Name: tblreportgroup_link pk_tblreportgroup_link; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblreportgroup_link
    ADD CONSTRAINT pk_tblreportgroup_link PRIMARY KEY (reportid, groupid);
