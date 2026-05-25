-- Table: mabarchive.my_tblcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tblcontract; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tblcontract (
    year smallint NOT NULL,
    contractno character varying(10) NOT NULL,
    category character varying(20) NOT NULL,
    manager character varying(50),
    customer character varying(50),
    title character varying(100),
    registereddate date,
    startdate date,
    enddate date,
    contractdoc bytea,
    duration integer
);
-- Name: my_tblcontract pk_my_tblcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblcontract
    ADD CONSTRAINT pk_my_tblcontract PRIMARY KEY (year, contractno);
