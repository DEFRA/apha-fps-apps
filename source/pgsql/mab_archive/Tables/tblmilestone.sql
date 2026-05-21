-- Table: mabarchive.tblmilestone
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblmilestone; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblmilestone (
    project character varying(20) NOT NULL,
    number character varying(10) NOT NULL,
    description character varying(500),
    datedue timestamp without time zone NOT NULL,
    datecompleted timestamp without time zone,
    dateformreceived timestamp without time zone,
    undersdreview smallint DEFAULT 0,
    ontarget smallint DEFAULT 0,
    projectleadercomment text,
    capscomment character varying(250),
    idtype character(1)
);
-- Name: tblmilestone pk_tblmilestone; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblmilestone
    ADD CONSTRAINT pk_tblmilestone PRIMARY KEY (project, number);
