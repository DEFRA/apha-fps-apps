-- Table: mabarchive.tblreport
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: tblreport; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.tblreport (
    id integer NOT NULL,
    reportname character varying(50) NOT NULL,
    reportdescription character varying(50),
    filter character varying(200),
    mailcomment character varying(250),
    mailtitle character varying(50),
    emailable boolean NOT NULL,
    sortorder integer,
    allowpickprogramme boolean NOT NULL,
    allowpickproject boolean NOT NULL,
    allowpickmanager boolean NOT NULL,
    allowpickcontract boolean NOT NULL,
    allowpickcustomer boolean NOT NULL,
    allowpickmonth boolean NOT NULL,
    allowpickfyear boolean NOT NULL,
    reporthelp character varying(250),
    type character(1) NOT NULL
);
-- Name: tblreport pk_tblreport; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.tblreport
    ADD CONSTRAINT pk_tblreport PRIMARY KEY (id);
