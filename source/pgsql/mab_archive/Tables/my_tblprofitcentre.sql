-- Table: mabarchive.my_tblprofitcentre
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tblprofitcentre; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tblprofitcentre (
    year smallint NOT NULL,
    profitcentre character varying(50) NOT NULL,
    profitcentrename character varying(40) NOT NULL,
    division character varying(10) NOT NULL,
    conttarget money,
    profitcentrehead character varying(50),
    divisionid integer
);
-- Name: my_tblprofitcentre pk__tblkpprofitcentr__1db06a4f; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tblprofitcentre
    ADD CONSTRAINT pk__tblkpprofitcentr__1db06a4f PRIMARY KEY (year, profitcentre);
