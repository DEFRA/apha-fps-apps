-- Table: mabarchive.tblradtrackprog
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblradtrackprog; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblradtrackprog (
    program character varying(10) NOT NULL,
    radtrackprog boolean NOT NULL,
    publicationprefix character varying(5)
);
-- Name: tblradtrackprog pk_tblradtrackprog; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblradtrackprog
    ADD CONSTRAINT pk_tblradtrackprog PRIMARY KEY (program);
