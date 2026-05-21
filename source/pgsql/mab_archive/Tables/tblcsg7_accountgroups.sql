-- Table: mabarchive.tblcsg7_accountgroups
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblcsg7_accountgroups; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblcsg7_accountgroups (
    csg7group character varying(15) NOT NULL,
    useinflation boolean DEFAULT true
);
-- Name: tblcsg7_accountgroups aaaaatblcsg7_accountgroups_pk; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblcsg7_accountgroups
    ADD CONSTRAINT aaaaatblcsg7_accountgroups_pk PRIMARY KEY (csg7group);
