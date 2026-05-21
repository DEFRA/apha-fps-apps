-- Table: mabarchive.my_tlkpprojectradtrackdata
-- Extracted from: fps-development.c7kkusgy4aqn.eu-west-2.rds.amazonaws.com/FPS [aws-rds-development]

-- Name: my_tlkpprojectradtrackdata; Type: TABLE; Schema: mabarchive; Owner: -
CREATE TABLE mabarchive.my_tlkpprojectradtrackdata (
    year smallint NOT NULL,
    project character varying(20) NOT NULL COLLATE pg_catalog."und-x-icu",
    bfbudget money,
    pybudget money,
    seedcorn money,
    manhours double precision,
    mandays double precision,
    manyears double precision,
    paycosts money,
    nonpayohcosts money,
    testcosts money,
    animalcosts money,
    nonanimalcosts money,
    manhourschanged smallint DEFAULT 0,
    paycostschanged smallint DEFAULT 0,
    nonpayohcostschanged smallint DEFAULT 0,
    testcostschanged smallint DEFAULT 0,
    animalcostschanged smallint DEFAULT 0,
    nonanimalcostschanged smallint DEFAULT 0,
    adjustment money,
    adjustmentcomment character varying(250) COLLATE pg_catalog."und-x-icu",
    locked smallint DEFAULT 0,
    datecosted timestamp without time zone,
    costedby character varying(20) COLLATE pg_catalog."und-x-icu",
    actualexpenditure money,
    actualmanyears double precision,
    vla_budget money
);
-- Name: my_tlkpprojectradtrackdata pk_my_tlkpprojectradtrackdata; Type: CONSTRAINT; Schema: mabarchive; Owner: -
ALTER TABLE ONLY mabarchive.my_tlkpprojectradtrackdata
    ADD CONSTRAINT pk_my_tlkpprojectradtrackdata PRIMARY KEY (year, project);
