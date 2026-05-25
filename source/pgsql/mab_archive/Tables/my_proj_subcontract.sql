-- Table: mabarchive.my_proj_subcontract
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_proj_subcontract; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_proj_subcontract (
    year smallint NOT NULL,
    subcontcounter integer NOT NULL,
    project character varying(20),
    testjob character varying(50),
    month real,
    amount money,
    workgroup character varying(50),
    acctcode character varying(30),
    supplier character varying(50),
    description character varying(255),
    suppliernumber integer,
    dailyrate money,
    animaldays integer
);
-- Name: my_proj_subcontract pk_my_proj_subcontract; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_proj_subcontract
    ADD CONSTRAINT pk_my_proj_subcontract PRIMARY KEY (year, subcontcounter);
